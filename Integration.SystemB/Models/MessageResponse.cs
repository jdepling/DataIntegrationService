namespace Integration.SystemB.Models
{
    public class MessageResponse
    {
        public int Id { get; set; }

        public string SourceId { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}