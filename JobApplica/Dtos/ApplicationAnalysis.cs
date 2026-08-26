using JobApplica.ResumeHolder;

namespace JobApplica.Dtos
{
    public class ApplicationAnalysis
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }

        public Resume Resume { get; set; } = null!;

        public int JobApplicationId { get; set; }

        public JobApplication JobApplication { get; set; } = null!;

        public int MatchScore { get; set; }

        public string MatchingSkills { get; set; } = string.Empty;

        public string MissingSkills { get; set; } = string.Empty;

        public string Suggestions { get; set; } = string.Empty;


        public string Summary { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
