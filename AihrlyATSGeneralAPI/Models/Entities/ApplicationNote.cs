using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.Entities;

public class ApplicationNote
{
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    
    public NoteType Type { get; set; }
    public required string Description { get; set; }
    
    public int CreatedById { get; set; }
    public TeamMember CreatedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
