namespace Integration.Data
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        /// <summary>
        ///    This is the id of the record in System A.
        /// </summary>
        public string SourceId { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the date and time when the message was published to the message broker.
        ///     If it is null, it means the message has not been published yet.
        /// </summary>
        public DateTime? PublishedAt { get; set; }

        public int RetryCount { get; set; }

        public string? LastError { get; set; }
    }
}
