using System.Text;

namespace MiniHTTPServer
{
    /// <summary>
    /// The body is bytes, not a string, because most files (e.g. images) aren't text.
    /// </summary>
    /// <param name="ExtraHeaders">Complete header lines, each ending in CRLF.</param>
    public record HttpReply(string Status, string ContentType, byte[] Body, string ExtraHeaders = "")
    {
        /// <summary>
        /// A reply with no file behind it, where the status itself is a good enough body.
        /// </summary>
        public static HttpReply Text(string status, string extraHeaders = "")
            => new(status, "text/plain; charset=UTF-8", Encoding.UTF8.GetBytes(status), extraHeaders);
    }
}
