using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common;
using Elevator.Agent.Manager.Api.AgentCommunication.Models;
using Elevator.Agent.Manager.Api.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Elevator.Agent.Manager.Api.AgentCommunication
{
    public class AgentClient
    {
        private readonly Uri agentUri;
        private readonly HttpClient httpClient;
        private readonly ILogger<AgentClient> logger;

        public AgentClient(Uri agentUri, ILogger<AgentClient> logger)
        {
            this.agentUri = agentUri;
            this.logger = logger;
            httpClient = new HttpClient();
        }

        public async Task<OperationResult<StatusResponse>> GetAgentStatusAsync()
        {
            using var scope = logger.BeginScope($"[Agent {agentUri}]");
            logger.LogInformation("Sending request GET /status to agent");

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(agentUri, "/status"));
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var responseMessage = await httpClient.SendAsync(request);

            logger.LogInformation($"Received response with status code {responseMessage.StatusCode} from agent");

            if ((int)responseMessage.StatusCode < 200 || (int)responseMessage.StatusCode> 299)
                return OperationResult<StatusResponse>.Failed("Something gone wrong. Check agent's logs");
                
            var body = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return OperationResult<StatusResponse>.Failed("Agent did not return value");

           
            return JsonConvert.DeserializeObject<OperationResult<StatusResponse>>(body);
        }

        public async Task<OperationResult<BuildTaskResult>> FreeAgentAsync()
        {
            using var scope = logger.BeginScope($"[Agent {agentUri}]");
            logger.LogInformation("Sending request POST /status/free to agent");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(agentUri, "/status/free"));
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var responseMessage = await httpClient.SendAsync(request);

            logger.LogInformation($"Received response with status code {responseMessage.StatusCode} from agent");

            if ((int)responseMessage.StatusCode < 200 || (int)responseMessage.StatusCode > 299)
                return OperationResult<BuildTaskResult>.Failed("Something gone wrong. Check agent's logs");

            var body = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return OperationResult<BuildTaskResult>.Failed("Agent did not return value");

           
            return JsonConvert.DeserializeObject<OperationResult<BuildTaskResult>>(body);
        }

        public async Task<VoidOperationResult> PushTaskAsync(BuildTask buildTask)
        {
            using var scope = logger.BeginScope($"[Agent {agentUri}");

            logger.LogInformation("Sending request POST /task/push to agent");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(agentUri, "/task/push"));
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var bodyString = JsonConvert.SerializeObject(buildTask);
            request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.SendAsync(request);

            logger.LogInformation($"Received response with status code {responseMessage.StatusCode} from agent");

            if ((int)responseMessage.StatusCode < 200 || (int)responseMessage.StatusCode > 299)
                return VoidOperationResult.Failed("Something gone wrong. Check agent's logs");

            var body = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return VoidOperationResult.Failed("Agent did not return value");

           
            return JsonConvert.DeserializeObject<VoidOperationResult>(body);
        }

        public async Task<OperationResult<BuildTaskResult>> PullResultAsync()
        {
            using var scope = logger.BeginScope($"[Agent {agentUri}]");
            logger.LogInformation("Sending request GET /task/pull to agent");

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(agentUri, "/task/pull"));
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var responseMessage = await httpClient.SendAsync(request);

            logger.LogInformation($"Received response with status code {responseMessage.StatusCode} from agent");

            if ((int)responseMessage.StatusCode < 200 || (int)responseMessage.StatusCode > 299)
                return OperationResult<BuildTaskResult>.Failed("Something gone wrong. Check agent's logs");

            var body = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return OperationResult<BuildTaskResult>.Failed("Agent did not return value");

          
            return JsonConvert.DeserializeObject<OperationResult<BuildTaskResult>>(body);
        }
    }
}
