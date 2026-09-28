namespace Integration.Worker.Models
{
    public class SystemBMessageRequest
    {
        public string SourceId { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}