using System.Net.Sockets;
using System.Text;

namespace MiniHTTPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = args.Length != 0 ? int.Parse(args.First()) : 9000;

            TcpListener server = new(System.Net.IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Service address: http://localhost:{port}");

            // Main server loop
            while (true)
            {
                Console.Write("Waiting for a connection... ");

                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected!");

                // Serve each connection on its own task so a slow or idle client (e.g. a browser's speculative preconnect) can't stall the accept loop.
                _ = Task.Run(() => HandleClient(client));
            }
        }

        static void HandleClient(TcpClient client)
        {
            const int bufferSize = 2048;

            using (client)
            {
                client.ReceiveTimeout = 5000;
                NetworkStream stream = client.GetStream();

                // Buffer for reading data
                byte[] bytes = new byte[bufferSize];

                // Loop until the whole request header (terminated by an empty line) has arrived.
                // Note: NetworkStream.DataAvailable only reports what's already buffered and is often still false right after accepting, so it can't be used to decide when the request is complete.
                StringBuilder sb = new();
                int i;
                try
                {
                    while (!sb.ToString().Contains("\r\n\r\n") && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a string.
                        string data = Encoding.UTF8.GetString(bytes, 0, i);
                        Console.WriteLine($"Received: {data}");
                        sb.Append(data);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Client timed out or disconnected before sending a full request.");
                    return;
                }

                string request = sb.ToString().TrimEnd();
                Console.WriteLine($"Complete message: {request}");
                if (request.Length == 0)
                    return; // Client closed without sending anything

                // Send reply
                byte[] replyData = MakeReply(request, out string _, out string replyHeader);
                stream.Write(replyData);
                Console.WriteLine($"Reply header: {replyHeader}");

                // Close gracefully: signal end of response, then drain whatever the client still sends.
                // Closing a socket with unread incoming data makes the OS send a TCP RST instead of a FIN, which browsers report as a connection reset and may discard the response.
                client.Client.Shutdown(SocketShutdown.Send);
                try
                {
                    while (stream.Read(bytes, 0, bytes.Length) != 0) { }
                }
                catch (IOException) { }
            }
        }

        private static byte[] MakeReply(string requestHeader, out string body, out string replyHeader)
        {
            ReplyHandler.HandleReply(requestHeader, out body, out replyHeader);
            byte[] replyData = Encoding.UTF8.GetBytes(replyHeader + body);
            return replyData;
        }
    }
}
