using AG.Justin.CustomClaim.Api.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AG.Justin.CustomClaim.Api.Features.Participant
{
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.JustinUser)]
    public class ParticipantController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Index.Model?>> GetParticipant([FromServices] IQueryHandler<Index.Query, Index.Model?> handler,
                                                                [FromQuery] Index.Query query)
      => await handler.HandleAsync(query);
    }
}
