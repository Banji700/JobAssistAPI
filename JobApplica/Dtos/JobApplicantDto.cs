namespace JobApplica.Dtos
{
    public class JobApplicantDto
    {
        public int SubmissionId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int ResumeId { get; set; }
        public string ResumeFileName { get; set; } = string.Empty;

        public int MatchScore { get; set; }
        public string Summary { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;


        public DateTime AppliedAt { get; set; }
    }
}
