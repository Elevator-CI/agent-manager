using System;
using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Runner.Configs
{
    [JsonObject]
    public class AgentRunnerConfiguration
    {
        [JsonProperty]
        public Uri AgentRepositoryUrl { get; set; }
    }
}
