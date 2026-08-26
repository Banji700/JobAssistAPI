using JobApplica.LoginRegister;

namespace JobApplica.ResumeHolder
{
    public class Resume
    {
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string FilePath { get; set; } = string.Empty;

        public string ExtractedText {  get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;


        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null;


    }
}
