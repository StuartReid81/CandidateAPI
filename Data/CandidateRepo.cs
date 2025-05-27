using CandidateAPI.DomainModels;
using CandidateAPI.Interfaces.Repos;
using Microsoft.Data.SqlClient;

namespace CandidateAPI.Data
{
    public class CandidateRepo : ICandidateRepo
    {
        private readonly string _connectionString;
        private readonly ILogger<CandidateRepo> _logger;

        public CandidateRepo(IConfiguration configuration, ILogger<CandidateRepo> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _logger = logger;
        }

        #region get

        /// <summary>
        /// Gets all candidates from db without skills
        /// </summary>
        /// <returns>list of candidates</returns>
        public async Task<List<Candidate>> GetAllCandidatesAsync()
        {
            var candidates = new List<Candidate>();
            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //create query
                    using (var command = new SqlCommand(@"SELECT ID, 
                                                                 FirstName, 
                                                                 Surname, 
                                                                 DateOfBirth, 
                                                                 Address1, 
                                                                 Town,
                                                                 Country, 
                                                                 PostCode, 
                                                                 PhoneHome, 
                                                                 PhoneMobile, 
                                                                 PhoneWork, 
                                                                 CreatedDate, 
                                                                 UpdatedDate 
                                                           FROM dbo.Candidate", connection))
                    {
                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                candidates.Add(new Candidate
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = GetStringOrNull(reader, "FirstName"),
                                    Surname = GetStringOrNull(reader, "Surname"),
                                    DateOfBirth = GetDateTimeOrNull(reader, "DateOfBirth"),
                                    Address1 = GetStringOrNull(reader, "Address1"),
                                    Town = GetStringOrNull(reader, "Town"),
                                    Country = GetStringOrNull(reader, "Country"),
                                    PostCode = GetStringOrNull(reader, "PostCode"),
                                    PhoneHome = GetStringOrNull(reader, "PhoneHome"),
                                    PhoneMobile = GetStringOrNull(reader, "PhoneMobile"),
                                    PhoneWork = GetStringOrNull(reader, "PhoneWork"),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                                });
                            }
                        }
                    }
                }
                return candidates;
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
        /// gets all candidates from db includes any skills
        /// </summary>
        /// <returns>list of candidates</returns>
        public async Task<List<Candidate>> GetCandidatesWithSkillsAsync()
        {
            var candidates = new List<Candidate>();

            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    //create query
                    string query = @"SELECT c.ID, 
                                            c.FirstName, 
                                            c.Surname,
                                            c.DateOfBirth, 
                                            c.Address1, 
                                            c.Town, 
                                            c.Country, 
                                            c.PostCode, 
                                            c.PhoneHome, 
                                            c.PhoneMobile, 
                                            c.PhoneWork, 
                                            c.CreatedDate, 
                                            c.UpdatedDate,
                                            cs.SkillID,
                                            cs.CreatedDate as candidateSkillCreatedDate,
                                            cs.UpdatedDate as candidateSkillUpdatedDate,
                                            s.Name as skillName,
                                            s.CreatedDate as skillCreatedDate,
                                            s.UpdatedDate as skillUpdatedDate
                    FROM dbo.Candidate c
                    LEFT JOIN dbo.CandidateSkill cs ON c.ID = cs.CandidateId
                    LEFT JOIN dbo.Skill s ON cs.SkillID = s.ID
                    ORDER BY c.ID, s.Name;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Candidate? currentCandidate = null;

                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                int candidateId = reader.GetInt32(reader.GetOrdinal("Id"));

                                // if current candidate has not been set yet or candidate Id exists 
                                if (currentCandidate == null || currentCandidate.Id != candidateId)
                                {
                                    currentCandidate = new Candidate
                                    {
                                        Id = candidateId,
                                        FirstName = GetStringOrNull(reader, "FirstName"),
                                        Surname = GetStringOrNull(reader, "Surname"),
                                        DateOfBirth = GetDateTimeOrNull(reader, "DateOfBirth"),
                                        Address1 = GetStringOrNull(reader, "Address1"),
                                        Town = GetStringOrNull(reader, "Town"),
                                        Country = GetStringOrNull(reader, "Country"),
                                        PostCode = GetStringOrNull(reader, "PostCode"),
                                        PhoneHome = GetStringOrNull(reader, "PhoneHome"),
                                        PhoneMobile = GetStringOrNull(reader, "PhoneMobile"),
                                        PhoneWork = GetStringOrNull(reader, "PhoneWork"),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                        UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                        Skills = new List<CandidateSkill>()
                                    };
                                    candidates.Add(currentCandidate);
                                }

                                //If we have skills to add
                                if (!reader.IsDBNull(reader.GetOrdinal("SkillId")))
                                {
                                    currentCandidate.Skills?.Add(new CandidateSkill // Use null conditional operator
                                    {
                                        CandidateId = candidateId,
                                        SkillId = reader.GetInt32(reader.GetOrdinal("SkillId")),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("candidateSkillCreatedDate")),
                                        UpdatedDate = reader.GetDateTime(reader.GetOrdinal("candidateSkillUpdatedDate")),
                                        Skill = new Skill()
                                        {
                                            ID = reader.GetInt32(reader.GetOrdinal("SkillId")),
                                            Name = GetStringOrNull(reader, "skillName"),
                                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("skillCreatedDate")),
                                            UpdatedDate = reader.GetDateTime(reader.GetOrdinal("skillUpdatedDate")),
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                return candidates;
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
        /// Get a candidate from db without matching skills
        /// </summary>
        /// <param name="id">id of candidate we want</param>
        /// <returns>a candidate</returns>
        public async Task<Candidate> GetCandidateByIDAsync(int id)
        {
            var candidate = new Candidate();
            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();
                    //create query
                    using (var command = new SqlCommand(@"SELECT ID, 
                                                                 FirstName, 
                                                                 Surname, 
                                                                 DateOfBirth, 
                                                                 Address1, 
                                                                 Town,
                                                                 Country, 
                                                                 PostCode, 
                                                                 PhoneHome, 
                                                                 PhoneMobile, 
                                                                 PhoneWork, 
                                                                 CreatedDate, 
                                                                 UpdatedDate 
                                                           FROM dbo.Candidate
                                                           WHERE ID = @CandidateID", connection))
                    {
                        //set param for query
                        command.Parameters.AddWithValue("@CandidateID", id);

                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                candidate = new Candidate
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = GetStringOrNull(reader, "FirstName"),
                                    Surname = GetStringOrNull(reader, "Surname"),
                                    DateOfBirth = GetDateTimeOrNull(reader, "DateOfBirth"),
                                    Address1 = GetStringOrNull(reader, "Address1"),
                                    Town = GetStringOrNull(reader, "Town"),
                                    Country = GetStringOrNull(reader, "Country"),
                                    PostCode = GetStringOrNull(reader, "PostCode"),
                                    PhoneHome = GetStringOrNull(reader, "PhoneHome"),
                                    PhoneMobile = GetStringOrNull(reader, "PhoneMobile"),
                                    PhoneWork = GetStringOrNull(reader, "PhoneWork"),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                    UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                                };
                            }
                        }
                    }
                }
                return candidate;
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
        /// get a candidate from the db with matching skills
        /// </summary>
        /// <param name="id">id of the candidate we want from the db</param>
        /// <returns>a candidate</returns>
        public async Task<Candidate> GetCandidateWithSkillsByIdAsync(int id)
        {
            var candidate = new Candidate();

            try
            {
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection
                    await connection.OpenAsync();

                    //create query
                    string query = @"SELECT c.ID, 
                                            c.FirstName, 
                                            c.Surname,
                                            c.DateOfBirth, 
                                            c.Address1, 
                                            c.Town, 
                                            c.Country, 
                                            c.PostCode, 
                                            c.PhoneHome, 
                                            c.PhoneMobile, 
                                            c.PhoneWork, 
                                            c.CreatedDate, 
                                            c.UpdatedDate,
                                            cs.SkillID,
                                            cs.CreatedDate as candidateSkillCreatedDate,
                                            cs.UpdatedDate as candidateSkillUpdatedDate,
                                            s.Name as skillName,
                                            s.CreatedDate as skillCreatedDate,
                                            s.UpdatedDate as skillUpdatedDate
                    FROM dbo.Candidate c
                    LEFT JOIN dbo.CandidateSkill cs ON c.ID = cs.CandidateId
                    LEFT JOIN dbo.Skill s ON cs.SkillID = s.ID
                    WHERE c.ID = @CandidateID
                    ORDER BY c.ID, s.Name;";

                    using (var command = new SqlCommand(query, connection))
                    {
                        //set params
                        command.Parameters.AddWithValue("CandidateID", id);

                        //excecute query
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            candidate = null;

                            //while we have results
                            while (await reader.ReadAsync())
                            {
                                int candidateId = reader.GetInt32(reader.GetOrdinal("Id"));

                                // if current candidate has not been set yet or candidate Id exists 
                                if (candidate == null || candidate.Id != candidateId)
                                {
                                    candidate = new Candidate
                                    {
                                        Id = candidateId,
                                        FirstName = GetStringOrNull(reader, "FirstName"),
                                        Surname = GetStringOrNull(reader, "Surname"),
                                        DateOfBirth = GetDateTimeOrNull(reader, "DateOfBirth"),
                                        Address1 = GetStringOrNull(reader, "Address1"),
                                        Town = GetStringOrNull(reader, "Town"),
                                        Country = GetStringOrNull(reader, "Country"),
                                        PostCode = GetStringOrNull(reader, "PostCode"),
                                        PhoneHome = GetStringOrNull(reader, "PhoneHome"),
                                        PhoneMobile = GetStringOrNull(reader, "PhoneMobile"),
                                        PhoneWork = GetStringOrNull(reader, "PhoneWork"),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                        UpdatedDate = reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                        Skills = new List<CandidateSkill>()
                                    };
                                }

                                //If we have skills to add
                                if (!reader.IsDBNull(reader.GetOrdinal("SkillId")))
                                {
                                    candidate.Skills?.Add(new CandidateSkill // Use null conditional operator
                                    {
                                        CandidateId = candidateId,
                                        SkillId = reader.GetInt32(reader.GetOrdinal("SkillId")),
                                        CreatedDate = reader.GetDateTime(reader.GetOrdinal("candidateSkillCreatedDate")),
                                        UpdatedDate = reader.GetDateTime(reader.GetOrdinal("candidateSkillUpdatedDate")),
                                        Skill = new Skill()
                                        {
                                            ID = reader.GetInt32(reader.GetOrdinal("SkillId")),
                                            Name = GetStringOrNull(reader, "skillName"),
                                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("skillCreatedDate")),
                                            UpdatedDate = reader.GetDateTime(reader.GetOrdinal("skillUpdatedDate")),
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                return candidate;
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

        /// <summary>
        /// create a candidate on the db
        /// </summary>
        /// <param name="candidate">the candidate we want to create</param>
        /// <returns>id of the new candidate</returns>
        public async Task<int> CreateCandidateAsync(Candidate candidate)
        {
            try
            {
                //create and open connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    //would never do it this way because of concurrency issues - should configure DB with auto-increment ID
                    int newId;

                    // First, get the maximum existing ID
                    using (var getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(Id), 0) FROM dbo.Candidate;", connection))
                    {
                        var result = await getMaxIdCommand.ExecuteScalarAsync();
                        int maxId = Convert.ToInt32(result);
                        newId = maxId + 1; // Calculate the next ID
                        _logger.LogInformation($"Calculated new candidate ID: {newId}");
                    }


                    // Using SCOPE_IDENTITY() to get the created ID
                    using (var command = new SqlCommand(
                        @"INSERT INTO dbo.Candidate 
                                       (ID,
                                        FirstName, 
                                        Surname, 
                                        DateOfBirth, 
                                        Address1, 
                                        Town, 
                                        Country, 
                                        PostCode, 
                                        PhoneHome, 
                                        PhoneMobile, 
                                        PhoneWork, 
                                        CreatedDate, 
                                        UpdatedDate)
                        OUTPUT INSERTED.ID
                        VALUES 
                                       (@ID,                                     
                                        @FirstName, 
                                        @Surname, 
                                        @DateOfBirth, 
                                        @Address1,  
                                        @Town, 
                                        @Country, 
                                        @PostCode, 
                                        @PhoneHome, 
                                        @PhoneMobile, 
                                        @PhoneWork, 
                                        @CreatedDate, 
                                        @UpdatedDate);",
                        connection))
                    {
                        //set params
                        command.Parameters.AddWithValue("@ID", newId);
                        command.Parameters.AddWithValue("@FirstName", candidate.FirstName);
                        command.Parameters.AddWithValue("@Surname", candidate.Surname);
                        command.Parameters.AddWithValue("@DateOfBirth", candidate.DateOfBirth);
                        command.Parameters.AddWithValue("@Address1", candidate.Address1);
                        command.Parameters.AddWithValue("@Town", candidate.Town);
                        command.Parameters.AddWithValue("@Country", candidate.Country);
                        command.Parameters.AddWithValue("@PostCode", candidate.PostCode);
                        command.Parameters.AddWithValue("@PhoneHome", candidate.PhoneHome);
                        command.Parameters.AddWithValue("@PhoneMobile", candidate.PhoneMobile);
                        command.Parameters.AddWithValue("@PhoneWork", candidate.PhoneWork);

                        if (candidate.CreatedDate == DateTime.MinValue)
                        {
                            command.Parameters.AddWithValue("@CreatedDate",DateTime.Now);
                        }
                        else {
                            command.Parameters.AddWithValue("@CreatedDate", candidate.CreatedDate);
                        }

                        if (candidate.UpdatedDate == DateTime.MinValue)
                        {
                            command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);
                        }
                        else {
                            command.Parameters.AddWithValue("@UpdatedDate", candidate.UpdatedDate);
                        }


                        //excecute query and return Id of created item
                        var result = await command.ExecuteScalarAsync();

                        return Convert.ToInt32(result);
                    }
                }
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
        /// creates assocaition between candidate and skill - adds a CandidateSkill to the db
        /// checks no existing link currently in db
        /// </summary>
        /// <param name="candidateId">id of the candidate</param>
        /// <param name="skillId">id of the skill</param>
        /// <returns>true if succcessfull - false if existing item already in db or if insert fails</returns>
        public async Task<bool> AddSkillToCandidateAsync(int candidateId, int skillId)
        {
            try
            {   
                //create connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    //open connection 
                    await connection.OpenAsync();
                    // Prevent duplicate entries
                    string checkQuery = @"SELECT 
                                                COUNT(*) 
                                          FROM 
                                                dbo.CandidateSkill 
                                          WHERE 
                                                CandidateId = @CandidateId AND SkillId = @SkillId";

                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        //set params
                        checkCommand.Parameters.AddWithValue("@CandidateId", candidateId);
                        checkCommand.Parameters.AddWithValue("@SkillId", skillId);
                        //check for existing link
                        if (Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0)
                        {
                            return false; // Skill already exists for this candidate
                        }
                    }

                    //would never do it this way because of concurrency issues - should configure DB with auto-increment ID
                    int newId;

                    // First, get the maximum existing ID
                    using (var getMaxIdCommand = new SqlCommand("SELECT ISNULL(MAX(Id), 0) FROM dbo.CandidateSkill;", connection))
                    {
                        var result = await getMaxIdCommand.ExecuteScalarAsync();
                        int maxId = Convert.ToInt32(result);
                        newId = maxId + 1; // Calculate the next ID
                        _logger.LogInformation($"Calculated new candidate ID: {newId}");
                    }


                    //create insert statement
                    string insertQuery = @"INSERT INTO 
                                                        dbo.CandidateSkill 
                                                        (ID,CandidateId, SkillId, CreatedDate, UpdatedDate) 
                                           OUTPUT INSERTED.ID
                                           VALUES 
                                                        (@ID,@CandidateId, @SkillId, @CreatedDate, @UpdatedDate)";
                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        //set params
                        command.Parameters.AddWithValue("@ID", newId);
                        command.Parameters.AddWithValue("@CandidateId", candidateId);
                        command.Parameters.AddWithValue("@SkillId", skillId);
                        command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        command.Parameters.AddWithValue("@UpdatedDate", DateTime.Now);

                        //execute insert returns true if success
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
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

        #region put

        /// <summary>
        /// update candidate entry on the db
        /// </summary>
        /// <param name="candidate">candidate with the new details we want to persist</param>
        /// <returns>returns true is update successful</returns>
        public async Task<bool> UpdateCandidateAsync(Candidate candidate)
        {
            try
            {
                //create and open connection
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    //create update statement
                    using (var command = new SqlCommand(
                                  @"UPDATE 
                                            dbo.Candidate 
                                    SET 
                                            FirstName = @FirstName, 
                                            Surname = @Surname, 
                                            DateOfBirth = @DateOfBirth, 
                                            Address1 = @Address1, 
                                            Town = @Town, 
                                            Country = @Country, 
                                            PostCode = @PostCode, 
                                            PhoneHome = @PhoneHome,
                                            PhoneMobile = @PhoneMobile, 
                                            PhoneWork = @PhoneWork, 
                                            UpdatedDate = @UpdatedDate 
                                    WHERE 
                                            Id = @Id",
                        connection))
                    {
                        //set params
                        command.Parameters.AddWithValue("@Id", candidate.Id);
                        command.Parameters.AddWithValue("@FirstName", candidate.FirstName);
                        command.Parameters.AddWithValue("@Surname", candidate.Surname);
                        command.Parameters.AddWithValue("@DateOfBirth", candidate.DateOfBirth);
                        command.Parameters.AddWithValue("@Address1", candidate.Address1);
                        command.Parameters.AddWithValue("@Town", candidate.Town);
                        command.Parameters.AddWithValue("@Country", candidate.Country);
                        command.Parameters.AddWithValue("@PostCode", candidate.PostCode);
                        command.Parameters.AddWithValue("@PhoneHome", candidate.PhoneHome);
                        command.Parameters.AddWithValue("@PhoneMobile", candidate.PhoneMobile);
                        command.Parameters.AddWithValue("@PhoneWork", candidate.PhoneWork);
                        command.Parameters.AddWithValue("@UpdatedDate", candidate.UpdatedDate);

                        //excecute update - returns true if update succeeds
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
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

        #region delete

        /// <summary>
        /// removes and association with a candidate and a skill
        /// </summary>
        /// <param name="candidateId">id of candidate</param>
        /// <param name="skillId">Id of skill</param>
        /// <returns>returns true if successful</returns>
        public async Task<bool> RemoveSkillFromCandidateAsync(int candidateId, int skillId)
        {
            //create and open connection
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                //create delete statement
                string deleteQuery = @"DELETE FROM 
                                                    dbo.CandidateSkill 
                                       WHERE        CandidateId = @CandidateId AND SkillId = @SkillId";
                using (var command = new SqlCommand(deleteQuery, connection))
                {
                    //set params
                    command.Parameters.AddWithValue("@CandidateId", candidateId);
                    command.Parameters.AddWithValue("@SkillId", skillId);
                    //excecute - returns true if succeeds
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
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

