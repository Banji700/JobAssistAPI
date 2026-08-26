using JobApplica.Interfaces;
using System.Text;
using UglyToad.PdfPig;

namespace JobApplica.Services
{
    public class ResumeTextExtractor : IResumeTextExtractor
    {
        public async Task<string> ExtractTextAsync(string filePath, string contentType, CancellationToken cancellationToken)
        {
            if (contentType == "application/pdf")
            {
                return ExtractPdfText(filePath);
            }

            if (contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                return await ExtractDocxTextAsync(filePath, cancellationToken);
            }

            throw new NotSupportedException("Unsupported resume file type.");
        }

        private string ExtractPdfText(string filePath)
        {
            var textbuilder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                textbuilder.AppendLine(page.Text);
            }

            return textbuilder.ToString();
        }
        

        private async Task<string> ExtractDocxTextAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            // DOCX extraction logic will go here
            throw new NotImplementedException();
        }
    }
}
