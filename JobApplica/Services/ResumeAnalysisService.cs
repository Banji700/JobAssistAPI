using JobApplica.Dtos;
using JobApplica.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace JobApplica.Services
{
    public class ResumeAnalysisService : IResumeAnalysis
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _geminiSettings;

        public ResumeAnalysisService(HttpClient httpClient, IOptions<GeminiSettings> geminiSettings)
        {
            _httpClient = httpClient;
            _geminiSettings = geminiSettings.Value;
        }

        public async Task<ResumeAnalysisDto> ResumeAnalysisAsync(string resumeText, string jobDescription, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(resumeText))
            {
                throw new ArgumentNullException("Resume Text Is Required", nameof(resumeText));

            }

            if (string.IsNullOrWhiteSpace(jobDescription))
            {
                throw new ArgumentException("Job description is required.",nameof(jobDescription));
            }

             var prompt = $"""
             You are a professional resume analyser.
            
             Compare the resume with the job description.
            
             Return:
             - A match score from 0 to 100
             - Skills found in both the resume and job description
             - Skills required by the job but missing from the resume
             - Practical suggestions for improving the resume
             - A short overall summary
            
             Only mention skills that are supported by the supplied text.
             Do not invent experience or qualifications.
            
             RESUME:
             {resumeText}
            
             JOB DESCRIPTION:
             {jobDescription}
             """;

            var requestBody = new
            {
                contents = new[]
    {
        new
        {
            parts = new[]
            {
                new
                {
                    text = prompt
                }
            }
        }
    },

                generationConfig = new
                {
                    responseMimeType = "application/json",

                    responseSchema = new
                    {
                        type = "object",

                        properties = new
                        {
                            matchScore = new
                            {
                                type = "integer"
                            },

                            matchingSkills = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "string"
                                }
                            },

                            missingSkills = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "string"
                                }
                            },

                            suggestions = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "string"
                                }
                            },

                            summary = new
                            {
                                type = "string"
                            }
                        },

                        required = new[]
            {
                "matchScore",
                "matchingSkills",
                "missingSkills",
                "suggestions",
                "summary"
            }
                    }
                }
            };

                     var url =
             $"https://generativelanguage.googleapis.com/v1beta/models/" +
             $"{_geminiSettings.Model}:generateContent";
            
                     using var request = new HttpRequestMessage(
                         HttpMethod.Post,
                         url);
            
                     request.Headers.Add(
                         "x-goog-api-key",
                         _geminiSettings.ApiKey);
            
                     request.Content = JsonContent.Create(requestBody);

            using var response = await SendWithRetryAsync(
            requestBody,
            cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                throw new HttpRequestException(
                    $"Gemini request failed with status " +
                    $"{response.StatusCode}: {errorContent}");
            }

            var geminiResponse =
            await response.Content.ReadFromJsonAsync<GeminiResponse>(
            cancellationToken: cancellationToken);

            var jsonResult = geminiResponse?

              .Candidates
              .FirstOrDefault()?
              .Content
              .Parts
              .FirstOrDefault()?
              .Text;


            if (string.IsNullOrWhiteSpace(jsonResult))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty analysis.");
            }

            var analysis =
           JsonSerializer.Deserialize<ResumeAnalysisDto>(
          jsonResult,
          new JsonSerializerOptions
          {
            PropertyNameCaseInsensitive = true
          });

            if (analysis is null)
            {
                throw new InvalidOperationException(
                    "Gemini returned an invalid analysis.");
            }

            return analysis;
        }

        private async Task<HttpResponseMessage> SendWithRetryAsync(
            object requestBody,
             CancellationToken cancellationToken)
        {
            const int maximumAttempts = 3;

            for (var attempt = 1; attempt <= maximumAttempts; attempt++)
            {
                var url =
                    $"https://generativelanguage.googleapis.com/v1beta/models/" +
                    $"{_geminiSettings.Model}:generateContent";

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

                request.Headers.Add(
                    "x-goog-api-key",
                    _geminiSettings.ApiKey);

                request.Content = JsonContent.Create(requestBody);

                var response = await _httpClient.SendAsync(
                    request,
                    cancellationToken);

                if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return response;
                }

                if (attempt == maximumAttempts)
                {
                    return response;
                }

                response.Dispose();

                var delayInSeconds = Math.Pow(2, attempt);

                await Task.Delay(
                    TimeSpan.FromSeconds(delayInSeconds),
                    cancellationToken);
            }

            throw new InvalidOperationException(
                "The Gemini request could not be completed.");
        }
    }
}
