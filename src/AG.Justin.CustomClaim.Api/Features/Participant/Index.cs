using AG.Justin.Infrastructure.Repository.Interface;
using FluentValidation;

namespace AG.Justin.CustomClaim.Api.Features.Participant;

public class Index
{
    public class Query : IQuery<Model?>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
    }

    public class Model
    {
        public double PartId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<string> AgencyIds { get; set; } = new();
        public List<string> AgencyAssignments { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator(IHttpContextAccessor accessor)
        {
            var user = accessor?.HttpContext?.User;
            this.RuleFor(x => x.FirstName).NotEmpty();
            this.RuleFor(x => x.LastName).NotEmpty();
            this.RuleFor(x => x.Email).NotEmpty();
        }
    }

    public class QueryHandler : IQueryHandler<Query, Model?>
    {
        private readonly IParticipantRepository participantRepository;
        private readonly ILogger logger;

        public QueryHandler(IParticipantRepository participantRepository, ILogger logger)
        {
            this.participantRepository = participantRepository;
            this.logger = logger;
        }

        public async Task<Model?> HandleAsync(Query query)
        {
            this.logger.LogInformation("First Name:" + query.FirstName);
            this.logger.LogInformation("Last Name: " +query.LastName);
            this.logger.LogInformation("Email: "+query.Email);
            var participant =  await this.participantRepository.GetParticipant(
                query.FirstName,
                query.LastName,
                query.Email);

            if (participant == null) { return null; }

            return new Model
            {
                PartId = participant.PartId,
                UserId= participant.UserId,
                AgencyIds= participant.AgencyIds,
                AgencyAssignments = participant.AgencyAssignments,
                Roles = participant.Roles,
            };
                
        }
    }
}
