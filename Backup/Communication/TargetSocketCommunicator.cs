﻿using BackupLogic;
using Communication.Serialization;
using FilesystemModel;
using System;
using System.Net;
using System.Net.Sockets;

namespace Communication
{
    public class TargetSocketCommunicator : SocketCommunicator, ITargetCommunicator
    {
        public TargetSocketCommunicator(string address,
            int port,
            ISerialization serialization,
            int bufferSize = 10485760) : base(address, port, serialization, bufferSize)
        {
            _socket.Bind(_endPoint);
            _socket.Listen(1);
        }

        public Directory GetDirectory()
        {
            Socket connection = _socket.Accept();
            _socket.Dispose();
            _socket = connection;

            return Receive<Directory>();
        }
        public void ReceiveFile(string fileRequestPath, string saveFileAs, int attributes)
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
        private void ReceiveFile(string filename, int attributes)
        {
            long size = Receive<long>();
            if (size == 0)
                HandleEmptyFile(filename);
            else
                HandleNoEmptyFile(filename, size, attributes);
        }

        private void HandleNoEmptyFile(string filename, long size, int attributes)
        {
            long total = 0;
            using (System.IO.Stream stream = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate))
            {
                System.IO.File.SetAttributes(filename, (System.IO.FileAttributes) attributes);
                byte[] buffer = new byte[_bufferSize];
                while (total < size)
                {
                    int received = _socket.Receive(buffer);
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