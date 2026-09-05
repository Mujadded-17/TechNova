using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using TechNova.Data;
using TechNova.Models;
using TechNova.Services;
using System.Security.Claims;

namespace TechNova.Controllers
{
    [Authorize(Roles = "Startup")]
    public class StartupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaUploadService _mediaUploadService;

        public StartupController(ApplicationDbContext context, IMediaUploadService mediaUploadService)
        {
            _context = context;
            _mediaUploadService = mediaUploadService;
        }

        // ============================
        // STARTUP DASHBOARD
        // ============================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var startup = await _context.Startups
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StartupID == startupId);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        // ============================
        // STARTUP PROFILE (VIEW)
        // ============================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            var startup = await _context.Startups
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StartupID == id && s.IsPublished);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        // ============================
        // STARTUP PROFILE EDIT
        // ============================

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var startup = await _context.Startups
                .FirstOrDefaultAsync(s => s.StartupID == startupId);

            if (startup == null)
            {
                return NotFound();
            }

            return View(startup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Startup model, IFormFile? logoFile, IFormFile? coverFile)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId) || model.StartupID != startupId)
            {
                return Forbid();
            }

            var startup = await _context.Startups.FindAsync(startupId);
            if (startup == null)
            {
                return NotFound();
            }

            // Handle logo upload
            if (logoFile != null && logoFile.Length > 0)
            {
                var (success, message, filePath) = await _mediaUploadService.UploadPhotoAsync(logoFile, startupId);
                if (success)
                {
                    startup.LogoPath = filePath;
                }
                else
                {
                    TempData["Error"] = $"Logo upload failed: {message}";
                }
            }

            // Handle cover image upload
            if (coverFile != null && coverFile.Length > 0)
            {
                var (success, message, filePath) = await _mediaUploadService.UploadPhotoAsync(coverFile, startupId);
                if (success)
                {
                    startup.CoverImagePath = filePath;
                }
                else
                {
                    TempData["Error"] = $"Cover image upload failed: {message}";
                }
            }

            // Update profile fields (don't update email or password here)
            startup.CompanyName = model.CompanyName;
            startup.Description = model.Description;
            startup.Website = model.Website;
            startup.Tagline = model.Tagline;
            startup.FundingRequired = model.FundingRequired;
            startup.AmountRaised = model.AmountRaised;
            startup.BusinessStage = model.BusinessStage;
            startup.Industry = model.Industry;
            startup.Location = model.Location;
            startup.FoundedYear = model.FoundedYear;
            startup.ProblemStatement = model.ProblemStatement;
            startup.Solution = model.Solution;
            startup.TargetMarket = model.TargetMarket;
            startup.CompetitiveAdvantage = model.CompetitiveAdvantage;
            startup.Traction = model.Traction;
            startup.EquityOffered = model.EquityOffered;
            startup.FundingDeadline = model.FundingDeadline;
            startup.ContactPhone = model.ContactPhone;
            startup.ContactPerson = model.ContactPerson;
            startup.NumberOfEmployees = model.NumberOfEmployees;
            startup.BusinessModel = model.BusinessModel;
            startup.MinimumInvestment = model.MinimumInvestment;
            startup.IsPublished = model.IsPublished;
            startup.UpdatedAt = DateTime.UtcNow;

            _context.Startups.Update(startup);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Dashboard");
        }

        // ============================
        // INVESTMENT OPPORTUNITIES
        // ============================

        [HttpGet]
        public async Task<IActionResult> Opportunities()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunities = await _context.StartupInvestmentOpportunities
                .AsNoTracking()
                .Where(o => o.StartupID == startupId)
                .ToListAsync();

            return View(opportunities);
        }

        [HttpGet]
        public IActionResult CreateOpportunity()
        {
            return View(new StartupInvestmentOpportunity());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOpportunity(StartupInvestmentOpportunity model)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.StartupID = startupId;
            model.CreatedAt = DateTime.UtcNow;

            _context.StartupInvestmentOpportunities.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity created successfully.";
            return RedirectToAction("Opportunities");
        }

        [HttpGet]
        public async Task<IActionResult> EditOpportunity(int id)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == id && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            return View(opportunity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOpportunity(StartupInvestmentOpportunity model)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == model.OpportunityID && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            opportunity.Title = model.Title;
            opportunity.Description = model.Description;
            opportunity.PitchSummary = model.PitchSummary;
            opportunity.FundingGoal = model.FundingGoal;
            opportunity.CurrentFunding = model.CurrentFunding;
            opportunity.FundingStage = model.FundingStage;
            opportunity.EquityPercentage = model.EquityPercentage;
            opportunity.MinimumInvestment = model.MinimumInvestment;
            opportunity.Industry = model.Industry;
            opportunity.Location = model.Location;
            opportunity.BusinessStage = model.BusinessStage;
            opportunity.FoundedYear = model.FoundedYear;
            opportunity.TeamSize = model.TeamSize;
            opportunity.Website = model.Website;
            opportunity.InvestmentDeadline = model.InvestmentDeadline;
            opportunity.IsPublished = model.IsPublished;
            opportunity.UpdatedAt = DateTime.UtcNow;

            _context.StartupInvestmentOpportunities.Update(opportunity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity updated successfully.";
            return RedirectToAction("Opportunities");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOpportunity(int id)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var opportunity = await _context.StartupInvestmentOpportunities
                .FirstOrDefaultAsync(o => o.OpportunityID == id && o.StartupID == startupId);

            if (opportunity == null)
            {
                return NotFound();
            }

            _context.StartupInvestmentOpportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Investment opportunity deleted successfully.";
            return RedirectToAction("Opportunities");
        }

        // ============================
        // INVESTMENT REQUESTS
        // ============================

        [HttpGet]
        public async Task<IActionResult> InvestmentRequests()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var requests = await _context.InvestmentRequests
                .AsNoTracking()
                .Where(r => r.Startup.StartupID == startupId)
                .Include(r => r.Investor)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, string status)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return RedirectToAction("Login", "Account");
            }

            var request = await _context.InvestmentRequests
                .Where(r => r.Startup.StartupID == startupId)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            _context.InvestmentRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Investment request status updated to {status}.";
            return RedirectToAction("InvestmentRequests");
        }

        // ============================
        // POSTS (FEED)
        // ============================

        [HttpGet]
        public async Task<IActionResult> GetFeed()
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.StartupID == startupId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Photos)
                .ToListAsync();

            return Json(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost([FromForm] string content)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Post content is required.");
            }

            var post = new Post
            {
                StartupID = startupId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { postId = post.PostID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int postId, [FromForm] string content)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostID == postId && p.StartupID == startupId);

            if (post == null)
            {
                return NotFound();
            }

            post.Content = content;
            post.UpdatedAt = DateTime.UtcNow;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostID == postId && p.StartupID == startupId);

            if (post == null)
            {
                return NotFound();
            }

            post.IsDeleted = true;
            post.DeletedAt = DateTime.UtcNow;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ============================
        // PHOTOS
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, [FromForm] string caption = "", [FromForm] int? postId = null)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest("Invalid file type. Only image files are allowed.");
            }

            if (file.Length > 10 * 1024 * 1024) // 10MB
            {
                return BadRequest("File size exceeds 10MB limit.");
            }

            // Create media directory if it doesn't exist
            var mediaDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "startup-media");
            if (!Directory.Exists(mediaDir))
            {
                Directory.CreateDirectory(mediaDir);
            }

            var fileName = $"{startupId}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(mediaDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                StartupID = startupId,
                PostID = postId,
                FilePath = $"/startup-media/{fileName}",
                Caption = caption,
                MimeType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(new { photoId = photo.PhotoID, filePath = photo.FilePath });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(int photoId, [FromForm] string caption)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var photo = await _context.Photos
                .FirstOrDefaultAsync(p => p.PhotoID == photoId && p.StartupID == startupId);

            if (photo == null)
            {
                return NotFound();
            }

            photo.Caption = caption;

            _context.Photos.Update(photo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var photo = await _context.Photos
                .FirstOrDefaultAsync(p => p.PhotoID == photoId && p.StartupID == startupId);

            if (photo == null)
            {
                return NotFound();
            }

            // Delete the file
            if (!string.IsNullOrEmpty(photo.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            photo.IsDeleted = true;
            photo.DeletedAt = DateTime.UtcNow;

            _context.Photos.Update(photo);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // ============================
        // VIDEOS
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVideo([FromForm] IFormFile file, [FromForm] string title = "", [FromForm] string description = "")
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var allowedExtensions = new[] { ".mp4", ".webm", ".avi", ".mov", ".mkv" };
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(ext))
            {
                return BadRequest("Invalid file type. Only video files are allowed.");
            }

            if (file.Length > 100 * 1024 * 1024) // 100MB
            {
                return BadRequest("File size exceeds 100MB limit.");
            }

            // Create media directory if it doesn't exist
            var mediaDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "startup-media");
            if (!Directory.Exists(mediaDir))
            {
                Directory.CreateDirectory(mediaDir);
            }

            var fileName = $"video_{startupId}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(mediaDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var video = new Video
            {
                StartupID = startupId,
                FilePath = $"/startup-media/{fileName}",
                Title = title,
                Description = description,
                MimeType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return Ok(new { videoId = video.VideoID, filePath = video.FilePath });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVideo(int videoId, [FromForm] string title, [FromForm] string description)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var video = await _context.Videos
                .FirstOrDefaultAsync(v => v.VideoID == videoId && v.StartupID == startupId);

            if (video == null)
            {
                return NotFound();
            }

            video.Title = title;
            video.Description = description;

            _context.Videos.Update(video);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideo(int videoId)
        {
            var startupIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(startupIdClaim, out int startupId))
            {
                return Unauthorized();
            }

            var video = await _context.Videos
                .FirstOrDefaultAsync(v => v.VideoID == videoId && v.StartupID == startupId);

            if (video == null)
            {
                return NotFound();
            }

            // Delete the file
            if (!string.IsNullOrEmpty(video.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            video.IsDeleted = true;
            video.DeletedAt = DateTime.UtcNow;

            _context.Videos.Update(video);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
