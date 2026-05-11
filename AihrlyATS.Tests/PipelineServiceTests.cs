using AihrlyATSGeneralAPI.Models.Enums;
using AihrlyATSGeneralAPI.Services;
using Xunit;

namespace AihrlyATS.Tests;

public class PipelineServiceTests
{
    private readonly PipelineService _service = new();

    [Theory]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Screening, true)]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Rejected, true)]
    [InlineData(ApplicationStage.Applied, ApplicationStage.Interview, false)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Interview, true)]
    [InlineData(ApplicationStage.Screening, ApplicationStage.Rejected, true)]
    [InlineData(ApplicationStage.Interview, ApplicationStage.Offer, true)]
    [InlineData(ApplicationStage.Offer, ApplicationStage.Hired, true)]
    [InlineData(ApplicationStage.Hired, ApplicationStage.Rejected, false)]
    [InlineData(ApplicationStage.Rejected, ApplicationStage.Applied, false)]
    public void IsTransitionValid_ShouldReturnExpectedResult(ApplicationStage from, ApplicationStage to, bool expected)
    {
        var result = _service.IsTransitionValid(from, to);
        Assert.Equal(expected, result);
    }
}
