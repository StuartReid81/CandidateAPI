using CandidateAPI.Data;
using CandidateAPI.Data.DTOS;
using CandidateAPI.DomainModels;
using CandidateAPI.Interfaces.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CandidateAPI.Controllers
{
    [Route("api/candidates/")]
    public class CandidatesController : Controller
    {
        private readonly ICandidateRepo _candidateRepo;

        public CandidatesController(ICandidateRepo candidateRepo)
        {
            _candidateRepo = candidateRepo;
        }

        #region GET Requests

        /// <summary>
        /// Get all candidates with skills
        /// </summary>
        /// <returns>OK respons with list of candidates currently in db</returns>
        [HttpGet]
        [Route("get/all")]
        public async Task<IActionResult> GetCandidates()
        {
            var candidates = await _candidateRepo.GetCandidatesWithSkillsAsync();

            var candidateDTOs = new List<CandidateDTO>();

            foreach (var candidate in candidates) {
                var dtoCandidate = new CandidateDTO()
                {
                    Id = candidate.Id,
                    AddressCity = candidate.Town ?? "",
                    AddressCountry = candidate.Country ?? "",
                    Name = candidate.FirstName ?? "",
                    LastName = candidate.Surname ?? "",
                    AddressPostCode = candidate.PostCode ?? "",
                    FirstLineOfAddress = candidate.Address1 ?? "",
                    HomePhoneNo = candidate.PhoneHome ?? "",
                    MobilePhoneNo = candidate.PhoneMobile ?? "",
                    WorkPhoneNo = candidate.PhoneWork ?? "",
                    DOB = candidate.DateOfBirth,
                    FormattedDOB = (candidate.DateOfBirth is not null) ? candidate.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                };

                if (candidate.Skills is not null)
                {
                    dtoCandidate.Skills = new List<CandidateSkillDTO>();
                    foreach (var skill in candidate.Skills)
                    {   
                        dtoCandidate.Skills.Add(new CandidateSkillDTO()
                        {
                            Id = skill.SkillId,
                            Name = (skill.Skill is not null) ? (skill.Skill.Name ?? "") : "",
                            DateSkillAdded = skill.CreatedDate.ToString("yyyy-MM-dd")
                        });
                    }
                }

                candidateDTOs.Add(dtoCandidate);
            }

            return Ok(candidateDTOs);
        }

        /// <summary>
        /// Get candidate by id - associated skills included
        /// </summary>
        /// <param name="candidateId">id of candidate we want from db</param>
        /// <returns></returns>
        [HttpGet]
        [Route("get/{candidateId}")]
        public async Task<IActionResult> GetCandidate([FromRoute] int candidateId)
        {
            var candidate = await _candidateRepo.GetCandidateWithSkillsByIdAsync(candidateId);

            if(candidate is null) return BadRequest("Unable to find a candidate with this ID.");

            var candidateDTO = new CandidateDTO()
            {
                Id = candidate.Id,
                AddressCity = candidate.Town ?? "",
                AddressCountry = candidate.Country ?? "",
                Name = candidate.FirstName ?? "",
                LastName = candidate.Surname ?? "",
                AddressPostCode = candidate.PostCode ?? "",
                FirstLineOfAddress = candidate.Address1 ?? "",
                HomePhoneNo = candidate.PhoneHome ?? "",
                MobilePhoneNo = candidate.PhoneMobile ?? "",
                WorkPhoneNo = candidate.PhoneWork ?? "",
                DOB = candidate.DateOfBirth,
                FormattedDOB = (candidate.DateOfBirth is not null) ? candidate.DateOfBirth.Value.ToString("yyyy-MM-dd") : "",
                FormattedDateLastUpdated = candidate.UpdatedDate.ToString("yyyy-MM-dd"),
                FormattedDateCreated = candidate.CreatedDate.ToString("yyyy-MM-dd")
            };

            if (candidate.Skills is not null)
            {
                candidateDTO.Skills = new List<CandidateSkillDTO>();

                foreach (var skill in candidate.Skills)
                {
                    candidateDTO.Skills.Add(new CandidateSkillDTO()
                    {
                        Id = skill.SkillId,
                        Name = (skill.Skill is not null) ? (skill.Skill.Name ?? "") : "",
                        DateSkillAdded = (candidate.CreatedDate != DateTime.MinValue) ? candidate.CreatedDate.ToString("yyyy-MM-dd") : ""
                    });
                }
            }

            return Ok(candidateDTO);
        }

        #endregion

        #region POST requests

        /// <summary>
        /// Create a new candidate record on db
        /// </summary>
        /// <param name="candidate">DTO with desired candidate details</param>
        /// <returns>returns OK result with created candidate DTO if successful - BadRequest if fails or model is not valid</returns>
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateCandidate([FromBody] CandidateDTO candidate)
        {
            //create candidate object
            var newCandidate = new Candidate() { 
                FirstName = candidate.Name,
                Surname = candidate.LastName,
                DateOfBirth = candidate.DOB,
                CreatedDate = DateTime.UtcNow,
                Address1 = candidate.FirstLineOfAddress,
                Town = candidate.AddressCity,
                Country = candidate.AddressCountry,
                PostCode = candidate.AddressPostCode,
                PhoneHome = candidate.HomePhoneNo,
                PhoneMobile = candidate.MobilePhoneNo,
                PhoneWork = candidate.WorkPhoneNo
            };

            try
            {
                //create
                var createCandidateResult = await _candidateRepo.CreateCandidateAsync(newCandidate);

                //map id
                candidate.Id = createCandidateResult;

                return Json(candidate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Link a skill to a candidate
        /// </summary>
        /// <param name="dto">Data transfer object containing an int skill id we want to link and the int candidate id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("skills/add")]
        public async Task<IActionResult> AddSkillToCandidate([FromBody] AddDeleteCandidateSkillDTO dto) {
            
            //if model is not valid
            if (!ModelState.IsValid) return BadRequest("The model is not valid");
            
            //if create fails
            if (!await _candidateRepo.AddSkillToCandidateAsync(dto.CandidateID, dto.SkillID))
                return BadRequest("There was an error creating this association");
            
            return Ok(new { success = true, message = "Skill added to candidate" });
        }

        #endregion

        #region PUT requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateId"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update/{candidateId}")]
        public async Task<IActionResult> UpdateCandidate(int candidateId, [FromBody] CandidateDTO candidate)
        {
            var originalCandidate = await _candidateRepo.GetCandidateByIDAsync(candidateId);

            if (originalCandidate is null) return BadRequest("Unable to find the user you want to update");

            //create candidate object
            var newCandidate = new Candidate()
            {
                Id = candidateId,
                FirstName = candidate.Name,
                Surname = candidate.LastName,
                DateOfBirth = candidate.DOB,
                UpdatedDate = DateTime.UtcNow,
                Address1 = candidate.FirstLineOfAddress,
                Town = candidate.AddressCity,
                Country = candidate.AddressCountry,
                PostCode = candidate.AddressPostCode,
                PhoneHome = candidate.HomePhoneNo,
                PhoneMobile = candidate.MobilePhoneNo,
                PhoneWork = candidate.WorkPhoneNo
            };

            if (!await _candidateRepo.UpdateCandidateAsync(newCandidate))
                return BadRequest("Unable to update candidate");

            candidate.Id = candidateId;

            return Ok(candidate);
        }

        #endregion

        #region DELETE requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto">Data transfer object holding list of ints relating to skill ids we want to remove and the int client id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("skills/remove")]
        public async Task<IActionResult> DeleteSkillsFromCandidate([FromBody] AddDeleteCandidateSkillDTO dto)
        {
            var result = await _candidateRepo.RemoveSkillFromCandidateAsync(dto.CandidateID, dto.SkillID);

            if (!result) return BadRequest("Unable to remove this skill");

            return Ok(new { success = true, message = "Skill removed." });
        }

        #endregion
    }
}
