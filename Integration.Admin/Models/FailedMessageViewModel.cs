using Integration.Data;

namespace Integration.Admin.Models
{
    public class FailedMessageViewModel
    {
        public Guid Id { get; set; }
        public string SourceId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public FailureType FailureType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAttemptAt { get; set; }
    }
}
