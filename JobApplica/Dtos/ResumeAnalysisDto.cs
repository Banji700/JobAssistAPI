namespace JobApplica.Dtos
{
    public class ResumeAnalysisDto
    {
        public int MatchScore { get; set; }

        public List<string> MatchingSkills { get; set; } = [];

        public List<string> MissingSkills { get; set; } = [];

        public List<string> Suggestions { get; set; } = [];


        public string Summary { get; set; } = string.Empty;


    }
}
