using JobApplica.LoginRegister;

namespace JobApplica.Dtos
{
    public class JobApplication
    {
        public int Id { get; set; }



        public string CompanyName { get; set; } = string.Empty;

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
    }
}
