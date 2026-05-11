using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.Entities;

public class Application
{
    public int Id { get; set; }
    
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;
    
    public required string CandidateName { get; set; }
    public required string CandidateEmail { get; set; }
    
    public ApplicationStage CurrentStage { get; set; } = ApplicationStage.Applied;
    
    public string? CoverLetter { get; set; }
    
    public CultureFitScore? CultureFitScore { get; set; }
    public InterviewScore? InterviewScore { get; set; }
    public AssessmentScore? AssessmentScore { get; set; }
    
    public ICollection<ApplicationNote> Notes { get; set; } = new List<ApplicationNote>();
    public ICollection<StageHistory> StageHistories { get; set; } = new List<StageHistory>();
}
