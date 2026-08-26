using JobApplica.Dtos;

namespace JobApplica.Interfaces
{
    public interface IResumeAnalysis
    {
        Task <ResumeAnalysisDto> ResumeAnalysisAsync (string resumeText,string jobDescription,CancellationToken cancellation);
    }
}
