using System.Text;

namespace MiniHTTPServer
{
    public class ReplyHandler
    {
        /// <returns>The bytes to send to the client.</returns>
        public static byte[] HandleReply(string requestHeader, string rootFolder, out string replyHeader)
        {
            HttpRequest? request = HttpRequest.Parse(requestHeader);
            HttpReply reply = request switch
            {
                null => HttpReply.Text("400 Bad Request"),
                { Method: "GET" or "HEAD" } => StaticFileHandler.Serve(rootFolder, request.Path),
                _ => HttpReply.Text("405 Method Not Allowed", "Allow: GET, HEAD\r\n"),
            };

            // HTTP requires CRLF line endings in the header, and Content-Length counts bytes (not chars).
            // We close the connection after each response, so tell the client not to reuse it.
            replyHeader =
                $"HTTP/1.1 {reply.Status}\r\n" +
                $"Date: {DateTime.UtcNow:r}\r\n" +
                "Connection: close\r\n" +
                "Vary: Origin\r\n" +
                "Cache-Control: public, max-age=0\r\n" +
                reply.ExtraHeaders +
                $"Content-Type: {reply.ContentType}\r\n" +
                $"Content-Length: {reply.Body.Length}\r\n" +
                "\r\n";

            // A HEAD reply is a GET reply with the same header but no body.
            byte[] headerData = Encoding.ASCII.GetBytes(replyHeader);
            return request?.Method == "HEAD" ? headerData : [.. headerData, .. reply.Body];
        }
    }
}
