using JobApplica.DataContext;
using JobApplica.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobApplica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSubmissionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public JobSubmissionController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize(Roles = "JobSeeker")]
        [HttpPost]
        public async Task <ActionResult>Apply(CreateJobSubmissionDto createJobSubmissionDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized();
            }

            var job = await _dbContext.Jobapplications.FirstOrDefaultAsync(j => j.Id == createJobSubmissionDto.JobApplicationId);

            if (job == null)
            {
                return NotFound("Job Not Found");
            }

            if (job.Status == "Closed")
            {
                return Conflict(
                    "This job is no longer accepting applications."
                );
            }

            var resume = await _dbContext.Resumes.FirstOrDefaultAsync(r => r.Id == createJobSubmissionDto.ResumeId && r.AppUserId == userId);

            if (resume == null)
            {
                return NotFound("Resume not found");
            }

            var analysis = await _dbContext.ApplicationAnalyses.FirstOrDefaultAsync(a => a.Id == createJobSubmissionDto.ApplicationAnalysisId 
            && a.ResumeId == createJobSubmissionDto.ResumeId && a.JobApplicationId==createJobSubmissionDto.JobApplicationId);

            if (analysis == null)
            {
                return BadRequest("A valid analysis is required before applying.");
            }

            var alreadyApplied = await _dbContext.Jobsubmissions.AnyAsync(s => s.JobApplicationId == createJobSubmissionDto.JobApplicationId && s.JobSeekerId == userId);

            if (alreadyApplied)
            {
                return Conflict("You have already applied for this job.");
            }
            

            var submission = new JobSubmission
            {
                JobApplicationId = createJobSubmissionDto.JobApplicationId,
                ResumeId = createJobSubmissionDto.ResumeId,
                ApplicationAnalysisId = createJobSubmissionDto.ApplicationAnalysisId,
                JobSeekerId = userId
            };

            _dbContext.Jobsubmissions.Add(submission);

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Employer")]
        [HttpGet("job/{jobApplicationId:int}")]
        public async Task<ActionResult<IEnumerable<JobApplicantDto>>> GetApplicants( int jobApplicationId)
        {
            var employerId =User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (employerId == null)
            {
                return Unauthorized();
            }

            var job = await _dbContext.Jobapplications
                .FirstOrDefaultAsync(j =>
                    j.Id == jobApplicationId &&
                    j.UserId == employerId);

            if (job == null)
            {
                return NotFound("Job not found");
            }

            var applicants = await _dbContext.Jobsubmissions
                .Where(s =>
                    s.JobApplicationId == jobApplicationId)
                .Select(s => new JobApplicantDto
                {
                    SubmissionId = s.Id,

                    FirstName = s.JobSeeker.FirstName,
                    LastName = s.JobSeeker.LastName,
                    Email = s.JobSeeker.Email!,

                    ResumeId = s.ResumeId,
                    ResumeFileName = s.Resume.FileName,

                    MatchScore =
                        s.ApplicationAnalysis.MatchScore,

                    Summary =
                        s.ApplicationAnalysis.Summary,

                    Status = s.Status,

                    AppliedAt = s.AppliedAt
                })
                .OrderByDescending(s => s.AppliedAt)
                .ToListAsync();

            return Ok(applicants);
        }

        [Authorize(Roles = "JobSeeker")]
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<MyJobSubmissionDto>>> GetMyApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var applications = await _dbContext.Jobsubmissions
                .Where(s => s.JobSeekerId == userId)
                .OrderByDescending(s => s.AppliedAt)
                .Select(s => new MyJobSubmissionDto
                {
                    SubmissionId = s.Id,

                    JobApplicationId = s.JobApplicationId,

                    CompanyName = s.JobApplication.CompanyName,
                    JobTitle = s.JobApplication.JobTitle,

                    ResumeFileName = s.Resume.FileName,

                    MatchScore = s.ApplicationAnalysis.MatchScore,

                    Status = s.Status,

                    Summary = s.ApplicationAnalysis.Summary,

                    AppliedAt = s.AppliedAt
                })
                .ToListAsync();

            return Ok(applications);
        }

        [Authorize(Roles = "Employer")]
        [HttpPut("{submissionId:int}/status")]
        public async Task<ActionResult> UpdateStatus(int submissionId, UpdateJobSubmissionStatusDto updateDto,CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (employerId == null)
            {
                return Unauthorized();
            }

            var allowedStatuses = new[]
            {
                "Submitted",
                "Reviewing",
                 "Accepted",
                 "Rejected"
            };

            if (!allowedStatuses.Contains(updateDto.Status))
            {
                return BadRequest("Invalid application status.");
            }

            var submission = await _dbContext.Jobsubmissions
             .Include(s => s.JobApplication)
            .FirstOrDefaultAsync(
            s => s.Id == submissionId,
            cancellationToken);

            if (submission == null)
            {
                return NotFound("Application not found.");
            }

            if (submission.JobApplication.UserId != employerId)
            {
                return Forbid();
            }

            submission.Status = updateDto.Status;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();

        }
    }
}
