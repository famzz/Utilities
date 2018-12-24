using System;
using System.Net.Sockets;
using System.Text;

namespace Utilities
{
    public static class MessageHandler
    {

        public static string GetMessage(NetworkStream stream)
        {
            byte[] lengthData = new byte[4];
            int length = 0;

            stream.Read(lengthData, 0, 4);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthData);

            length = BitConverter.ToInt32(lengthData, 0);

            byte[] data = new byte[length];

            stream.Read(data, 0, length);

            return Encoding.ASCII.GetString(data);
        }

        public static void SendMessage(NetworkStream stream, string message)
        {
            int length = message.Length;

            byte[] intBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] lengthBytes = intBytes;

            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            byte[] data = new byte[lengthBytes.Length + messageBytes.Length];

            Buffer.BlockCopy(lengthBytes, 0, data, 0, lengthBytes.Length);
            Buffer.BlockCopy(messageBytes, 0, data, lengthBytes.Length, messageBytes.Length);

            stream.Write(data, 0, data.Length);
        }
    }
}
