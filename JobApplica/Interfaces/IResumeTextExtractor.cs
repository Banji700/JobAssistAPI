namespace JobApplica.Interfaces
{
    public interface IResumeTextExtractor
    {
        Task<string> ExtractTextAsync(string filePath, string contentType,CancellationToken cancellationToken);
    }
}
