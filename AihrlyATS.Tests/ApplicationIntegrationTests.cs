using System.Net;
using System.Net.Http.Json;
using AihrlyATSGeneralAPI.Models.DTOs;
using AihrlyATSGeneralAPI.Models.Enums;
using Xunit;

namespace AihrlyATS.Tests;

public class ApplicationIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApplicationIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateApplication_And_AddNote_ShouldWork()
    {
        // 1. Create a Job
        var createJobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto("Dev", "Desc", "Remote"));
        var job = await createJobResponse.Content.ReadFromJsonAsync<JobDto>();

        // 2. Create Application
        var createAppResponse = await _client.PostAsJsonAsync($"/api/jobs/{job!.Id}/applications", new CreateApplicationDto("Jane", "jane@test.com", "CL"));
        var app = await createAppResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        // 3. Add Note (Needs header)
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/applications/{app!.Id}/notes")
        {
            Content = JsonContent.Create(new CreateNoteDto(NoteType.General, "Nice candidate"))
        };
        request.Headers.Add("X-Team-Member-Id", "1"); // Alice Recruiter (from seed)
        
        var noteResponse = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, noteResponse.StatusCode);

        // 4. Read Profile
        var getProfileRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/applications/{app!.Id}");
        getProfileRequest.Headers.Add("X-Team-Member-Id", "1");
        var profileResponse = await _client.SendAsync(getProfileRequest);
        var profile = await profileResponse.Content.ReadFromJsonAsync<ApplicationProfileDto>();

        Assert.Single(profile!.Notes);
        Assert.Equal("Alice Recruiter", profile.Notes.First().CreatedBy);
    }

    [Fact]
    public async Task DuplicateApplication_ShouldReturnConflict()
    {
        // 1. Create a Job
        var createJobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto("Dev2", "Desc", "Remote"));
        var job = await createJobResponse.Content.ReadFromJsonAsync<JobDto>();

        // 2. First Application
        await _client.PostAsJsonAsync($"/api/jobs/{job!.Id}/applications", new CreateApplicationDto("Jane", "jane2@test.com", "CL"));

        // 3. Duplicate Application
        var duplicateResponse = await _client.PostAsJsonAsync($"/api/jobs/{job.Id}/applications", new CreateApplicationDto("Jane", "jane2@test.com", "CL"));
        
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateScoreTwice_ShouldOverwriteAndTrackAttribution()
    {
        // 1. Setup
        var createJobResponse = await _client.PostAsJsonAsync("/api/jobs", new CreateJobDto("Dev3", "Desc", "Remote"));
        var job = await createJobResponse.Content.ReadFromJsonAsync<JobDto>();
        var createAppResponse = await _client.PostAsJsonAsync($"/api/jobs/{job!.Id}/applications", new CreateApplicationDto("Jane", "jane3@test.com", "CL"));
        var app = await createAppResponse.Content.ReadFromJsonAsync<ApplicationDto>();

        // 2. First Score (Alice)
        var score1Request = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{app!.Id}/scores/culture-fit")
        {
            Content = JsonContent.Create(new UpdateScoreDto(3, "Average"))
        };
        score1Request.Headers.Add("X-Team-Member-Id", "1");
        await _client.SendAsync(score1Request);

        // 3. Second Score (Bob)
        var score2Request = new HttpRequestMessage(HttpMethod.Put, $"/api/applications/{app.Id}/scores/culture-fit")
        {
            Content = JsonContent.Create(new UpdateScoreDto(5, "Excellent!"))
        };
        score2Request.Headers.Add("X-Team-Member-Id", "2");
        await _client.SendAsync(score2Request);

        // 4. Verify
        var getProfileRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/applications/{app.Id}");
        getProfileRequest.Headers.Add("X-Team-Member-Id", "1");
        var profileResponse = await _client.SendAsync(getProfileRequest);
        var profile = await profileResponse.Content.ReadFromJsonAsync<ApplicationProfileDto>();

        Assert.Equal(5, profile!.CultureFit!.Score);
        Assert.Equal("Bob Manager", profile.CultureFit.UpdatedBy);
    }
}
