using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public StartAgentService(IOptions<StartAgentServiceConfig> configuration, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
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
                    logger.LogInformation(agentInformation.Url.ToString());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
