using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AG.Justin.Infrastructure.Models;
public class JustinParticipant
{
    [Column("part_id")]
    public double PartId { get; set; }

    [Column("part_user_id")]
    public string UserId { get; set; } = string.Empty;

    [Column("agency_ids")]
    public List<string> AgencyIds { get; set; } = new();

    [Column("agency_assignments")]
    public List<string> AgencyAssignments { get; set; } = new();

    [Column("roles")]
    public List<string> Roles { get; set; } = new();
}
public class Role
{
    public string AssignedRole { get; set; } = string.Empty;
}

public class AgencyAssignment
{
    public string AgencyName { get; set;} = string.Empty;
}