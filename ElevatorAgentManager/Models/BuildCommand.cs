using System.Collections;
using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Api.Models
{
    [JsonObject]
    public class BuildCommand
    {
        public string Command { get; set; }

        public string Arguments { get; set; }
    }
}