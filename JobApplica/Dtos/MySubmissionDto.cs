namespace JobApplica.Dtos
{
    public class MyJobSubmissionDto
    {
        public int SubmissionId { get; set; }

        public int JobApplicationId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string ResumeFileName { get; set; } = string.Empty;

        public int MatchScore { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; }
    }
}
