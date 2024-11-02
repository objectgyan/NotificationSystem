using Hangfire;
using NotificationSystem.Model;
using NotificationSystem.Service;

public class CampaignEventService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IUserProfileService _userProfileService;
    private readonly NotificationDbContext _context; // Add DbContext to access the database

    public CampaignEventService(
        IBackgroundJobClient backgroundJobClient,
        IUserProfileService userProfileService,
        NotificationDbContext context)
    {
        _backgroundJobClient = backgroundJobClient;
        _userProfileService = userProfileService;
        _context = context;
    }

    public void ScheduleDripCampaignEvent(CampaignEvent campaignEvent)
    {
        var userProfiles = _userProfileService.GetAllUserProfiles();
        foreach (var profile in userProfiles)
        {
            ScheduleInitialEvent(campaignEvent, profile);
        }
    }

    private void ScheduleInitialEvent(CampaignEvent campaignEvent, LoyaltyProfile profile)
    {
        if (campaignEvent.ParentEventId == null)
        {
            ScheduleUserSpecificEvent(campaignEvent, profile, profile.SubscriptionDate);
        }
    }

    private void ScheduleUserSpecificEvent(CampaignEvent campaignEvent, LoyaltyProfile profile, DateTime parentExecutionTime)
    {
        if (campaignEvent.DelayAfterParentInDays.HasValue)
        {
            var delay = TimeSpan.FromDays(campaignEvent.DelayAfterParentInDays.Value);
            var scheduledTime = parentExecutionTime.Add(delay);
            var delayFromNow = scheduledTime - DateTime.UtcNow;

            string jobId = $"{campaignEvent.CampaignId}_{profile.LoyaltyProgramId}_{campaignEvent.Name}_{profile.Name}_{Guid.NewGuid()}";

            if (delayFromNow > TimeSpan.Zero)
            {
                _backgroundJobClient.Schedule(
                    () => ExecuteCampaignEvent(campaignEvent, profile, scheduledTime),
                    delayFromNow);
            }
            else
            {
                _backgroundJobClient.Enqueue(
                    () => ExecuteCampaignEvent(campaignEvent, profile, scheduledTime));
            }
        }
        else
        {
            string jobId = $"{campaignEvent.CampaignId}_{profile.LoyaltyProgramId}_{campaignEvent.Name}_{profile.Name}_{Guid.NewGuid()}";
            _backgroundJobClient.Enqueue(
                () => ExecuteCampaignEvent(campaignEvent, profile, parentExecutionTime));
        }
    }

    public void ExecuteCampaignEvent(CampaignEvent campaignEvent, LoyaltyProfile profile, DateTime executionTime)
    {
        if (campaignEvent.EventType == "Email")
        {
            // Send email
            Console.WriteLine($"Sending email to {profile.Name}: {campaignEvent.Message}");
            profile.FirstEmailSentDate = DateTime.UtcNow; // Update the profile
            _userProfileService.UpdateUserProfile(profile);
        }
        else if (campaignEvent.EventType == "SMS")
        {
            // Send SMS
            Console.WriteLine($"Sending SMS to {profile.Name}: {campaignEvent.Message}");
        }

        // Schedule dependent events only after the parent event is executed
        var childEvents = GetChildEvents(campaignEvent.Id);
        foreach (var childEvent in childEvents)
        {
            ScheduleUserSpecificEvent(childEvent, profile, executionTime);
        }
    }

    private List<CampaignEvent> GetChildEvents(int parentEventId)
    {
        // Retrieve child events from the database
        return _context.CampaignEvents.Where(e => e.ParentEventId == parentEventId).ToList();
    }

    public void RemoveCampaignEvent(int campaignEventId)
    {
        // Recursively delete all child events
        var childEvents = GetChildEvents(campaignEventId);
        foreach (var childEvent in childEvents)
        {
            RemoveCampaignEvent(childEvent.Id);
        }

        // Delete the parent event
        _backgroundJobClient.Delete(campaignEventId.ToString());
    }

    public void RemoveJobsByLoyaltyProgram(int loyaltyProgramId)
    {
        var monitoringApi = JobStorage.Current.GetMonitoringApi();

        // Get all campaigns related to the loyalty program
        var campaignIds = _context.Campaigns
            .Where(c => c.LoyaltyProgramId == loyaltyProgramId)
            .Select(c => c.Id)
            .ToList();

        // Remove scheduled jobs
        var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);
        foreach (var job in scheduledJobs)
        {
            if (job.Value.Job.Args.Any(arg => arg is CampaignEvent campaignEvent && campaignIds.Contains(campaignEvent.CampaignId)))
            {
                _backgroundJobClient.Delete(job.Key);
            }
        }

        // Remove processing jobs
        var processingJobs = monitoringApi.ProcessingJobs(0, int.MaxValue);
        foreach (var job in processingJobs)
        {
            if (job.Value.Job.Args.Any(arg => arg is CampaignEvent campaignEvent && campaignIds.Contains(campaignEvent.CampaignId)))
            {
                _backgroundJobClient.Delete(job.Key);
            }
        }

        // Remove enqueued jobs
        var queues = monitoringApi.Queues();
        foreach (var queue in queues)
        {
            var enqueuedJobs = monitoringApi.EnqueuedJobs(queue.Name, 0, int.MaxValue);
            foreach (var job in enqueuedJobs)
            {
                if (job.Value.Job.Args.Any(arg => arg is CampaignEvent campaignEvent && campaignIds.Contains(campaignEvent.CampaignId)))
                {
                    _backgroundJobClient.Delete(job.Key);
                }
            }
        }
    }

    public void ScheduleBirthdayCampaignEvent()
    {
        var today = DateTime.UtcNow.Date;
        var userProfiles = _userProfileService.GetAllUserProfiles()
            .Where(profile => profile.Birthday.HasValue && profile.Birthday.Value.Date == today)
            .ToList();

        var birthdayCampaignEvent = _context.CampaignEvents.FirstOrDefault(e => e.Name == "Birthday Campaign");

        if (birthdayCampaignEvent != null)
        {
            foreach (var profile in userProfiles)
            {
                ScheduleUserSpecificEvent(birthdayCampaignEvent, profile, today);
            }
        }
    }
}