using System;
using BackupCore;
using Communication.Serialization;
using FilesystemModel;
using System.Net.Sockets;
using Common;
using Common.Translations;

namespace Communication
{
    public class SourceSocketCommunicator : SocketCommunicator, ISourceCommunicator
    {
        public SourceSocketCommunicator(string address,
            int port,
            int bufferSize,
            ISerialization serialization,
            ILogger logger) : base(address, port, bufferSize, serialization, logger)
        {
            _logger.Write(string.Format(LoggerMessages.ConnectingTo, _endPoint));
            _socket.Connect(_endPoint);  
            _logger.Write(string.Format(LoggerMessages.ConnectedWith, _endPoint));
        }

        public Request GetRequest()
        {
            byte[] buffer = new byte[sizeof(Request)];
            _socket.Receive(buffer, SocketFlags.None);
            return (Request)BitConverter.ToInt32(buffer, 0);
        }
        public string GetFilename()
        {
            return Receive<string>();
        }
        public void SendCrc32(uint crc32)
        {
            Send(crc32);
        }

        public void SendDirectory(Directory directory)
        {
            Send(directory);
        }
        public void SendFile(string filename)
        {
            try
            {
                using (System.IO.Stream stream =
                    SafeFileUsage.GetFile(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, _logger))
                {
                    byte[] buffer = stream.Length > _bufferSize ?
                        new byte[_bufferSize] : new byte[stream.Length];
                    Send(stream.Length);
                    _logger.MaxProgress = stream.Length * 2; //read file and upload
                    while (stream.Position != stream.Length)
                    {
                        int count = stream.Read(buffer, 0, buffer.Length);
                        _logger.UpdateProgressBar(count);
                        _socket.Send(buffer, count, SocketFlags.None);
                        _logger.UpdateProgressBar(count);
                        ReceiveAck();
                    }
                }
            }
            catch (Exception)
            {
                Send(0L);
            }
        }
    }
}
