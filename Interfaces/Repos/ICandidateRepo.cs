using CandidateAPI.DomainModels;

namespace CandidateAPI.Interfaces.Repos
{
    public interface ICandidateRepo
    {
        //get
        Task<List<Candidate>> GetAllCandidatesAsync();
        Task<List<Candidate>> GetCandidatesWithSkillsAsync();
        Task<Candidate> GetCandidateByIDAsync(int id);
        Task<Candidate> GetCandidateWithSkillsByIdAsync(int id);

        //post
        Task<int> CreateCandidateAsync(Candidate candidate);
        Task<bool> AddSkillToCandidateAsync(int candidateId, int skillId);

        //put
        Task<bool> UpdateCandidateAsync(Candidate candidate);

        //delete
        Task<bool> RemoveSkillFromCandidateAsync(int candidateId, int skillId);
    }
}