using BackupCore;
using Common;
using Communication.Serialization;
using FilesystemModel;
using System;
using System.Net.Sockets;

namespace Communication
{
    public class DestinationSocketCommunicator : SocketCommunicator, IDestinationCommunicator
    {
        public DestinationSocketCommunicator(string address,
            int port,
            int bufferSize,
            ISerialization serialization,
            ILogger logger) : base(address, port, bufferSize, serialization, logger)
        {
            _socket.Bind(_endPoint);
            _socket.Listen(1);
            _logger.Write($"Listening on {_endPoint}");
        }

        public Directory GetDirectory()
        {
            Socket connection = _socket.Accept();
            _socket.Dispose();
            _socket = connection;

            _logger.Write($"Connected with: {connection.RemoteEndPoint.ToString()}");

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
		public uint GetCrc32(string fileRequestPath)
        {
            SendRequest(Request.GET_CRC32);
            Send(fileRequestPath);
            return Receive<uint>();
        }		
        public void Finish()
        {
            SendRequest(Request.FINISH);
        }
        private void ReceiveFile(string filename,
            System.IO.FileAttributes attributes)
        {
            long size = Receive<long>();
            if (size == 0)
                HandleEmptyFile(filename);
            else
                HandleNoEmptyFile(filename, size, attributes);
        }

        private void HandleNoEmptyFile(string filename,
            long size,
            System.IO.FileAttributes attributes)
        {
            long total = 0;
            _logger.MaxProgress = size;

            using (System.IO.Stream stream = SafeFileUsage.GetFile(filename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, _logger))
            {
                System.IO.File.SetAttributes(filename, attributes);
               byte[] buffer = size > _bufferSize ?
                    new byte[_bufferSize] : new byte[size];
                while (total < size)
                {
                    int received = _socket.Receive(buffer);
                    _logger.UpdateProgressBar(received);
                    SendAck();
                    total += received;
                    stream.Write(buffer, 0, received);   
                }
                _logger.ResetProgressBar();
            }
        }

        private void HandleEmptyFile(string filename)
        {
            System.IO.File.Create(filename);
        }

		private void SendRequest(Request request) 
		{
			byte[] content = BitConverter.GetBytes((int)request);
			_socket.Send(content);
		}
        
    }
}
