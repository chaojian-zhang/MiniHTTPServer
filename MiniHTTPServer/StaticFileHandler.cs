namespace MiniHTTPServer
{
    public static class StaticFileHandler
    {
        private const string DefaultContentType = "application/octet-stream"; // Browsers download what they can't identify

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = "text/html; charset=UTF-8",
            [".css"] = "text/css; charset=UTF-8",
            [".js"] = "text/javascript; charset=UTF-8",
            [".json"] = "application/json",
            [".txt"] = "text/plain; charset=UTF-8",
            [".svg"] = "image/svg+xml",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".ico"] = "image/x-icon",
            [".pdf"] = "application/pdf",
            [".woff2"] = "font/woff2",
        };

        /// <param name="rootFolder">Full path of the folder being served, ending in a directory separator.</param>
        /// <param name="urlPath">The decoded path from the request.</param>
        public static HttpReply Serve(string rootFolder, string urlPath)
        {
            // Never trust the URL: it decides which file we open, so it must not be able to reach anything outside the root.

            // ':' can open a hidden NTFS stream (file.txt::$DATA), and NUL makes GetFullPath throw.
            if (urlPath.IndexOfAny([':', '\0']) != -1)
                return HttpReply.Text("404 Not Found");

            // Use Path.Join, because Path.Combine would throw away the root: a URL path starts with '/', which makes it look absolute.
            // GetFullPath then collapses any "..". Only that final path is checked, so no matter how the ".." was spelled, it's caught here.
            // The root ends in a separator, so a sibling folder like "root-private" doesn't pass as being inside "root".
            string fullPath = Path.GetFullPath(Path.Join(rootFolder, urlPath));
            if (!fullPath.StartsWith(rootFolder, StringComparison.Ordinal))
                return HttpReply.Text("404 Not Found");

            if (Directory.Exists(fullPath))
            {
                // Links in a page are relative to its URL, so "/docs" must become "/docs/" or the links in its index.html would point at the parent folder.
                if (!fullPath.EndsWith(Path.DirectorySeparatorChar))
                    return HttpReply.Text("301 Moved Permanently", $"Location: {Uri.EscapeDataString(Path.GetFileName(fullPath))}/\r\n");

                fullPath = Path.Join(fullPath, "index.html");
            }

            if (!File.Exists(fullPath))
                return HttpReply.Text("404 Not Found");

            try
            {
                byte[] body = File.ReadAllBytes(fullPath);
                DateTime lastModified = File.GetLastWriteTimeUtc(fullPath);
                string extraHeaders =
                    $"Last-Modified: {lastModified:r}\r\n" +
                    $"ETag: W/\"{lastModified.Ticks:x}-{body.Length:x}\"\r\n";
                string contentType = ContentTypes.GetValueOrDefault(Path.GetExtension(fullPath), DefaultContentType);
                return new HttpReply("200 OK", contentType, body, extraHeaders);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                // The file exists but can't be read, e.g. it's locked or we lack permission.
                Console.WriteLine($"Failed to read {fullPath}: {e.Message}");
                return HttpReply.Text("500 Internal Server Error");
            }
        }
    }
}
