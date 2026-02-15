using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace EhrModelMapping.Controllers
{
    [ApiController]
    [Route("api/ehr-config")]
    public class EhrConfigController : ControllerBase

    {
        private readonly string _connectionString = "";
        [HttpPost]
        [Route("Add")]

        public async Task<IActionResult> Add([FromBody] EhrModelConfigDto dto)
        {
            await AddConfig(dto, "admin");
            return Ok();
        }
        [HttpPut]
        [Route("Update")]

        public async Task<IActionResult> Update([FromBody] EhrModelConfigDto dto)
        {
            await UpdateConfig(dto, "admin");
            return Ok();
        }

        [HttpGet("models/{institutionId}")]
        public async Task<IActionResult> GetModels(int institutionId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                @"SELECT  Id, ModelName, UpdatedAt 
          FROM EhrModelConfigs
          WHERE InstitutionId = @InstitutionId",
                conn);

            cmd.Parameters.AddWithValue("@InstitutionId", institutionId);

            await conn.OpenAsync();

            List<ModelSummaryDto> models = new List<ModelSummaryDto>();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                models.Add(new ModelSummaryDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ModelName = reader["ModelName"].ToString(),
                    UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                });

            }

            return Ok(models);
        }

        [HttpGet("{institutionId}/{id}")]
        public async Task<IActionResult> Get(int institutionId, int id)
        {
            var result = await GetConfig(institutionId, id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        public async Task<EhrModelConfigDto> GetConfig(int institutionId, int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                @"SELECT ConfigJson 
          FROM EhrModelConfigs 
          WHERE InstitutionId = @InstitutionId 
          AND Id = @Id", conn);

            cmd.Parameters.AddWithValue("@InstitutionId", institutionId);
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            if (result == null) return null;

            return JsonConvert.DeserializeObject<EhrModelConfigDto>(result.ToString());
        }


        public async Task<int> AddConfig(EhrModelConfigDto dto, string user)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(@"
        INSERT INTO EhrModelConfigs 
        (InstitutionId, ModelName, ConfigJson, UpdatedBy, UpdatedAt)
        OUTPUT INSERTED.Id
        VALUES (@InstitutionId, @ModelName, @ConfigJson, @UpdatedBy, GETUTCDATE())",
                conn);

            cmd.Parameters.AddWithValue("@InstitutionId", dto.InstitutionId);
            cmd.Parameters.AddWithValue("@ModelName", dto.ModelName);
            cmd.Parameters.AddWithValue("@UpdatedBy", user);

            // We first insert WITHOUT ConfigJson because Id not known yet
            cmd.Parameters.Add("@ConfigJson", System.Data.SqlDbType.NVarChar);

            await conn.OpenAsync();

            // Temporarily store dummy json
            cmd.Parameters["@ConfigJson"].Value = "{}";

            var newId = (int)await cmd.ExecuteScalarAsync();

            // Assign ID back to DTO
            dto.Id = newId;

            // Now update JSON with correct Id inside it
            string json = JsonConvert.SerializeObject(dto);

            using SqlCommand updateCmd = new SqlCommand(@"
        UPDATE EhrModelConfigs
        SET ConfigJson = @ConfigJson
        WHERE Id = @Id", conn);

            updateCmd.Parameters.AddWithValue("@ConfigJson", json);
            updateCmd.Parameters.AddWithValue("@Id", newId);

            await updateCmd.ExecuteNonQueryAsync();

            return newId;
        }

        public async Task UpdateConfig(EhrModelConfigDto dto, string user)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(
                @"UPDATE EhrModelConfigs
          SET ConfigJson = @ConfigJson,
              UpdatedBy = @UpdatedBy,
              UpdatedAt = GETUTCDATE()
          WHERE InstitutionId = @InstitutionId
             AND Id = @Id",
                conn);

            string json = JsonConvert.SerializeObject(dto);

            cmd.Parameters.AddWithValue("@InstitutionId", dto.InstitutionId);
            cmd.Parameters.AddWithValue("@ModelName", dto.ModelName);
            cmd.Parameters.AddWithValue("@ConfigJson", json);
            cmd.Parameters.AddWithValue("@UpdatedBy", user);
            cmd.Parameters.AddWithValue("@Id", dto.Id);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

    }
}
