using AihrlyATSGeneralAPI.Data;
using AihrlyATSGeneralAPI.Infrastructure;
using AihrlyATSGeneralAPI.Models.DTOs;
using AihrlyATSGeneralAPI.Models.Entities;
using AihrlyATSGeneralAPI.Models.Enums;
using AihrlyATSGeneralAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AihrlyATSGeneralAPI.Controllers;

[ApiController]
[Route("api")]
public class ApplicationsController : ControllerBase
{
    private readonly AihrlyDbContext _db;
    private readonly IPipelineService _pipeline;
    private readonly INotificationQueue _notificationQueue;

    public ApplicationsController(AihrlyDbContext db, IPipelineService pipeline, INotificationQueue notificationQueue)
    {
        _db = db;
        _pipeline = pipeline;
        _notificationQueue = notificationQueue;
    }

    [HttpPost("jobs/{jobId}/applications")]
    public async Task<ActionResult<ApplicationDto>> CreateApplication(int jobId, [FromBody] CreateApplicationDto dto)
    {
        var job = await _db.Jobs.FindAsync(jobId);
        if (job == null) return NotFound("Job not found.");
        if (job.Status == JobStatus.Closed) return BadRequest("Cannot apply to a closed job.");

        var existing = await _db.Applications.AnyAsync(a => a.JobId == jobId && a.CandidateEmail == dto.Email);
        if (existing) return Conflict("Candidate has already applied to this job.");

        var application = new Application
        {
            JobId = jobId,
            CandidateName = dto.Name,
            CandidateEmail = dto.Email,
            CoverLetter = dto.CoverLetter,
            CurrentStage = ApplicationStage.Applied
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApplication), new { id = application.Id }, new ApplicationDto(application.Id, application.CandidateName, application.CandidateEmail, application.CurrentStage));
    }

    [HttpGet("jobs/{jobId}/applications")]
    [AuthorizeTeamMember]
    public async Task<ActionResult<IEnumerable<ApplicationDto>>> ListApplications(int jobId, [FromQuery] ApplicationStage? stage)
    {
        var query = _db.Applications.Where(a => a.JobId == jobId);
        if (stage.HasValue) query = query.Where(a => a.CurrentStage == stage.Value);

        var apps = await query
            .Select(a => new ApplicationDto(a.Id, a.CandidateName, a.CandidateEmail, a.CurrentStage))
            .ToListAsync();

        return Ok(apps);
    }

    [HttpGet("applications/{id}")]
    [AuthorizeTeamMember]
    public async Task<ActionResult<ApplicationProfileDto>> GetApplication(int id)
    {
        var app = await _db.Applications
            .Include(a => a.CultureFitScore).ThenInclude(s => s!.UpdatedBy)
            .Include(a => a.InterviewScore).ThenInclude(s => s!.UpdatedBy)
            .Include(a => a.AssessmentScore).ThenInclude(s => s!.UpdatedBy)
            .Include(a => a.Notes).ThenInclude(n => n.CreatedBy)
            .Include(a => a.StageHistories).ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app == null) return NotFound();

        var dto = new ApplicationProfileDto(
            app.Id,
            app.CandidateName,
            app.CandidateEmail,
            app.CurrentStage,
            app.CoverLetter,
            app.CultureFitScore != null ? new ScoreDto(app.CultureFitScore.Score, app.CultureFitScore.Comment, app.CultureFitScore.UpdatedBy.Name, app.CultureFitScore.UpdatedAt) : null,
            app.InterviewScore != null ? new ScoreDto(app.InterviewScore.Score, app.InterviewScore.Comment, app.InterviewScore.UpdatedBy.Name, app.InterviewScore.UpdatedAt) : null,
            app.AssessmentScore != null ? new ScoreDto(app.AssessmentScore.Score, app.AssessmentScore.Comment, app.AssessmentScore.UpdatedBy.Name, app.AssessmentScore.UpdatedAt) : null,
            app.Notes.OrderByDescending(n => n.CreatedAt).Select(n => new NoteDto(n.Id, n.Type, n.Description, n.CreatedBy.Name, n.CreatedAt)),
            app.StageHistories.OrderByDescending(h => h.ChangedAt).Select(h => new StageHistoryDto(h.FromStage, h.ToStage, h.ChangedBy.Name, h.ChangedAt, h.Comment))
        );

        return Ok(dto);
    }

    [HttpPatch("applications/{id}/stage")]
    [AuthorizeTeamMember]
    public async Task<ActionResult> UpdateStage(int id, [FromBody] UpdateApplicationStageDto dto)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app == null) return NotFound();

        if (!_pipeline.IsTransitionValid(app.CurrentStage, dto.TargetStage))
        {
            return BadRequest($"Invalid stage transition from {app.CurrentStage} to {dto.TargetStage}.");
        }

        var oldStage = app.CurrentStage;
        app.CurrentStage = dto.TargetStage;

        var teamMember = HttpContext.GetTeamMember();
        var history = new StageHistory
        {
            ApplicationId = id,
            FromStage = oldStage,
            ToStage = dto.TargetStage,
            ChangedById = teamMember.Id,
            Comment = dto.Comment
        };

        _db.StageHistories.Add(history);
        await _db.SaveChangesAsync();

        if (dto.TargetStage == ApplicationStage.Hired || dto.TargetStage == ApplicationStage.Rejected)
        {
            await _notificationQueue.QueueNotificationAsync(new NotificationTask(id, dto.TargetStage.ToString()));
        }

        return NoContent();
    }

    [HttpPost("applications/{id}/notes")]
    [AuthorizeTeamMember]
    public async Task<ActionResult<NoteDto>> AddNote(int id, [FromBody] CreateNoteDto dto)
    {
        var app = await _db.Applications.FindAsync(id);
        if (app == null) return NotFound();

        var teamMember = HttpContext.GetTeamMember();
        var note = new ApplicationNote
        {
            ApplicationId = id,
            Type = dto.Type,
            Description = dto.Description,
            CreatedById = teamMember.Id
        };

        _db.ApplicationNotes.Add(note);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotes), new { id }, new NoteDto(note.Id, note.Type, note.Description, teamMember.Name, note.CreatedAt));
    }

    [HttpGet("applications/{id}/notes")]
    [AuthorizeTeamMember]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes(int id)
    {
        var notes = await _db.ApplicationNotes
            .Where(n => n.ApplicationId == id)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NoteDto(n.Id, n.Type, n.Description, n.CreatedBy.Name, n.CreatedAt))
            .ToListAsync();

        return Ok(notes);
    }

    [HttpPut("applications/{id}/scores/culture-fit")]
    [AuthorizeTeamMember]
    public async Task<ActionResult> UpdateCultureFitScore(int id, [FromBody] UpdateScoreDto dto)
    {
        if (dto.Score < 1 || dto.Score > 5) return BadRequest("Score must be between 1 and 5.");

        var app = await _db.Applications.Include(a => a.CultureFitScore).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var teamMember = HttpContext.GetTeamMember();
        if (app.CultureFitScore == null)
        {
            app.CultureFitScore = new CultureFitScore { ApplicationId = id, Score = dto.Score, Comment = dto.Comment, UpdatedById = teamMember.Id };
        }
        else
        {
            app.CultureFitScore.Score = dto.Score;
            app.CultureFitScore.Comment = dto.Comment;
            app.CultureFitScore.UpdatedById = teamMember.Id;
            app.CultureFitScore.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("applications/{id}/scores/interview")]
    [AuthorizeTeamMember]
    public async Task<ActionResult> UpdateInterviewScore(int id, [FromBody] UpdateScoreDto dto)
    {
        if (dto.Score < 1 || dto.Score > 5) return BadRequest("Score must be between 1 and 5.");

        var app = await _db.Applications.Include(a => a.InterviewScore).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var teamMember = HttpContext.GetTeamMember();
        if (app.InterviewScore == null)
        {
            app.InterviewScore = new InterviewScore { ApplicationId = id, Score = dto.Score, Comment = dto.Comment, UpdatedById = teamMember.Id };
        }
        else
        {
            app.InterviewScore.Score = dto.Score;
            app.InterviewScore.Comment = dto.Comment;
            app.InterviewScore.UpdatedById = teamMember.Id;
            app.InterviewScore.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("applications/{id}/scores/assessment")]
    [AuthorizeTeamMember]
    public async Task<ActionResult> UpdateAssessmentScore(int id, [FromBody] UpdateScoreDto dto)
    {
        if (dto.Score < 1 || dto.Score > 5) return BadRequest("Score must be between 1 and 5.");

        var app = await _db.Applications.Include(a => a.AssessmentScore).FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var teamMember = HttpContext.GetTeamMember();
        if (app.AssessmentScore == null)
        {
            app.AssessmentScore = new AssessmentScore { ApplicationId = id, Score = dto.Score, Comment = dto.Comment, UpdatedById = teamMember.Id };
        }
        else
        {
            app.AssessmentScore.Score = dto.Score;
            app.AssessmentScore.Comment = dto.Comment;
            app.AssessmentScore.UpdatedById = teamMember.Id;
            app.AssessmentScore.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
