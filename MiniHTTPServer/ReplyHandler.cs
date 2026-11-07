using System.Text;

namespace MiniHTTPServer
{
    public class ReplyHandler
    {
        public static void HandleReply(string requestHeader, out string body, out string replyHeader)
        {
            body = $"""
                    <!DOCTYPE html>
                    <html lang="en">

                    <head>
                      <meta charset="utf-8">
                      <meta name="viewport" content="width=device-width, initial-scale=1">
                      <title>HTTP Server Welcome!</title>
                      <link rel="stylesheet" href="styles.css">
                    </head>

                    <body>
                      <h1>Barebone HTTP Server</h1>
                      <p>Hello World!</p>
                      <script src="scripts.js"></script>
                    </body>

                    </html>
                    """;
            // HTTP requires CRLF line endings in the header, and Content-Length counts bytes (not chars).
            // We close the connection after each response, so tell the client not to reuse it.
            replyHeader =
                "HTTP/1.1 200 OK\r\n" +
                $"Date: {DateTime.Now.ToUniversalTime().ToString("r")}\r\n" +
                "Connection: close\r\n" +
                "Vary: Origin\r\n" +
                "Cache-Control: public, max-age=0\r\n" +
                $"Last-Modified: {DateTime.Now.ToUniversalTime():r}\r\n" +
                $"ETag: W/\"{Guid.NewGuid().ToString()[..15]}\"\r\n" +
                "Content-Type: text/html; charset=UTF-8\r\n" +
                $"Content-Length: {Encoding.UTF8.GetByteCount(body)}\r\n" +
                "\r\n";
        }
    }
}