namespace AssignmentModule6Client.Models
{
    /// <summary>
    /// Represents an API error response following RFC 7807 Problem Details format.
    /// </summary>
    public class ProblemDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
        public string? TraceId { get; set; }
        
        /// <summary>
        /// Dictionary of validation errors (field name -> error messages)
        /// Used in ValidationProblemDetails responses
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
