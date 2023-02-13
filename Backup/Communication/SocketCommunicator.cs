using Common;
using Common.Translations;
using Communication.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (bufferSize <= 0)
                throw new InvalidBufferSizeException();
            if (!IPAddress.TryParse(address, out var ip))
            {
                ip = Dns.GetHostEntry(address).AddressList.FirstOrDefault() ?? IPAddress.Parse(address);
            }

            _endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _serialization = serialization;
            _bufferSize = bufferSize;
            _logger = logger;
        }

        protected T Receive<T>()
        {
            MemoryStream bytes = new MemoryStream();
            long totalRead = 0;
            long size = ReceiveSize();
            byte[] buffer = size > _bufferSize ?
                    new byte[_bufferSize] : new byte[size];
            do
            {
                int received = _socket.Receive(buffer);
                totalRead += received;
                bytes.Write(buffer, 0, received);
            } while (totalRead < size);
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
