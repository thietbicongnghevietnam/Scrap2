namespace ScrapSystem.Api.Application.Request
{
    public class VerifyRequest
    {
        public IFormFile File { get; set; }

        public string Type { get; set; }
    }

    public class ImportRequest
    {
        public IFormFile File { get; set; }
        public string Sanction { get; set; }
        public string Section { get; set; }
        public string issueout { get; set; }
    }
}
