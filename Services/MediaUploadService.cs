using System.Text.RegularExpressions;

namespace TechNova.Services
{
    public interface IMediaUploadService
    {
        Task<(bool success, string message, string? filePath)> UploadPhotoAsync(IFormFile file, int startupId);
        Task<(bool success, string message, string? filePath)> UploadVideoAsync(IFormFile file, int startupId);
        bool DeleteFile(string filePath);
        string GetMediaDirectory();
    }

    public class MediaUploadService : IMediaUploadService
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly long _maxPhotoSize = 10 * 1024 * 1024; // 10MB
        private readonly long _maxVideoSize = 100 * 1024 * 1024; // 100MB
        private readonly string[] _allowedPhotoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".avi", ".mov", ".mkv" };

        public MediaUploadService(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        public async Task<(bool success, string message, string? filePath)> UploadPhotoAsync(IFormFile file, int startupId)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is required.", null);
            }

            // Validate file extension
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedPhotoExtensions.Contains(ext))
            {
                return (false, "Invalid file type. Only image files are allowed.", null);
            }

            // Validate file size
            if (file.Length > _maxPhotoSize)
            {
                return (false, $"File size exceeds {_maxPhotoSize / (1024 * 1024)}MB limit.", null);
            }

            // Validate MIME type
            if (!file.ContentType.StartsWith("image/"))
            {
                return (false, "Invalid MIME type. Only image files are allowed.", null);
            }

            try
            {
                var mediaDir = GetMediaDirectory();
                if (!Directory.Exists(mediaDir))
                {
                    Directory.CreateDirectory(mediaDir);
                }

                // Generate safe filename
                var fileName = GenerateSafeFileName(file.FileName, startupId, "photo");
                var filePath = Path.Combine(mediaDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var webPath = $"/startup-media/{fileName}";
                return (true, "Photo uploaded successfully.", webPath);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, string? filePath)> UploadVideoAsync(IFormFile file, int startupId)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is required.", null);
            }

            // Validate file extension
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedVideoExtensions.Contains(ext))
            {
                return (false, "Invalid file type. Only video files are allowed.", null);
            }

            // Validate file size
            if (file.Length > _maxVideoSize)
            {
                return (false, $"File size exceeds {_maxVideoSize / (1024 * 1024)}MB limit.", null);
            }

            // Validate MIME type
            if (!file.ContentType.StartsWith("video/"))
            {
                return (false, "Invalid MIME type. Only video files are allowed.", null);
            }

            try
            {
                var mediaDir = GetMediaDirectory();
                if (!Directory.Exists(mediaDir))
                {
                    Directory.CreateDirectory(mediaDir);
                }

                // Generate safe filename
                var fileName = GenerateSafeFileName(file.FileName, startupId, "video");
                var filePath = Path.Combine(mediaDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var webPath = $"/startup-media/{fileName}";
                return (true, "Video uploaded successfully.", webPath);
            }
            catch (Exception ex)
            {
                return (false, $"Error uploading file: {ex.Message}", null);
            }
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                var fullPath = Path.Combine(_hostEnvironment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                return false;
            }
        }

        public string GetMediaDirectory()
        {
            return Path.Combine(_hostEnvironment.WebRootPath, "startup-media");
        }

        private string GenerateSafeFileName(string originalFileName, int startupId, string fileType)
        {
            // Get the file extension
            var ext = Path.GetExtension(originalFileName).ToLower();

            // Create a unique filename: type_startupid_timestamp.ext
            var fileName = $"{fileType}_{startupId}_{DateTime.UtcNow.Ticks}{ext}";

            // Remove any potentially dangerous characters
            fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9._-]", "");

            return fileName;
        }
    }
}
