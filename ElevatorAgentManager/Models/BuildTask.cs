using System;
using System.Collections.Generic;
using Models;
using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Api.Models
{
    [JsonObject]
    public class BuildTask
    {
        public Guid Id { get; set; }
        public Guid BuildId { get; set; }
        public IList<BuildCommand> Commands { get; set; }
        public string ProjectUrl { get; set; }
        public string GitToken { get; set; }
    }
}