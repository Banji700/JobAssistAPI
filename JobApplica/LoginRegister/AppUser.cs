using JobApplica.Dtos;
using JobApplica.ResumeHolder;
using Microsoft.AspNetCore.Identity;

namespace JobApplica.LoginRegister
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public ICollection<Resume> Resumes { get; set; } = [];

        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();


    }
}
