using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

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

    public static class MessageHandlerAsync
    {

        private static NetworkStream stream;
        private static Action<string> onCompletionMethod;

        public static void SendMessage(NetworkStream stream, string message)
        {
            MessageHandlerAsync.stream = stream;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += SendMessage;
            worker.RunWorkerAsync(message);
        }

        private static void SendMessage(object sender, DoWorkEventArgs e)
        {
            string message = (string)e.Argument;
            MessageHandler.SendMessage(MessageHandlerAsync.stream, message);
        }

        public static void GetMessage(NetworkStream stream, Action<string> method)
        {
            MessageHandlerAsync.stream = stream;
            MessageHandlerAsync.onCompletionMethod = method;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += GetMessage;
            worker.RunWorkerCompleted += OnGetMessageCompletion;
            worker.RunWorkerAsync();
        }

        private static void GetMessage(object sender, DoWorkEventArgs e)
        {
            e.Result = MessageHandler.GetMessage(MessageHandlerAsync.stream);
        }

        private static void OnGetMessageCompletion(object sender, RunWorkerCompletedEventArgs e)
        {
            string message = (string) e.Result;
            onCompletionMethod(message);
        }

    }
}
