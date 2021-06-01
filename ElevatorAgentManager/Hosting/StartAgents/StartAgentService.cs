using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elevator.Agent.Manager.Api.AgentCommunication;
using Elevator.Agent.Manager.Api.Utils;
using Elevator.Agent.Manager.Runner;
using Elevator.Agent.Manager.Runner.Configs;
using Elevator.Agent.Manager.Runner.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elevator.Agent.Manager.Api.Hosting.StartAgents
{
    public class StartAgentService: IHostedService
    {
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<StartAgentService> logger;
        private readonly IOptions<StartAgentServiceConfig> configuration;
        private readonly AgentsService agentsService;

        public StartAgentService(IOptions<StartAgentServiceConfig> configuration, ILoggerFactory loggerFactory, AgentsService agentsService)
        {
            this.loggerFactory = loggerFactory;
            this.agentsService = agentsService;
            logger = loggerFactory.CreateLogger<StartAgentService>();

            this.configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Starting {configuration.Value.CountOfAgents} agents");
            var agentRunner = new AgentRunner(new AgentRunnerConfiguration
            {
                AgentRepositoryUrl = configuration.Value.AgentRepositoryUrl
            }, loggerFactory);

            var info = FreeTcpPortFinder.GetAvailablePort(10000, configuration.Value.CountOfAgents).Select(x => new StartAgentInformation(x)).ToArray();
            var runAgentsResult = await agentRunner.RunAgentsAsync(info);
            
            if (!runAgentsResult.IsSuccessful)
            {
                logger.LogError(runAgentsResult.Error);
            }
            else
            {
                foreach (var agentInformation in runAgentsResult.Value)
                {
                    agentsService.AddAgent(agentInformation);
                    logger.LogInformation($"Started agent on {agentInformation.Url}");
                }
            }

            agentsService.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
