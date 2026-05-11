using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Models.DTOs;

public record ApplicationDto(int Id, string CandidateName, string CandidateEmail, ApplicationStage CurrentStage);

public record CreateApplicationDto(string Name, string Email, string? CoverLetter);

public record ApplicationProfileDto(
    int Id,
    string CandidateName,
    string CandidateEmail,
    ApplicationStage CurrentStage,
    string? CoverLetter,
    ScoreDto? CultureFit,
    ScoreDto? Interview,
    ScoreDto? Assessment,
    IEnumerable<NoteDto> Notes,
    IEnumerable<StageHistoryDto> StageHistory
);

public record ScoreDto(int Score, string? Comment, string UpdatedBy, DateTime UpdatedAt);

public record NoteDto(int Id, NoteType Type, string Description, string CreatedBy, DateTime CreatedAt);

public record CreateNoteDto(NoteType Type, string Description);

public record StageHistoryDto(ApplicationStage FromStage, ApplicationStage ToStage, string ChangedBy, DateTime ChangedAt, string? Comment);

public record UpdateApplicationStageDto(ApplicationStage TargetStage, string? Comment);

public record UpdateScoreDto(int Score, string? Comment);
