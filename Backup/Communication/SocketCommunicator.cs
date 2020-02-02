using BackupCore;
using Common;
using Communication.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Communication
{
    public abstract class SocketCommunicator: IDisposable
    {
        protected Socket _socket;
        protected readonly IPEndPoint _endPoint;
        protected readonly ISerialization _serialization;
        protected readonly int _bufferSize;
        protected readonly ILogger _logger;
        protected SocketCommunicator(string address, 
            int port,
            int bufferSize,
            ISerialization serialization,
            ILogger logger)
        {
            if (bufferSize < 1)
                throw new InvalidBufferSizeException("Buffer size must be greater than 0");

            IPAddress ip = IPAddress.Parse(address);
            _endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _serialization = serialization;
            _bufferSize = bufferSize;
            _logger = logger;
        }

        protected T Receive<T>()
        {
            List<byte> bytes = new List<byte>();
            byte[] buffer = new byte[_bufferSize];
            long total = 0;
            long size = ReceiveSize();
            do
            {
                total += _socket.Receive(buffer);
                bytes.AddRange(buffer);
            } while (total < size);
            SendAck();
            return _serialization.Deserialize<T>(bytes.ToArray());
        }
        protected void Send<T>(T obj)
        {
            byte[] content = _serialization.Serialize(obj);
            SendSize(content.Length);
            _socket.Send(content);
			ReceiveAck();
        }
		protected void SendAck() 
		{
			byte[] content = BitConverter.GetBytes((int)Message.ACK);
			_socket.Send(content);
		}
		protected void ReceiveAck() 
		{
			byte[] buffer = new byte[sizeof(Message)];
			_socket.Receive(buffer, sizeof(Message), SocketFlags.None);
		}
        protected void SendSize(long size)
        { 
            byte[] content = BitConverter.GetBytes(size);
            _socket.Send(content);
        }
        protected long ReceiveSize()
        {
            byte[] buffer = new byte[sizeof(long)];
            _socket.Receive(buffer, sizeof(long), SocketFlags.None);
            return BitConverter.ToInt64(buffer, 0);
        }
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
