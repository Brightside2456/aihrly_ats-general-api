namespace AihrlyATSGeneralAPI.Models.Entities;

public class TeamMember
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}
