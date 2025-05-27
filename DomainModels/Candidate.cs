using System.ComponentModel.DataAnnotations;

namespace CandidateAPI.DomainModels
{
    public class Candidate
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? Surname { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? Address1 { get; set; }

        [MaxLength(50)]
        public string? Town { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(20)]
        public string? PostCode { get; set; }

        [MaxLength(50)]
        public string? PhoneHome { get; set; }

        [MaxLength(50)]
        public string? PhoneMobile { get; set; }

        [MaxLength(50)]
        public string? PhoneWork { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public virtual List<CandidateSkill>? Skills { get; set; }

    }

}