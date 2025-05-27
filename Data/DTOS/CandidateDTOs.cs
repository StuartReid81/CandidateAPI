
using CandidateAPI.DomainModels;
using System.ComponentModel.DataAnnotations;

namespace CandidateAPI.Data.DTOS
{
    public class CandidateDTO
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        public DateTime? DOB { get; set; }

        public required string FormattedDOB { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstLineOfAddress { get; set; }

        [Required]
        [MaxLength(50)]
        public required string AddressCity { get; set; }

        [Required]
        [MaxLength(50)]
        public required string AddressCountry { get; set; }

        [Required]
        [MaxLength(20)]
        public required string AddressPostCode { get; set; }

        [MaxLength(50)]
        public string? HomePhoneNo { get; set; }

        [MaxLength(50)]
        public string? MobilePhoneNo { get; set; }

        [MaxLength(50)]
        public string? WorkPhoneNo { get; set; }

        public List<CandidateSkillDTO>? Skills { get; set; }
        public string? FormattedDateLastUpdated { get; set; }
        public string? FormattedDateCreated { get; set; }
    }

    public class AddDeleteCandidateSkillDTO
    {
        [Required]
        public int CandidateID { get; set; }

        [Required]
        public int SkillID { get; set; }
    }

    public class CandidateSkillDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string DateSkillAdded { get; set; }
    }
}
