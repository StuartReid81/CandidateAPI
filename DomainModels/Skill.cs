using System.ComponentModel.DataAnnotations;

namespace CandidateAPI.DomainModels
{
    public class Skill
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(50)]
        public string? Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}