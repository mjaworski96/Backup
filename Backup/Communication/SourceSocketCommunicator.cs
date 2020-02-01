using System;
using BackupCore;
using Communication.Serialization;
using FilesystemModel;
using FilesystemModel.Extensions;
using System.Net.Sockets;

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
            _socket.Connect(_endPoint);
            _logger.Write($"Connecting to {_endPoint}");
        }

        public Request GetRequest()
        {
            byte[] buffer = new byte[1];
			_socket.Receive(buffer, 1, SocketFlags.None);
			return _serialization.Deserialize<Request>(buffer);
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
				//byte[] buffer = new byte[_bufferSize];
                using (System.IO.Stream stream = 
                    new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
				{

                    byte[] buffer = stream.Length > _bufferSize ?
                        new byte[_bufferSize] : new byte[stream.Length];
                    Send(stream.Length);
                    _logger.MaxProgress = stream.Length;
					while(stream.Position != stream.Length)
					{
						int count = stream.Read(buffer, 0, buffer.Length);
                        _logger.UpdateProgress(count);
						_socket.Send(buffer, count, SocketFlags.None);
						ReceiveAck();
					}
                    _logger.ResetProgress();
				}
			}
			catch(Exception)
			{
				Send(0L);
			}
        }
    }
}
