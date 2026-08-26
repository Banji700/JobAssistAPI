using JobApplica.DataContext;
using JobApplica.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace JobApplica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobApplicationController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public JobApplicationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        [Authorize(Roles = "Employer")]
        [HttpPost]
        public async Task<ActionResult<JobApplicationDto>> CreateJobApplication(JobApplicationDto jobApplicationDto)
        {
            var  userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized();
            }

            var jobApplication = new JobApplication
            {
                CompanyName = jobApplicationDto.CompanyName,
                JobTitle = jobApplicationDto.JobTitle,
                JobDescription = jobApplicationDto.JobDescription,
                Status = jobApplicationDto.Status,
                 UserId = userId,
                

            };

            _dbContext.Jobapplications.Add(jobApplication);

            await _dbContext.SaveChangesAsync();

            var response = new JobApplicationDto
            {
                Id = jobApplication.Id,
                CompanyName = jobApplication.CompanyName,
                JobTitle = jobApplication.JobTitle,
                JobDescription = jobApplication.JobDescription,
                Status = jobApplication.Status,
            };

            return Ok(response);
        }

        
        [HttpGet]
        public async Task<ActionResult<List<JobApplicationDto>>>GetJobApplication()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var jobApplications = await _dbContext.Jobapplications
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => new JobApplicationDto
                {
                    Id = j.Id,
                    CompanyName = j.CompanyName,
                    JobTitle = j.JobTitle,
                    JobDescription= j.JobDescription,
                    Status = j.Status,
                    ApplicantCount = _dbContext.Jobsubmissions
                    .Count(s => s.JobApplicationId == j.Id),
                    //  CreatedAt = j.CreatedAt,
                    // UpdatedAt = j.UpdatedAt
                    HasApplied = _dbContext.Jobsubmissions.Any(s =>
                   s.JobApplicationId == j.Id &&
                   s.JobSeekerId == userId),

                }).ToListAsync();

            return Ok(jobApplications);
        }

        
        [HttpGet("{id:int}")]
        public async Task<ActionResult<JobApplicationDto>>GetjobApplicationById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (userId == null)
            {
                return Unauthorized();
            }

            var jobApplicationById = await _dbContext.Jobapplications.FirstOrDefaultAsync(j => j.Id == id);

            if(jobApplicationById == null)
            {
                return NotFound();
            }


            var response = new JobApplicationDto
            {
                Id = jobApplicationById.Id,
                CompanyName = jobApplicationById.CompanyName,
                JobTitle = jobApplicationById.JobTitle,
                JobDescription = jobApplicationById.JobDescription,
                Status = jobApplicationById.Status,
                // CreatedAt = jobApplicationById.CreatedAt,
                // UpdatedAt = jobApplicationById.UpdatedAt,
                HasApplied = await _dbContext.Jobsubmissions
                 .AnyAsync(s =>
                 s.JobApplicationId == jobApplicationById.Id &&
                 s.JobSeekerId == userId),


            };

            return Ok(response);
        }

        [Authorize(Roles = "Employer")]
        [HttpPut("{id:int}")]

        public async Task<ActionResult<JobApplicationDto>>UpdateJobApplication(int id,JobApplicationDto jobApplicationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized();
            }

            var jobApplicationbyId = await _dbContext.Jobapplications.FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

            if(jobApplicationbyId == null)
            {
                return NotFound();
            }

            jobApplicationbyId.CompanyName = jobApplicationDto.CompanyName;
            jobApplicationbyId.JobTitle = jobApplicationDto.JobTitle;
            jobApplicationbyId.JobDescription = jobApplicationDto.JobDescription;
            jobApplicationbyId.Status = jobApplicationDto.Status;
            jobApplicationbyId.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            var response = new JobApplicationDto
            {
                Id= jobApplicationbyId.Id,
                CompanyName= jobApplicationbyId.CompanyName,
                JobTitle= jobApplicationbyId.JobTitle,
                JobDescription= jobApplicationbyId.JobDescription,
                Status = jobApplicationbyId.Status,
               // CreatedAt = jobApplicationbyId.CreatedAt,
               // UpdatedAt = jobApplicationbyId.UpdatedAt,
            };

            return Ok(response);
        }

        [Authorize(Roles = "Employer")]
        [HttpDelete("{id:int}")]
        public async Task <ActionResult>DeleteJobApplication(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized();
            }

            var jobApplicationById = await _dbContext.Jobapplications.FirstOrDefaultAsync(j => j.Id ==id && userId == j.UserId);

            if(jobApplicationById == null)
            {
                return NotFound();
            }


            var hasApplicants = await _dbContext.Jobsubmissions
       .AnyAsync(s => s.JobApplicationId == id);

            if (hasApplicants)
            {
                return Conflict(
                    "This job has applicants and cannot be deleted. Close the job instead."
                );
            }

            _dbContext.Jobapplications.Remove(jobApplicationById);

            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Employer")]
        [HttpGet("mine")]
        public async Task<ActionResult<List<JobApplicationDto>>> GetMyJobPostings()
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (employerId == null)
            {
                return Unauthorized();
            }

            var jobs = await _dbContext.Jobapplications
                .Where(j => j.UserId == employerId)
                .Select(j => new JobApplicationDto
                {
                    Id = j.Id,
                    CompanyName = j.CompanyName,
                    JobTitle = j.JobTitle,
                    JobDescription = j.JobDescription,
                    Status = j.Status,

                    ApplicantCount = _dbContext.Jobsubmissions
                        .Count(s => s.JobApplicationId == j.Id)
                })
                .ToListAsync();

            return Ok(jobs);
        }
    }
}
