using CandidateAPI.DomainModels;
using CandidateAPI.Interfaces.Repos;
using Microsoft.Data.SqlClient;

namespace CandidateAPI.Data
{
    public class SkillRepo : ISkillRepo
    {
        private readonly string _connectionString;
        private readonly ILogger<SkillRepo> _logger;


        public SkillRepo(IConfiguration configuration, ILogger<SkillRepo> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _logger = logger;
        }

        #region get

        /// <summary>
        /// Get all skills from the db
        /// </summary>
        /// <returns>returns a list of skills</returns>
        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            var skills = new List<Skill>();
            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //create query
                    using (var command = new SqlCommand(@"SELECT ID, 
                                                                 Name, 
                                                                 CreatedDate, 
                                                                 UpdatedDate 
                                                           FROM dbo.Skill", connection))
                    {
                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                skills.Add(new Skill
                                {
                                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = GetStringOrNull(reader, "Name"),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                                });
                            }
                        }
                    }
                }
                return skills;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "A database access error has occurred");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred");
                throw;
            }
        }

        /// <summary>
        ///  get a specidic skill from the db
        /// </summary>
        /// <param name="id">id of the skill we want</param>
        /// <returns>returns a skill</returns>
        public async Task<Skill> GetSkillByIDAsync(int id)
        {
            var skill = new Skill();

            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    //create query
                    string query = @"SELECT ID, 
                                            Name, 
                                            CreatedDate, 
                                            UpdatedDate
                                     FROM dbo.Skill
                                     WHERE ID = @SkillID
                                     ORDER BY ID, Name;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        //set params
                        command.Parameters.AddWithValue("SkillID", id);

                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                skill = new Skill()
                                {
                                    ID = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = GetStringOrNull(reader, "FirstName"),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                };
                            }
                        }
                    }
                }

                return skill;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "A database access error has occurred");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred");
                throw;
            }
        }

        #endregion

        #region post

        public Task<int> CreateSkillAsync(Skill skill)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region delete

        public Task<bool> DeleteSkillAsync(int skillId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region put
        public Task<bool> UpdateSkillAsync(Skill skill)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private methods

        private string? GetStringOrNull(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private DateTime? GetDateTimeOrNull(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }

        #endregion
    }
}
