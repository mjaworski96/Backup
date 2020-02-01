using BackupCore;
using Communication.Serialization;
using FilesystemModel;
using System.Net.Sockets;

namespace Communication
{
    public class TargetSocketCommunicator : SocketCommunicator, ITargetCommunicator
    {
        public TargetSocketCommunicator(string address,
            int port,
            int bufferSize,
            ISerialization serialization,
            ILogger logger) : base(address, port, bufferSize, serialization, logger)
        {
            _socket.Bind(_endPoint);
            _socket.Listen(1);
            _logger.Write($"Listening on {_endPoint.Address}:{_endPoint.Port}");
        }

        public Directory GetDirectory()
        {
            Socket connection = _socket.Accept();
            _socket.Dispose();
            _socket = connection;

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

            using (System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate))
            {
                System.IO.File.SetAttributes(filename, attributes);
                byte[] buffer = new byte[_bufferSize];
                while (total < size)
                {
                    int received = _socket.Receive(buffer);
                    _logger.UpdateProgress(received);
                    SendAck();
                    total += received;
                    stream.Write(buffer, 0, received);   
                }
            }
        }

        private void HandleEmptyFile(string filename)
        {
            System.IO.File.Create(filename);
        }

		private void SendRequest(Request request) 
		{
			byte[] content = _serialization.Serialize(request);
			_socket.Send(content);
		}
        
    }
}
