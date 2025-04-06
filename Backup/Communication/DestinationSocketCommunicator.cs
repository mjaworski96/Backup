using BackupCore;
using Common;
using Common.Translations;
using Communication.Serialization;
using FilesystemModel;
using FilesystemModel.Extensions;
using System;
using System.Net.Sockets;

namespace Communication
{
    public class DestinationSocketCommunicator : SocketCommunicator, IDestinationCommunicator
    {
        public DestinationSocketCommunicator(string address,
            int port,
            long bufferSize,
            ISerialization serialization,
            ILogger logger) : base(address, port, bufferSize, serialization, logger)
        {
            _socket.Bind(_endPoint);
            _socket.Listen(1);
            _logger.Write(string.Format(LoggerMessages.ListenOn, _endPoint));
        }

        public Directory GetDirectory()
        {
            return Receive<Directory>();
        }
        public void ReceiveFile(string fileRequestPath,
            string saveFileAs,
            System.IO.FileAttributes attributes)
        {
            SendRequest(Request.GET_FILE);
            Send(fileRequestPath);
            ReceiveFile(saveFileAs, attributes);
        }
        public uint GetCrc32(ChecksumRequest checksumRequest)
        {
            SendRequest(Request.GET_CRC32);
            Send(checksumRequest);
            return Receive<uint>();
        }
        public void Finish()
        {
            SendRequest(Request.FINISH);
        }
        private void ReceiveFile(string filename,
            System.IO.FileAttributes attributes)
        {
            var size = Receive<long>();
            if (size == 0)
                HandleEmptyFile(filename);
            else
                HandleNoEmptyFile(filename, size, attributes);
        }

        private void HandleNoEmptyFile(string filename,
            long size,
            System.IO.FileAttributes attributes)
        {
            var total = 0L;

            using (System.IO.Stream stream = SafeFileUsage.GetFile(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, _logger, true))
            {
                attributes.Set(filename, _logger);
                _logger.Write(string.Format(LoggerMessages.Downloanding, filename));
                _logger.MaxProgress = size * 2; //download and save

                var buffer = size > _bufferSize ?
                     new byte[_bufferSize] : new byte[size];
                while (total < size)
                {
                    var currentBufferReceived = 0L;
                    var bytesLeft = size - total;
                    var currentBufferSize = bytesLeft > buffer.Length ? buffer.Length : bytesLeft;
                    while (currentBufferReceived < currentBufferSize)
                    {
                        int received = _socket.Receive(buffer);
                        _logger.UpdateProgressBar(received);

                        total += received;
                        currentBufferReceived += received;
                        stream.Write(buffer, 0, received);
                        _logger.UpdateProgressBar(received);
                    }
                    SendAck();
                }
            }
        }

        private void HandleEmptyFile(string filename)
        {
            _logger.Write(string.Format(LoggerMessages.Downloanding, filename));
            _logger.MaxProgress = 1;
            System.IO.File.Create(filename).Dispose();
            _logger.UpdateProgressBar(1);
        }

        private void SendRequest(Request request)
        {
           var content = BitConverter.GetBytes((int)request);
            _socket.Send(content);
        }

        public long GetFileSize(string fileRequestPath)
        {
            SendRequest(Request.GET_FILE_SIZE);
            Send(fileRequestPath);
            return ReceiveSize();
        }

        public void Connect()
        {
            var connection = _socket.Accept();
            _socket.Dispose();
            _socket = connection;
            _logger.Write(string.Format(LoggerMessages.ConnectedWith, connection.RemoteEndPoint));
        }

        public void Reset()
        {
            CreateSocket();
            _socket.Bind(_endPoint);
            _socket.Listen(1);
            _logger.Write(string.Format(LoggerMessages.ResetingConnection));
        }

        public ClientConfiguration GetSourceConfiguration()
        {
            return Receive<ClientConfiguration>();
        }

        public void SendConnectionStatus(ConnectionStatus connectionStatus)
        {
            Send(connectionStatus);
        }
    }
}
