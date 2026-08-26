using TechNova.Models;

namespace TechNova.Services
{
    /// <summary>
    /// Stores uploaded pitch decks on disk.
    ///
    /// Files live in App_Data/pitch-decks — deliberately outside wwwroot,
    /// so nothing uploaded can ever be requested directly by URL. Every
    /// read goes through PitchDeckController.Download, which applies
    /// access control first.
    ///
    /// The name on disk is generated. The uploader's filename is stored
    /// only as a display label, so a crafted name like "../../web.config"
    /// can never influence the write path.
    /// </summary>
    public class PitchDeckStorage
    {
        public const long MaxBytes = 15 * 1024 * 1024;   // 15 MB

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<PitchDeckStorage> _logger;

        public PitchDeckStorage(IWebHostEnvironment env, ILogger<PitchDeckStorage> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>Extension → content type. Anything absent is rejected.</summary>
        private static readonly Dictionary<string, string> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".ppt"] = "application/vnd.ms-powerpoint",
            [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
        };

        public static string AllowedDescription => "PDF, PPT or PPTX, up to 15 MB";

        private string Root => Path.Combine(_env.ContentRootPath, "App_Data", "pitch-decks");


        public record Validation(bool Ok, string? Error, string ContentType = "");

        /// <summary>
        /// Checked before anything touches disk: presence, size, extension,
        /// and — for PDFs — the actual file signature, so renaming an .exe
        /// to .pdf does not get through.
        /// </summary>
        public static Validation Validate(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return new Validation(false, "Choose a file to upload.");
            }

            if (file.Length > MaxBytes)
            {
                return new Validation(false,
                    $"That file is {file.Length / 1024 / 1024} MB. The limit is 15 MB.");
            }

            var ext = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(ext) || !Allowed.TryGetValue(ext, out var contentType))
            {
                return new Validation(false, $"Unsupported file type. Upload a {AllowedDescription}.");
            }

            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) && !LooksLikePdf(file))
            {
                return new Validation(false, "That file isn't a valid PDF.");
            }

            return new Validation(true, null, contentType);
        }


        private static bool LooksLikePdf(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                Span<byte> header = stackalloc byte[5];

                if (stream.Read(header) < 5) return false;

                // "%PDF-"
                return header[0] == 0x25 && header[1] == 0x50 &&
                       header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>Writes the file under a generated name and returns that name.</summary>
        public async Task<string> SaveAsync(IFormFile file)
        {
            Directory.CreateDirectory(Root);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var stored = $"{Guid.NewGuid():N}{ext}";

            var full = Path.Combine(Root, stored);

            await using (var target = File.Create(full))
            {
                await file.CopyToAsync(target);
            }

            _logger.LogInformation("Stored pitch deck as {Stored} ({Bytes} bytes).", stored, file.Length);

            return stored;
        }


        /// <summary>
        /// Resolves a stored name to a full path, refusing anything that
        /// escapes the pitch-deck directory.
        /// </summary>
        public string? ResolvePath(string storedName)
        {
            if (string.IsNullOrWhiteSpace(storedName)) return null;

            // A stored name is always a bare generated filename.
            if (storedName.Contains('/') || storedName.Contains('\\') ||
                storedName.Contains("..") || Path.IsPathRooted(storedName))
            {
                _logger.LogWarning("Rejected suspicious pitch deck path {Name}.", storedName);
                return null;
            }

            var full = Path.GetFullPath(Path.Combine(Root, storedName));

            // Belt and braces: the resolved path must still sit under Root.
            if (!full.StartsWith(Path.GetFullPath(Root), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return File.Exists(full) ? full : null;
        }


        public void Delete(string storedName)
        {
            var path = ResolvePath(storedName);

            if (path != null)
            {
                try { File.Delete(path); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not delete {Path}.", path); }
            }
        }


        /// <summary>A filename safe to put in a Content-Disposition header.</summary>
        public static string SafeDownloadName(PitchDeck deck)
        {
            var name = Path.GetFileName(deck.FileName);

            var cleaned = string.Concat(name.Select(c =>
                char.IsLetterOrDigit(c) || c is '-' or '_' or '.' or ' ' ? c : '_')).Trim();

            return string.IsNullOrWhiteSpace(cleaned) ? "pitch-deck.pdf" : cleaned;
        }
    }
}
