namespace AihrlyATSGeneralAPI.Models.Entities;

public class Notification
{
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    
    public required string Type { get; set; } // e.g., "Hired", "Rejected"
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
