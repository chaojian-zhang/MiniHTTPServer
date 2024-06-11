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
                      <title>HTML5 Boilerplate</title>
                      <link rel="stylesheet" href="styles.css">
                    </head>

                    <body>
                      <h1>Page Title</h1>
                      <script src="scripts.js"></script>
                    </body>

                    </html>
                    """;
            replyHeader = $"""
                    HTTP/1.1 200 OK
                    Content-Length: {body.Length}
                    Content-Type: text/plain; charset=utf-8
                    """;
        }
    }
}