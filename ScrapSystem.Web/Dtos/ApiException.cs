namespace ScrapSystem.Web.Dtos
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string Response { get; }

        public ApiException(string message, int statusCode, string response) : base(message)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }
}
