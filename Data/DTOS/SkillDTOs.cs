namespace CandidateAPI.Data.DTOS
{
    public class SkillDTO {
        public int Id { get; set; }
        public required string SkillName { get; set; }
        public required string DateCreated { get; set; }
        public required string DateUpdated { get; set; }
    }
}
