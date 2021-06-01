using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elevator.Agent.Manager.Runner.Models;
using Microsoft.Extensions.Logging;

namespace Elevator.Agent.Manager.Api.AgentCommunication
{
    public sealed class AgentsService
    {
        public List<AgentClient> Agents { get; }

        public bool IsServiceReady { get; private set; }


        private readonly ILogger<AgentsService> logger;
        private readonly ILoggerFactory loggerFactory;
            
        public AgentsService(ILogger<AgentsService> logger, ILoggerFactory loggerFactory)
        {
            Agents = new();
            this.logger = logger;
            this.loggerFactory = loggerFactory;
        }

        public void AddAgent(AgentInformation agentInformation)
        {
            var client = new AgentClient(agentInformation.Url, loggerFactory.CreateLogger<AgentClient>());
            Agents.Add(client);
        }

        public void Start()
        {
            IsServiceReady = true;
        }
    }
}
