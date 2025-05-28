using CandidateAPI.Data;
using CandidateAPI.Data.DTOS;
using CandidateAPI.DomainModels;
using CandidateAPI.Interfaces.Repos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CandidateAPI.Controllers
{
    [Route("api/skills/")]
    public class SkillsController : Controller
    {

        private readonly ISkillRepo _skillRepo;

        public SkillsController(ISkillRepo skillRepo)
        {
            _skillRepo = skillRepo;
        }

        #region GET Requests

        /// <summary>
        /// get all skills from the db
        /// </summary>
        /// <returns>200 with list of skills in JSON format</returns>
        [HttpGet]
        [Route("get/all")]
        public async Task<IActionResult> GetSkills()
        {
            var skills = await _skillRepo.GetAllSkillsAsync();

            if (skills is null) return BadRequest("Unable to retrieve skills.");

            var skillDTOs = new List<SkillDTO>();

            foreach (var skill in skills)
            {
                skillDTOs.Add(new SkillDTO()
                {
                    Id = skill.ID,
                    SkillName = skill.Name ?? "",
                    DateCreated = skill.CreatedDate.ToString("yyyy-MM-dd"),
                    DateUpdated = skill.UpdatedDate.ToString("yyyy-MM-dd"),
                });
            }

            return Ok(skillDTOs);
        }

        /// <summary>
        /// get a skill by its id
        /// </summary>
        /// <param name="skillId">id of the skill we want to return</param>
        /// <returns>200 with skill in Json format</returns>
        [HttpGet]
        [Route("get/{skillId}")]
        public async Task<IActionResult> GetSkill([FromRoute] int skillId)
        {
            var skill = await _skillRepo.GetSkillByIDAsync(skillId);

            if (skill is null) return BadRequest("Unable to retrieve this skill.");

            var skillDTO = new SkillDTO()
            {
                Id = skill.ID,
                SkillName = skill.Name ?? "",
                DateCreated = skill.CreatedDate.ToString("yyyy-MM-dd"),
                DateUpdated = skill.UpdatedDate.ToString("yyyy-MM-dd"),
            };

            return Ok(skillDTO);
        }

        #endregion
    }
}
