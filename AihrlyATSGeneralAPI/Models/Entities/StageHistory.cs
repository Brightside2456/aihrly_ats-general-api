using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.Entities;

public class StageHistory
{
    public int Id { get; set; }
    
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    
    public ApplicationStage FromStage { get; set; }
    public ApplicationStage ToStage { get; set; }
    
    public int ChangedById { get; set; }
    public TeamMember ChangedBy { get; set; } = null!;
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    public string? Comment { get; set; }
}
