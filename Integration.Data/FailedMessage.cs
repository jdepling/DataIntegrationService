namespace Integration.Data
{
    public class FailedMessage
    {
        public Guid Id { get; set; }

        public string SourceId { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public FailureType FailureType { get; set; }

        public int AttemptCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastAttemptAt { get; set; }
    }

    public enum FailureType
    {
        Unknown = 0,
        Transient = 1,
        Permanent = 2
    }
}
