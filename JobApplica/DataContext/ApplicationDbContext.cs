using JobApplica.Dtos;
using JobApplica.LoginRegister;
using JobApplica.ResumeHolder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobApplica.DataContext
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Resume> Resumes => Set<Resume>();
        public DbSet<JobApplication> Jobapplications => Set<JobApplication>();
        public DbSet<ApplicationAnalysis> ApplicationAnalyses => Set<ApplicationAnalysis>();
        
        public DbSet<JobSubmission> Jobsubmissions => Set<JobSubmission>(); 

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<IdentityRole>().HasData(
              new IdentityRole
              {
                Id = "job-seeker-role",
               Name = "JobSeeker",
                NormalizedName = "JOBSEEKER",
               ConcurrencyStamp = "job-seeker-role-stamp"
              },
              new IdentityRole
              {
              Id = "employer-role",
              Name = "Employer",
              NormalizedName = "EMPLOYER",
               ConcurrencyStamp = "employer-role-stamp"
             }
             );

            builder.Entity<Resume>()
                .HasOne(r => r.AppUser)
                .WithMany(u => u.Resumes)
                .HasForeignKey(r => r.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationAnalysis>()
               .HasOne(a => a.Resume)
                 .WithMany()
               .HasForeignKey(a => a.ResumeId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ApplicationAnalysis>()
                .HasOne(a => a.JobApplication)
                .WithMany()
                .HasForeignKey(a => a.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobSubmission>()
             .HasOne(j => j.JobSeeker)
             .WithMany()
             .HasForeignKey(j => j.JobSeekerId)
             .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<JobSubmission>()
              .HasOne(j => j.JobApplication)
              .WithMany()
              .HasForeignKey(j => j.JobApplicationId)
              .OnDelete(DeleteBehavior.NoAction);
              
              
                      builder.Entity<JobSubmission>()
              .HasOne(j => j.Resume)
              .WithMany()
              .HasForeignKey(j => j.ResumeId)
              .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<JobSubmission>()
                .HasOne(j => j.ApplicationAnalysis)
                .WithMany()
                .HasForeignKey(j => j.ApplicationAnalysisId)
                .OnDelete(DeleteBehavior.NoAction);




        }

    }
}
