using System.ComponentModel.DataAnnotations;

namespace JobApplica.Dtos
{
    public class AnalyseResumeDto
    {
        [Required]
        public int ResumeId { get; set; }

        [Required]
        public string JobDescription { get; set; } = string.Empty;
    }
}
