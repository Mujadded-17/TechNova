namespace TechNova.Services
{
    /// <summary>
    /// Single source of truth for where uploaded files live on disk.
    ///
    /// Locally these sit inside the project (wwwroot/startup-media and
    /// App_Data), which is convenient but disappears on every container
    /// rebuild. In production <c>Storage:Root</c> points at a mounted
    /// persistent disk instead, so uploads survive a redeploy. The public
    /// URLs are unchanged either way — Program.cs serves the media root at
    /// <see cref="MediaRequestPath"/> regardless of where it physically is.
    /// </summary>
    public class StoragePaths
    {
        /// <summary>URL prefix that startup photos and videos are served from.</summary>
        public const string MediaRequestPath = "/startup-media";

        private readonly IWebHostEnvironment _env;
        private readonly string? _root;

        public StoragePaths(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _root = configuration["Storage:Root"];
        }

        /// <summary>True when uploads are being written outside the app directory.</summary>
        public bool UsesExternalRoot => !string.IsNullOrWhiteSpace(_root);

        /// <summary>Falls back to the content root when wwwroot is absent (published trimmed output).</summary>
        private string WebRoot =>
            string.IsNullOrWhiteSpace(_env.WebRootPath)
                ? Path.Combine(_env.ContentRootPath, "wwwroot")
                : _env.WebRootPath;

        /// <summary>Startup photos and videos.</summary>
        public string MediaRoot =>
            UsesExternalRoot
                ? Path.Combine(_root!, "startup-media")
                : Path.Combine(WebRoot, "startup-media");

        /// <summary>Pitch decks. Never web-served — streamed through an authorized action.</summary>
        public string PitchDeckRoot =>
            UsesExternalRoot
                ? Path.Combine(_root!, "pitch-decks")
                : Path.Combine(_env.ContentRootPath, "App_Data", "pitch-decks");

        /// <summary>Development-only mailbox used when no SMTP host is configured.</summary>
        public string SentEmailRoot =>
            UsesExternalRoot
                ? Path.Combine(_root!, "sent-emails")
                : Path.Combine(_env.ContentRootPath, "App_Data", "sent-emails");

        /// <summary>Creates the directory if needed and returns it.</summary>
        public string EnsureCreated(string directory)
        {
            Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        /// Turns a stored path such as "/startup-media/12_638.jpg" back into an
        /// absolute path. Returns null when the value escapes the media root, so
        /// a tampered database row cannot reach arbitrary files on disk.
        /// </summary>
        public string? ResolveMediaFile(string? storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return null;
            }

            var relative = storedPath.Replace('\\', '/').TrimStart('/');

            // Stored paths carry the request prefix; strip it before joining.
            const string prefix = "startup-media/";
            if (relative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                relative = relative[prefix.Length..];
            }

            if (string.IsNullOrWhiteSpace(relative))
            {
                return null;
            }

            var root = Path.GetFullPath(MediaRoot);
            var full = Path.GetFullPath(Path.Combine(root, relative));

            var rootWithSeparator =
                root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

            return full.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) ? full : null;
        }
    }
}
