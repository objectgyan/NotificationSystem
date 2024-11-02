using NotificationSystem.Model;

namespace NotificationSystem.Service
{
    public interface IUserProfileService
    {
        List<LoyaltyProfile> GetAllUserProfiles();
        void UpdateUserProfile(LoyaltyProfile profile);
        LoyaltyProfile GetUserProfile(int profileId);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly NotificationDbContext _context;

        public UserProfileService(NotificationDbContext context)
        {
            _context = context;
        }

        public List<LoyaltyProfile> GetAllUserProfiles()
        {
            return _context.LoyaltyProfiles.ToList();
        }

        public void UpdateUserProfile(LoyaltyProfile profile)
        {
            _context.LoyaltyProfiles.Update(profile);
            _context.SaveChanges();
        }

        public LoyaltyProfile GetUserProfile(int profileId)
        {
            return _context.LoyaltyProfiles.Find(profileId);
        }
    }
}
