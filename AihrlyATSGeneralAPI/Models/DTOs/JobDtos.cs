using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.DTOs;

public record JobDto(int Id, string Title, string Description, string Location, JobStatus Status);

public record CreateJobDto(string Title, string Description, string Location);
