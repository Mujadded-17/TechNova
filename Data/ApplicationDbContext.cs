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
        }
    }
}