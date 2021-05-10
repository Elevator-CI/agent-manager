using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common;
using Elevator.Agent.Manager.Runner.Configs;
using Elevator.Agent.Manager.Runner.Models;
using Git;
using Microsoft.Extensions.Logging;
using Shell;

namespace Elevator.Agent.Manager.Runner
{
    public class AgentRunner
    {
        private readonly AgentRunnerConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<AgentRunner> logger;

        public AgentRunner(AgentRunnerConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            logger = loggerFactory.CreateLogger<AgentRunner>();
        }

        public async Task<OperationResult<IReadOnlyCollection<AgentInformation>>> RunAgentsAsync(
            params StartAgentInformation[] startAgentInformations)
        {
            var workingDirectory = GetTempWorkingDirectory();
            var getAgentRepositoryResult = await GetAgentGitRepositoryAsync(workingDirectory);

            if (!getAgentRepositoryResult.IsSuccessful)
                return OperationResult<IReadOnlyCollection<AgentInformation>>.InternalServerError(getAgentRepositoryResult.Error);

            var result = new List<AgentInformation>();

            foreach (var startAgentInformation in startAgentInformations)
            {
                var runAgentResult = await RunAgentAsync(startAgentInformation, getAgentRepositoryResult.Value);
                if (!runAgentResult.IsSuccessful)
                    return OperationResult<IReadOnlyCollection<AgentInformation>>.InternalServerError(runAgentResult.Error);
                result.Add(runAgentResult.Value);
            }

            return OperationResult<IReadOnlyCollection<AgentInformation>>.Ok(result);
        }

        private string GetTempWorkingDirectory()
        {
            var tempDirectory = Path.GetTempPath();
            var workingDirectoryName = Guid.NewGuid().ToString();

            var directoryInfo = Directory.CreateDirectory(Path.Combine(tempDirectory, workingDirectoryName));

            return directoryInfo.FullName;
        }

        private async Task<OperationResult<GitRepository>> GetAgentGitRepositoryAsync(string workingDirectory)
        {
            var agentUrl = configuration.AgentRepositoryUrl;

            var gitProjectInformation =
                new GitProjectInformation(agentUrl, "", workingDirectory, Guid.NewGuid().ToString());
            var repository = new GitProject(gitProjectInformation, loggerFactory);

            var cloneResult = await repository.CloneAsync();

            if (!cloneResult.IsSuccessful)
                return OperationResult<GitRepository>.InternalServerError($"Cannot clone agent repository. Error message: '{cloneResult.Error}'");

            return OperationResult<GitRepository>.Ok(cloneResult.Value);
        }

        private async Task<OperationResult<AgentInformation>> RunAgentAsync(StartAgentInformation startAgentInformation, GitRepository agentRepository)
        {
            var buildAgentResult = await BuildAgentAsync(agentRepository);
            if (!buildAgentResult.IsSuccessful)
                return OperationResult<AgentInformation>.InternalServerError(buildAgentResult.Error);

            var shellRunner = new ShellRunner(new ShellRunnerArgs(agentRepository.Directory,
                "docker", $"run -p {startAgentInformation.Port}:80 -d agent:latest"));
            var runAgentResult = await shellRunner.RunAsync();

            if (!runAgentResult.IsSuccessful)
                return OperationResult<AgentInformation>.InternalServerError($"Cannot run container with agent. Error: {runAgentResult.Error}");

            logger.LogInformation(await runAgentResult.Value.Output.ReadToEndAsync());

            return OperationResult<AgentInformation>.Ok(new AgentInformation(new Uri($"http://localhost:{startAgentInformation.Port}")));
        }

        private async Task<VoidOperationResult> BuildAgentAsync(GitRepository agentRepository)
        {
            var shellRunner = new ShellRunner(new ShellRunnerArgs(agentRepository.Directory,
                "docker", $"build -f {Path.Combine("Elevator.Agent", "Dockerfile")} -t agent ."));
            var buildAgentResult = await shellRunner.RunAsync();

            if (!buildAgentResult.IsSuccessful)
                return VoidOperationResult.InternalServerError($"Cannot build agent image. Error: '{buildAgentResult.Error}'");

            logger.LogInformation(await buildAgentResult.Value.Output.ReadToEndAsync());

            return VoidOperationResult.Ok();
        }
    }
}
