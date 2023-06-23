using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Generator
{
    public static class Generator
    {
        [FunctionName("Generator")]
        [ServiceBusAccount("Connection")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [ServiceBus("%TopicName%", ServiceBusEntityType.Topic)]
            ServiceBusSender topicSender,
            ILogger log,
            CancellationToken token)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            var sbMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(name));

            sbMessage.ApplicationProperties.Add("demoproperty", "foobar");

            try {
                topicSender.SendMessageAsync(sbMessage, token);
            }
            catch(Exception e)
            {
                log.LogError(e, "got exception");
            }

            return new OkObjectResult(responseMessage);
        }
    }
}
