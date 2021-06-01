using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Api.AgentCommunication.Models
{
    public enum AgentStatus
    {
        Free,
        Working,
        Finished
    }

    [JsonObject]
    public class StatusResponse
    {
        public AgentStatus Status { get; set; }
    }
}
