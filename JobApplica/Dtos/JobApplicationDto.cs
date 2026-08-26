using JobApplica.LoginRegister;
using System.ComponentModel.DataAnnotations;

namespace JobApplica.Dtos
{
    public class JobApplicationDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string JobTitle { get; set; } = string.Empty;
        [Required]
        [MaxLength(5000)]
        public string JobDescription { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Open";

        public bool HasApplied { get; set; }

        public int ApplicantCount { get; set; }

        //public DateTime CreatedAt { get; set; }

        //public DateTime UpdatedAt { get; set; }


    }
}
