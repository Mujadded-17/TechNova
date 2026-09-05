using Microsoft.EntityFrameworkCore;
using TechNova.Models;

namespace TechNova.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Startup> Startups { get; set; }
        public DbSet<Founder> Founders { get; set; }
        public DbSet<Investor> Investors { get; set; }
        public DbSet<PitchDeck> PitchDecks { get; set; }
        public DbSet<InvestmentRequest> InvestmentRequests { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<StartupInvestmentOpportunity> StartupInvestmentOpportunities { get; set; }
        public DbSet<FavoriteStartup> FavoriteStartups { get; set; }

        public DbSet<NfcCardRequest> NfcCardRequests { get; set; }

        // Social Media Feed
        public DbSet<Post> Posts { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Video> Videos { get; set; }

        // Billing
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // Primary Keys
            // -------------------------
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.AdminID);

            modelBuilder.Entity<Startup>()
                .HasKey(s => s.StartupID);

            modelBuilder.Entity<Founder>()
                .HasKey(f => f.FounderID);

            modelBuilder.Entity<Investor>()
                .HasKey(i => i.InvestorID);

            modelBuilder.Entity<PitchDeck>()
                .HasKey(p => p.PitchID);

            modelBuilder.Entity<InvestmentRequest>()
                .HasKey(r => r.RequestID);

            modelBuilder.Entity<Message>()
                .HasKey(m => m.MessageID);

            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .HasKey(o => o.OpportunityID);

            modelBuilder.Entity<FavoriteStartup>()
                .HasKey(f => f.FavoriteID);

            modelBuilder.Entity<NfcCardRequest>()
    .HasKey(n => n.NfcCardRequestID);

            // -------------------------
            // Startup -> Founder
            // One Startup has many Founders
            // -------------------------
            modelBuilder.Entity<Founder>()
                .HasOne(f => f.Startup)
                .WithMany(s => s.Founders)
                .HasForeignKey(f => f.StartupID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Startup -> PitchDeck
            // One Startup has many PitchDecks
            // -------------------------
            modelBuilder.Entity<PitchDeck>()
                .HasOne(p => p.Startup)
                .WithMany(s => s.PitchDecks)
                .HasForeignKey(p => p.StartupID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Startup -> InvestmentRequest
            // One Startup has many InvestmentRequests
            // -------------------------
            modelBuilder.Entity<InvestmentRequest>()
                .HasOne(r => r.Startup)
                .WithMany(s => s.InvestmentRequests)
                .HasForeignKey(r => r.StartupID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Investor -> InvestmentRequest
            // One Investor has many InvestmentRequests
            // -------------------------
            modelBuilder.Entity<InvestmentRequest>()
                .HasOne(r => r.Investor)
                .WithMany(i => i.InvestmentRequests)
                .HasForeignKey(r => r.InvestorID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Startup -> Message
            // One Startup has many Messages
            // -------------------------
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Startup)
                .WithMany(s => s.Messages)
                .HasForeignKey(m => m.StartupID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Investor -> Message
            // One Investor has many Messages
            // -------------------------
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Investor)
                .WithMany(i => i.Messages)
                .HasForeignKey(m => m.InvestorID)
                .OnDelete(DeleteBehavior.Cascade);


            // -------------------------
            // Admin -> Startup
            // Admin verifies Startups
            // -------------------------
            modelBuilder.Entity<Startup>()
                .HasOne(s => s.VerifiedByAdmin)
                .WithMany(a => a.VerifiedStartups)
                .HasForeignKey(s => s.VerifiedByAdminID)
                .OnDelete(DeleteBehavior.SetNull);


            // -------------------------
            // Admin -> Investor
            // Admin verifies Investors
            // -------------------------
            modelBuilder.Entity<Investor>()
                .HasOne(i => i.VerifiedByAdmin)
                .WithMany(a => a.VerifiedInvestors)
                .HasForeignKey(i => i.VerifiedByAdminID)
                .OnDelete(DeleteBehavior.SetNull);


            // -------------------------
            // Decimal precision
            // -------------------------
            modelBuilder.Entity<Startup>()
                .Property(s => s.FundingRequired)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Startup>()
                .Property(s => s.AmountRaised)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Startup>()
                .Property(s => s.MinimumInvestment)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Startup>()
                .Property(s => s.EquityOffered)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvestmentRequest>()
                .Property(r => r.InvestmentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Investor>()
                .Property(i => i.InvestmentRange)
                .HasPrecision(18, 2);


            // -------------------------
            // Billing
            // -------------------------
            modelBuilder.Entity<SubscriptionPlan>()
                .HasKey(p => p.SubscriptionPlanID);

            modelBuilder.Entity<Subscription>()
                .HasKey(s => s.SubscriptionID);

            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentID);

            // Plan codes are the stable machine key.
            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Our payment reference must be unique so it can be quoted
            // on a bank transfer and reconciled unambiguously.
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Reference)
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Investor)
                .WithMany()
                .HasForeignKey(s => s.InvestorID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPlanID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(p => p.SubscriptionID)
                .OnDelete(DeleteBehavior.Cascade);

            // Deleting an investor cascades via Subscription; a second
            // cascade path here would create multiple-cascade-paths.
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Investor)
                .WithMany()
                .HasForeignKey(p => p.InvestorID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SubscriptionPlan>()
                .Property(p => p.PriceMonthly)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.PriceAtSubscription)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Investor>()
                .Property(i => i.MinInvestmentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Investor>()
                .Property(i => i.MaxInvestmentAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .Property(o => o.FundingGoal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .Property(o => o.CurrentFunding)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .Property(o => o.EquityPercentage)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .Property(o => o.MinimumInvestment)
                .HasPrecision(18, 2);

            // -------------------------
            // Startup -> StartupInvestmentOpportunity
            // One Startup has many Investment Opportunities
            // -------------------------
            modelBuilder.Entity<StartupInvestmentOpportunity>()
                .HasOne(o => o.Startup)
                .WithMany() // No reverse navigation property yet
                .HasForeignKey(o => o.StartupID)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // StartupInvestmentOpportunity -> InvestmentRequest
            // One Opportunity has many Investment Requests
            // -------------------------
            // Note: We need to update InvestmentRequest model to have OpportunityID foreign key
            // For now, we'll handle relationship via Startup

            // -------------------------
            // Investor -> FavoriteStartup
            // One Investor has many Favorite Startups
            // -------------------------
            modelBuilder.Entity<FavoriteStartup>()
                .HasKey(f => f.FavoriteID);

            modelBuilder.Entity<FavoriteStartup>()
                .HasOne(f => f.Investor)
                .WithMany(i => i.FavoriteStartups)
                .HasForeignKey(f => f.InvestorID)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Startup -> FavoriteStartup
            // One Startup has many Investors who favorited it
            // -------------------------
            modelBuilder.Entity<FavoriteStartup>()
                .HasOne(f => f.Startup)
                .WithMany(s => s.FavoredByInvestors)
                .HasForeignKey(f => f.StartupID)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: Each investor can favorite a startup only once
            modelBuilder.Entity<FavoriteStartup>()
                .HasIndex(f => new { f.InvestorID, f.StartupID })
                .IsUnique();

            // -------------------------
            // Startup -> Post
            // One Startup has many Posts
            // -------------------------
            modelBuilder.Entity<Post>()
                .HasKey(p => p.PostID);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Startup)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.StartupID)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Startup -> Photo
            // One Startup has many Photos
            // -------------------------
            modelBuilder.Entity<Photo>()
                .HasKey(p => p.PhotoID);

            modelBuilder.Entity<Photo>()
                .HasOne(p => p.Startup)
                .WithMany(s => s.Photos)
                .HasForeignKey(p => p.StartupID)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // Post -> Photo
            // One Post has many Photos
            // -------------------------
            modelBuilder.Entity<Photo>()
                .HasOne(p => p.Post)
                .WithMany(po => po.Photos)
                .HasForeignKey(p => p.PostID)
                .OnDelete(DeleteBehavior.Cascade);

            // -------------------------
            // Startup -> Video
            // One Startup has many Videos
            // -------------------------
            modelBuilder.Entity<Video>()
                .HasKey(v => v.VideoID);

            modelBuilder.Entity<Video>()
                .HasOne(v => v.Startup)
                .WithMany(s => s.Videos)
                .HasForeignKey(v => v.StartupID)
                .OnDelete(DeleteBehavior.NoAction);

            // -------------------------
            // Post -> Video
            // One Post has many Videos
            // -------------------------
            modelBuilder.Entity<Video>()
                .HasOne(v => v.Post)
                .WithMany()
                .HasForeignKey(v => v.PostID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}