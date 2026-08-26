using JobApplica.DataContext;
using JobApplica.Dtos;
using JobApplica.Interfaces;
using JobApplica.ResumeHolder;
using JobApplica.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace JobApplica.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppAnalysisController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private  readonly IResumeAnalysis _resumeAnalysisService;

        public AppAnalysisController(ApplicationDbContext dbContext, IResumeAnalysis resumeAnalysisService)
        {
            _dbContext = dbContext;
            _resumeAnalysisService = resumeAnalysisService;
        }

        [HttpPost("analyse-application")]
        public async Task <ActionResult<ApplicationAnalysisDto>> AnalyseApplication(int resumeId, int jobApplicationId,CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var resume = await _dbContext.Resumes.FirstOrDefaultAsync(x => x.Id == resumeId && x.AppUserId == userId, cancellationToken);

            var job = await _dbContext.Jobapplications.FirstOrDefaultAsync(x => x.Id == jobApplicationId, cancellationToken);


            if(resume == null)
            {
                return NotFound("Resume Not Found");
            }

            if(job == null)
            {
                return NotFound("Job Application Not Found");
            }

            var exsistingAnalysis = await _dbContext.ApplicationAnalyses.FirstOrDefaultAsync(x => x.ResumeId == resumeId && x.JobApplicationId == jobApplicationId, cancellationToken);


            var analysis = await _resumeAnalysisService.ResumeAnalysisAsync(resume.ExtractedText, job.JobDescription, cancellationToken);


            ApplicationAnalysis applicationAnalysis;

            if (exsistingAnalysis != null)
            {
                exsistingAnalysis.MatchScore = analysis.MatchScore;
                exsistingAnalysis.MatchingSkills = string.Join(", ", analysis.MatchingSkills);
                exsistingAnalysis.MissingSkills = string.Join(", ", analysis.MissingSkills);
                exsistingAnalysis.Suggestions = string.Join(", ", analysis.Suggestions);
                exsistingAnalysis.Summary = analysis.Summary;
                exsistingAnalysis.UpdatedAt = DateTime.UtcNow;



                applicationAnalysis = exsistingAnalysis;
            }
            else
            {
                applicationAnalysis = new ApplicationAnalysis
                {
                    ResumeId = resumeId,
                    JobApplicationId = jobApplicationId,
                    MatchScore = analysis.MatchScore,
                    MatchingSkills = string.Join(", ", analysis.MatchingSkills),
                    MissingSkills = string.Join(", ", analysis.MissingSkills),
                    Suggestions = string.Join(", ", analysis.Suggestions),
                    Summary = analysis.Summary,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.ApplicationAnalyses.Add(applicationAnalysis);
            }


            await _dbContext.SaveChangesAsync(cancellationToken);

            var dto = new ApplicationAnalysisDto
            {
                Id = applicationAnalysis.Id,
                ResumeId = applicationAnalysis.ResumeId,
                JobApplicationId = applicationAnalysis.JobApplicationId,

                CompanyName = job.CompanyName,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,

                MatchScore = applicationAnalysis.MatchScore,
                MatchingSkills = applicationAnalysis.MatchingSkills,
                MissingSkills = applicationAnalysis.MissingSkills,
                Suggestions = applicationAnalysis.Suggestions,
                Summary = applicationAnalysis.Summary,
                CreatedAt = applicationAnalysis.CreatedAt

            };

            return Ok(dto);
        }

        [HttpGet]
        public async Task <ActionResult<IEnumerable<ApplicationAnalysisDto>>>GetAnalyses(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                return Unauthorized();
            }

            var anaylses = await _dbContext.ApplicationAnalyses
                .Where(a => a.JobApplication.UserId == userId)
                .Select(a => new ApplicationAnalysisDto
                {
                    Id = a.Id,
                    ResumeId= a.ResumeId,
                    JobApplicationId= a.JobApplicationId,
                    CompanyName = a.JobApplication.CompanyName,
                    JobTitle = a.JobApplication.JobTitle,
                    JobDescription = a.JobApplication.JobDescription,
                    MatchScore = a.MatchScore,
                    MatchingSkills = a.MatchingSkills,
                    MissingSkills = a.MissingSkills,
                    Suggestions = a.Suggestions,
                    Summary = a.Summary,
                    ResumeFileName = a.Resume.FileName,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt

                }).ToListAsync(cancellationToken);


            return Ok(anaylses);
        }

        [HttpGet("{id:int}")]

        public async Task <ActionResult<ApplicationAnalysisDto>>GetAnalysisById(int id,CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var analysis = await _dbContext.ApplicationAnalyses
             .Where(a => a.Id == id && a.JobApplication.UserId == userId)
             .Select(a => new ApplicationAnalysisDto
           {
             Id = a.Id,
             ResumeId = a.ResumeId,
             JobApplicationId = a.JobApplicationId,
             CompanyName = a.JobApplication.CompanyName,
            JobTitle = a.JobApplication.JobTitle,
            JobDescription = a.JobApplication.JobDescription,
            MatchScore = a.MatchScore,
            MatchingSkills = a.MatchingSkills,
            MissingSkills = a.MissingSkills,
            Suggestions = a.Suggestions,
              Summary = a.Summary,
                 ResumeFileName = a.Resume.FileName,
                 CreatedAt = a.CreatedAt,
                 UpdatedAt = a.UpdatedAt
             })
           .FirstOrDefaultAsync(cancellationToken);

            if (analysis == null)
            {
                return NotFound("Analysis not found");
            }

            return Ok(analysis);
        } 
    }
}
