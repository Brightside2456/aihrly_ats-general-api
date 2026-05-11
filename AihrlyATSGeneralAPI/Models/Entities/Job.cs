using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.Entities;

public class Job
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Location { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Open;
    
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
