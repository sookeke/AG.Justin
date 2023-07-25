using AG.Justin.Infrastructure.Models;
using AG.Justin.Infrastructure.Repository.Interface;
using Dapper;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace AG.Justin.Infrastructure.Repository;
public class ParticipantRepository : IParticipantRepository
{
    private readonly string connectionString;

    public ParticipantRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }
  

    public async Task<string?> GetParticipantId(string Id)
    {
        using (var connection = new OracleConnection(this.connectionString))
        {
            connection.Open();
            var query = "SELECT PART_ID FROM JUSTIN.JUSTIN_PARTICIPANTS WHERE PART_ENT_USER_ID = :PART_ENT_USER_ID";
            var parameter = new { PART_ENT_USER_ID = Id };
            var Partipant = await connection.QueryFirstOrDefaultAsync<string>(query, parameter);


           
            return Partipant; 
        }
    }
    public async Task<JustinParticipant?> GetParticipant(string firstname, string lastname, string email)
    {
        using var connection = new OracleConnection(this.connectionString);
        connection.Open();
        var query = @"SELECT 
       part.part_id,
       part.part_user_id,
       (
           SELECT LISTAGG(paas.agen_id, ', ') WITHIN GROUP (ORDER BY agen.agen_agency_nm)
           FROM justin_partic_assignments paas
           JOIN justin_agencies agen ON agen.agen_id = paas.agen_id
           WHERE paas.paas_end_dt IS NULL
             AND paas.part_id = part.part_id
       ) AS agency_ids,
       (
           SELECT LISTAGG(agen.agen_agency_nm, ', ') WITHIN GROUP (ORDER BY agen.agen_agency_nm)
           FROM justin_partic_assignments paas
           JOIN justin_agencies agen ON agen.agen_id = paas.agen_id
           WHERE paas.paas_end_dt IS NULL
             AND paas.part_id = part.part_id
       ) AS agency_assignments,
       (
           SELECT LISTAGG(granted_role, ', ') WITHIN GROUP (ORDER BY granted_role)
           FROM appl_role_privs
           WHERE granted_role NOT IN ('APPL6_USER', 'OBJLOC_USER', 'OWNER_ACCOUNT', 'USER_ACCOUNT')
             AND grantee = part.part_user_id
       ) AS roles
        FROM justin_participants part
        JOIN justin_identification_details iddt ON part.part_id = iddt.part_id
        WHERE iddt.iddt_name_type_cd = 'CUR'
          AND iddt.IDDT_GIVEN_1_NM = :IDDT_GIVEN_1_NM
          AND iddt.IDDT_SURNAME_NM = :IDDT_SURNAME_NM
          AND (SELECT fcom.fcom_number_txt
               FROM justin_communication_devices fcom
               WHERE fcom.cdcm_com_type_cd = 'EM'
                 AND fcom.part_id = part.part_id
                 AND fcom.fcom_seq_no = (SELECT MAX(fcom1.fcom_seq_no)
                                         FROM justin_communication_devices fcom1
                                         WHERE fcom1.cdcm_com_type_cd = 'EM'
                                           AND fcom1.part_id = fcom.part_id)) = :fcom_seq_no
          AND ROWNUM = 1";
        var parameter = new { IDDT_GIVEN_1_NM = firstname, IDDT_SURNAME_NM = lastname, fcom_seq_no = email };
        var queryResult = await connection.QueryFirstOrDefaultAsync(query, parameter);
        
        if (queryResult == null) { return null; }

        return new JustinParticipant
        {
            PartId = queryResult.PART_ID,
            UserId = queryResult.PART_USER_ID,
            AgencyIds = ((string)queryResult.AGENCY_IDS).Split(", ").ToList(),
            AgencyAssignments = ((string)queryResult.AGENCY_ASSIGNMENTS).Split(", ").ToList(),
            Roles = ((string)queryResult.ROLES).Split(", ").ToList()
        };
    }
}

