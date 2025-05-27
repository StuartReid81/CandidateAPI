using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CandidateAPI.DomainModels
{
    public class CandidateSkill
    {
        public int CandidateId { get; set; }

        public virtual Candidate? Candidate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public int SkillId { get; set; }

        [ForeignKey("SkillId")]
        public virtual Skill? Skill { get; set; }
    }

}