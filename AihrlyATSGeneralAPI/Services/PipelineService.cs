using AihrlyATSGeneralAPI.Models.Enums;

namespace AihrlyATSGeneralAPI.Services;

public interface IPipelineService
{
    bool IsTransitionValid(ApplicationStage from, ApplicationStage to);
}

public class PipelineService : IPipelineService
{
    public bool IsTransitionValid(ApplicationStage from, ApplicationStage to)
    {
        if (from == to) return true;

        return from switch
        {
            ApplicationStage.Applied => to == ApplicationStage.Screening || to == ApplicationStage.Rejected,
            ApplicationStage.Screening => to == ApplicationStage.Interview || to == ApplicationStage.Rejected,
            ApplicationStage.Interview => to == ApplicationStage.Offer || to == ApplicationStage.Rejected,
            ApplicationStage.Offer => to == ApplicationStage.Hired || to == ApplicationStage.Rejected,
            ApplicationStage.Hired => false,
            ApplicationStage.Rejected => false,
            _ => false
        };
    }
}
