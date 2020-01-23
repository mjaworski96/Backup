using BackupLogic;
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
        protected IPEndPoint _endPoint;
        protected ISerialization _serialization;
        protected int _bufferSize;
        protected SocketCommunicator(string address, 
            int port,
            int bufferSize,
            ISerialization serialization)
        {
            if (bufferSize < 1)
                throw new InvalidBufferSizeException("Buffer size must be greater than 0");

            IPAddress ip = IPAddress.Parse(address);
            _endPoint = new IPEndPoint(ip, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _serialization = serialization;
            _bufferSize = bufferSize;
        }

        protected T Receive<T>()
        {
            List<byte> bytes = new List<byte>();
            byte[] buffer = new byte[_bufferSize];
            do
            {
                int received = _socket.Receive(buffer);
                for (int i = 0; i < received; i++)
                {
                    bytes.Add(buffer[i]);
                }
            } while (_socket.Available > 0);
            SendAck();
            return _serialization.Deserialize<T>(bytes.ToArray());
        }
        protected void Send<T>(T obj)
        {
            byte[] content = _serialization.Serialize(obj);
            _socket.Send(content);
			ReceiveAck();
        }
		protected void SendAck() 
		{
			byte[] content = _serialization.Serialize(Message.ACK);
			_socket.Send(content);
		}
		protected void ReceiveAck() 
		{
			byte[] buffer = new byte[1];
			_socket.Receive(buffer, 1, SocketFlags.None);
		}
        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
