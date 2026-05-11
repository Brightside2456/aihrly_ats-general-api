using AihrlyATSGeneralAPI.Data;
using AihrlyATSGeneralAPI.Models.DTOs;
using AihrlyATSGeneralAPI.Models.Entities;
using AihrlyATSGeneralAPI.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AihrlyATSGeneralAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AihrlyDbContext _db;

    public JobsController(AihrlyDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<JobDto>> CreateJob([FromBody] CreateJobDto dto)
    {
        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            Location = dto.Location,
            Status = JobStatus.Open
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, new JobDto(job.Id, job.Title, job.Description, job.Location, job.Status));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDto>>> ListJobs([FromQuery] JobStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Jobs.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JobDto(j.Id, j.Title, j.Description, j.Location, j.Status))
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDto>> GetJob(int id)
    {
        var job = await _db.Jobs.FindAsync(id);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(new JobDto(job.Id, job.Title, job.Description, job.Location, job.Status));
    }
}
