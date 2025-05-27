using CandidateAPI.DomainModels;

namespace CandidateAPI.Interfaces.Repos
{
    public interface ISkillRepo
    {
        //get
        Task<List<Skill>> GetAllSkillsAsync();
        Task<Skill> GetSkillByIDAsync(int id);

        //post
        Task<int> CreateSkillAsync(Skill skill);

        //put
        Task<bool> UpdateSkillAsync(Skill skill);

        //delete
        Task<bool> DeleteSkillAsync(int skillId);
    }
}