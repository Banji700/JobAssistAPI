using JobApplica.ResumeHolder;

namespace JobApplica.Dtos
{
    public class ApplicationAnalysisDto
    {
        public int Id { get; set; }

        public int ResumeId { get; set; }

        public int JobApplicationId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public int MatchScore { get; set; }

        public string MatchingSkills { get; set; } = string.Empty;

        public string MissingSkills { get; set; } = string.Empty;

        public string Suggestions { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string ResumeFileName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
