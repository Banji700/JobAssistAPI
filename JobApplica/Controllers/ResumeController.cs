using JobApplica.DataContext;
using JobApplica.Interfaces;
using JobApplica.ResumeHolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobApplica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IResumeTextExtractor _resumeTextExtractor;
        private readonly IResumeAnalysis _resumeAnalysis;

        public ResumeController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, IResumeTextExtractor resumeTextExtractor,IResumeAnalysis resumeAnalysis)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _resumeTextExtractor = resumeTextExtractor;
            _resumeAnalysis = resumeAnalysis;
            
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException(
                "The authenticated user ID Not found");
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResumeDto>> UploadResume([FromForm] UploadResumeDto uploadDto, CancellationToken cancellationToken)
        {
            var file = uploadDto.File;

            if (file.Length == 0)
            {
                return BadRequest("The upload file is empty");
            }

            const long maximumFileSize = 5 * 1024 * 1024;

            if (file.Length > maximumFileSize)
            {
                return BadRequest("The resume must be 5MB or smaller");
            }

            var permittedExtensions = new[] { ".pdf", ".docx" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!permittedExtensions.Contains(extension))
            {
                return BadRequest("Only PDF and DOCX files are supported.");
            }

            var uploadFolder = Path.Combine(
                _webHostEnvironment.ContentRootPath,
                "Uploads",
                "Resumes");

            Directory.CreateDirectory(uploadFolder);

            var storedFileName = $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(uploadFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var extractedText = await _resumeTextExtractor.ExtractTextAsync(filePath, file.ContentType, cancellationToken);

            var resume = new Resume
            {
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = filePath,
                AppUserId = GetCurrentUserId(),
                ExtractedText = extractedText,
            };

            _dbContext.Resumes.Add(resume);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetResumeById), new { id = resume.Id }, new ResumeDto
            {
                Id = resume.Id,
                FileName = resume.FileName,
                ContentType = resume.ContentType,
                FileSize = resume.FileSize,
                UploadedAt = resume.UploadedAt,
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResumeDto>> GetResumeById(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var resume = await _dbContext.Resumes
                .AsNoTracking()
                .FirstOrDefaultAsync(resume =>
                    resume.Id == id &&
                    resume.AppUserId == userId, cancellationToken);

            if (resume is null)
            {
                return NotFound();
            }

            return Ok(new ResumeDto
            {
                Id = resume.Id,
                FileName = resume.FileName,
                ContentType = resume.ContentType,
                FileSize = resume.FileSize,
                UploadedAt = resume.UploadedAt
            });
        }

        [HttpGet]
        public async Task<ActionResult<List<ResumeDto>>> GetMyResumes(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            
            var resumes = await _dbContext.Resumes
                .AsNoTracking()
                .Where(resume => resume.AppUserId == userId)
                .OrderByDescending(resume => resume.UploadedAt)
                .Select(resume => new ResumeDto
                {
                    Id = resume.Id,
                    FileName = resume.FileName,
                    ContentType = resume.ContentType, 
                    FileSize = resume.FileSize,
                    UploadedAt =resume.UploadedAt
                })
                .ToListAsync(cancellationToken);

            return Ok(resumes);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteResume(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var resume = await _dbContext.Resumes
                .FirstOrDefaultAsync(resume => resume.Id == id && resume.AppUserId == userId, cancellationToken);

            if(resume == null)
            {
                return NotFound();
            }
            var usedInSubmission = await _dbContext.Jobsubmissions
               .AnyAsync(s => s.ResumeId == id, cancellationToken);

            if (usedInSubmission)
            {
                return Conflict(
                    "This resume has been used in a job application and cannot be deleted."
                );
            }

            var analyses = await _dbContext.ApplicationAnalyses
             .Where(a => a.ResumeId == id)
             .ToListAsync(cancellationToken);

            _dbContext.ApplicationAnalyses.RemoveRange(analyses);

            if (System.IO.File.Exists(resume.FileName))
            {
                System.IO.File.Delete(resume.FileName);
            }

            _dbContext.Resumes.Remove(resume);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        [HttpPost("{id:int}/analyze")]
        public async Task<ActionResult> AnalyzeResume(int id, AnalyzeResumeRequest request,CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var resume = await _dbContext.Resumes
                .FirstOrDefaultAsync(resume => resume.Id == id && resume.AppUserId == userId, cancellationToken);

            if (resume == null)
            {
                return NotFound();
            }

            var analysis = await _resumeAnalysis.ResumeAnalysisAsync(resume.ExtractedText,request.JobDescription, cancellationToken);

            return Ok(analysis);
        }

        [Authorize(Roles = "Employer")]
        [HttpGet("{id:int}/file")]
        public async Task<ActionResult> GetResumeFile(int id,CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (employerId == null)
            {
                return Unauthorized();
            }

            var resume = await _dbContext.Resumes
                .FirstOrDefaultAsync(
                    r => r.Id == id,
                    cancellationToken);

            if (resume == null)
            {
                return NotFound("Resume not found.");
            }

            var hasAccess = await _dbContext.Jobsubmissions
                .AnyAsync(
                    s =>
                        s.ResumeId == id &&
                        s.JobApplication.UserId == employerId,
                    cancellationToken);

            if (!hasAccess)
            {
                return Forbid();
            }

            if (!System.IO.File.Exists(resume.FilePath))
            {
                return NotFound("Resume file not found.");
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(
                resume.FilePath,
                cancellationToken);

            return File(
                bytes,
                resume.ContentType,
                resume.FileName
            );
        }

       
    }
}
