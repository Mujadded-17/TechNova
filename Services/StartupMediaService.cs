using Microsoft.EntityFrameworkCore;
using TechNova.Data;
using TechNova.Models;

namespace TechNova.Services
{
    public interface IStartupMediaService
    {
        Task<Post?> GetPostAsync(int postId, int startupId);
        Task<List<Post>> GetStartupPostsAsync(int startupId, int pageSize = 20, int pageNumber = 1);
        Task<List<Photo>> GetStartupPhotosAsync(int startupId, int pageSize = 12);
        Task<List<Video>> GetStartupVideosAsync(int startupId, int pageSize = 12);
        Task<int> GetPhotoCountAsync(int startupId);
        Task<int> GetVideoCountAsync(int startupId);
        Task<int> GetPostCountAsync(int startupId);
    }

    public class StartupMediaService : IStartupMediaService
    {
        private readonly ApplicationDbContext _context;

        public StartupMediaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetPostAsync(int postId, int startupId)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.PostID == postId && p.StartupID == startupId && !p.IsDeleted)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Post>> GetStartupPostsAsync(int startupId, int pageSize = 20, int pageNumber = 1)
        {
            return await _context.Posts
                .AsNoTracking()
                .Where(p => p.StartupID == startupId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<List<Photo>> GetStartupPhotosAsync(int startupId, int pageSize = 12)
        {
            return await _context.Photos
                .AsNoTracking()
                .Where(p => p.StartupID == startupId && !p.IsDeleted)
                .OrderByDescending(p => p.UploadedAt)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Video>> GetStartupVideosAsync(int startupId, int pageSize = 12)
        {
            return await _context.Videos
                .AsNoTracking()
                .Where(v => v.StartupID == startupId && !v.IsDeleted)
                .OrderByDescending(v => v.UploadedAt)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPhotoCountAsync(int startupId)
        {
            return await _context.Photos
                .Where(p => p.StartupID == startupId && !p.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetVideoCountAsync(int startupId)
        {
            return await _context.Videos
                .Where(v => v.StartupID == startupId && !v.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetPostCountAsync(int startupId)
        {
            return await _context.Posts
                .Where(p => p.StartupID == startupId && !p.IsDeleted)
                .CountAsync();
        }
    }
}
