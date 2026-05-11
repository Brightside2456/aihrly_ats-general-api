namespace AihrlyATSGeneralAPI.Models.Entities;

public class AssessmentScore
{
    public int Id { get; set; }
    
    public int UpdatedById { get; set; }
    public TeamMember UpdatedBy { get; set; } = null!;
    
    public int Score { get; set; }
    public string? Comment { get; set; }
    
    public int ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
