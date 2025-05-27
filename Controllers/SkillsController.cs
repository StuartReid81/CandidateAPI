using CandidateAPI.Data;
using CandidateAPI.Data.DTOS;
using CandidateAPI.DomainModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CandidateAPI.Controllers
{
    [Route("api/skills/")]
    public class SkillsController : Controller
    {

        private readonly SkillRepo _skillRepo;

        public SkillsController(SkillRepo skillRepo)
        {
            _skillRepo = skillRepo;
        }

        #region GET Requests

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
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

        #region POST requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public IActionResult CreateSkill([FromBody] Skill skill)
        {
            return Json("");
        }

        #endregion

        #region PUT requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="skill"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update/{skillId}")]
        public IActionResult UpdateCandidate(int skillId, [FromBody] Skill skill)
        {
            return Json("");
        }

        #endregion
    }
}
