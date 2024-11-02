using Microsoft.AspNetCore.Mvc;
using NotificationSystem.Model;

namespace NotificationSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignEventController : ControllerBase
    {
        private readonly CampaignEventService _campaignEventService;

        public CampaignEventController(CampaignEventService campaignEventService)
        {
            _campaignEventService = campaignEventService;
        }

        [HttpPost("scheduleDripCampaign")]
        public IActionResult ScheduleDripCampaignEvent([FromBody] CampaignEvent campaignEvent)
        {
            _campaignEventService.ScheduleDripCampaignEvent(campaignEvent);
            return Ok("Drip campaign event scheduled for all users");
        }

        [HttpPost("remove/{id}")]
        public IActionResult RemoveCampaignEvent(int id)
        {
            _campaignEventService.RemoveCampaignEvent(id);
            return Ok("Campaign event removed");
        }

        [HttpPost("removeByLoyaltyProgram/{loyaltyProgramId}")]
        public IActionResult RemoveJobsByLoyaltyProgram(int loyaltyProgramId)
        {
            _campaignEventService.RemoveJobsByLoyaltyProgram(loyaltyProgramId);
            return Ok($"All jobs related to loyalty program {loyaltyProgramId} have been removed");
        }

        [HttpPost("scheduleBirthdayCampaign")]
        public IActionResult ScheduleBirthdayCampaign()
        {
            _campaignEventService.ScheduleBirthdayCampaignEvent();
            return Ok("Birthday campaign event scheduled for today");
        }
    }
}
