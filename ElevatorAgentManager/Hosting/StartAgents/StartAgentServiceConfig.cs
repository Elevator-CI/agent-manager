using System;
using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Api.Hosting.StartAgents
{
    [JsonObject]
    public class StartAgentServiceConfig
    {
        [JsonProperty]
        public Uri AgentRepositoryUrl { get; set; }

        [JsonProperty]
        public int CountOfAgents { get; set; }
    }
}
