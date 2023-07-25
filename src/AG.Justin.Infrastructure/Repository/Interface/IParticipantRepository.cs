
using AG.Justin.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AG.Justin.Infrastructure.Repository.Interface;
public interface IParticipantRepository
{
    Task<string?> GetParticipantId(string Id);
    Task<JustinParticipant?> GetParticipant(string FirstName, string LastName, string Email);
}

