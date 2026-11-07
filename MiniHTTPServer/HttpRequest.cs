namespace MiniHTTPServer
{
    /// <summary>
    /// What we need from a request, which is all in its first line, e.g. "GET /css/site.css?v=2 HTTP/1.1".
    /// </summary>
    public record HttpRequest(string Method, string Path)
    {
        /// <returns>The parsed request, or null if the request line is malformed.</returns>
        public static HttpRequest? Parse(string requestHeader)
        {
            string requestLine = requestHeader.Split("\r\n")[0];
            string[] parts = requestLine.Split(' ');
            if (parts.Length != 3 || !parts[1].StartsWith('/') || !parts[2].StartsWith("HTTP/"))
                return null;

            // The query string isn't part of the file's name. Drop it before decoding, because a decoded %3F is a literal '?'.
            string path = parts[1].Split('?')[0];
            return new HttpRequest(parts[0], Uri.UnescapeDataString(path));
        }
    }
}
