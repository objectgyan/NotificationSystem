using Microsoft.EntityFrameworkCore;

namespace NotificationSystem.Model
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignEvent> CampaignEvents { get; set; }
        public DbSet<LoyaltyProfile> LoyaltyProfiles { get; set; }
    }
    public class LoyaltyProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Campaign> Campaigns { get; set; }
    }

    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LoyaltyProgramId { get; set; }
        public List<CampaignEvent> CampaignEvents { get; set; }
    }

    public class CampaignEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CampaignId { get; set; }
        public string EventType { get; set; } // Email or SMS
        public string Message { get; set; }
        public int? ParentEventId { get; set; } // Nullable to support independent events
        public int? DelayAfterParentInDays { get; set; } // Nullable to support independent events
    }

    public class LoyaltyProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LoyaltyProgramId { get; set; }
        public DateTime? Birthday { get; set; } // Track when the user subscribed
        public DateTime SubscriptionDate { get; set; } // Track when the user subscribed
        public DateTime? FirstEmailSentDate { get; set; } // Track when the first email was sent
    }
}
