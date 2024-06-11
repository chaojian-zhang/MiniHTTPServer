using System.Net.Sockets;
using System.Text;

namespace PracticeBareboneHTTPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Single argument
            int port = int.Parse(args.Single());
            const int bufferSize = 2048;

            TcpListener server = new(System.Net.IPAddress.Any, port);
            server.Start();

            // Main server loop
            byte[] bytes = new byte[bufferSize]; // Buffer for reading data
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                using TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                // Loop to receive all the data sent by the client.
                StringBuilder sb = new();
                int i;
                NetworkStream stream = client.GetStream();
                while (stream.DataAvailable && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    // Translate data bytes to a ASCII string.
                    string data = Encoding.UTF8.GetString(bytes, 0, i);
                    Console.WriteLine($"Received: {data}");
                    sb.Append(data);
                }

                string requestHeader = sb.ToString().TrimEnd();
                Console.WriteLine($"Complete message: {requestHeader}");

                // Send reply
                byte[] replyData = MakeReply(requestHeader);
                stream.Write(replyData);
            }
        }

        private static byte[] MakeReply(string requestHeader)
        {
            ReplyHandler.HandleReply(requestHeader, out string body, out string replyHeader);
            string reply = $"""
                    {replyHeader}
                    
                    {body}
                    """;
            byte[] replyData = Encoding.UTF8.GetBytes(reply);
            return replyData;
        }
    }
}
