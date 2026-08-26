using JobApplica.LoginRegister;
using JobApplica.ResumeHolder;

namespace JobApplica.Dtos
{
    public class JobSubmission
    {
        public int Id { get; set; }

        public int JobApplicationId { get; set; }
        public JobApplication JobApplication { get; set; } = null!;

        public string JobSeekerId { get; set; } = string.Empty;
        public AppUser JobSeeker { get; set; } = null!;

        public int ResumeId { get; set; }
        public Resume Resume { get; set; } = null!;

        public int ApplicationAnalysisId { get; set; }
        public ApplicationAnalysis ApplicationAnalysis { get; set; } = null!;

        public string Status { get; set; } = "Submitted";

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
