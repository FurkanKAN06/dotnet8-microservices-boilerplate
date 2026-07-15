using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace BuildingBlocks.Application.Behaviors
{
    public class KvkkCrudLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<KvkkCrudLoggingBehavior<TRequest, TResponse>> _logger;

        private static readonly HashSet<string> SensitiveFields = new HashSet<string>
        {
            "IdentityNumber",
            "PhoneNumber",
            "identityNumber",
            "phoneNumber"
        };

        public KvkkCrudLoggingBehavior(ILogger<KvkkCrudLoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            string requestName = typeof(TRequest).Name;

            if (requestName.StartsWith("Create") || requestName.StartsWith("Update") || requestName.StartsWith("Delete"))
            {
                string serializedRequest = JsonSerializer.Serialize<TRequest>(request);
                string maskedData = MaskPersonalDataValues(serializedRequest);

                _logger.LogInformation(
                    "KVKK CRUD Audit: Operation={Operation}, LegalBasis=LegitimateInterest, Data={MaskedData}",
                    requestName,
                    maskedData);
            }

            return await next();
        }

        private static string MaskPersonalDataValues(string jsonData)
        {
            try
            {
                JsonNode? node = JsonNode.Parse(jsonData);
                if (node is JsonObject jsonObject)
                {
                    MaskJsonObject(jsonObject);
                    return jsonObject.ToJsonString();
                }
            }
            catch
            {
                return "***MASKED_DUE_TO_PARSE_ERROR***";
            }
            return jsonData;
        }

        private static void MaskJsonObject(JsonObject jsonObject)
        {
            List<string> keysToMask = new List<string>();

            foreach (KeyValuePair<string, JsonNode?> property in jsonObject)
            {
                if (SensitiveFields.Contains(property.Key))
                {
                    keysToMask.Add(property.Key);
                }
                else if (property.Value is JsonObject nestedObject)
                {
                    MaskJsonObject(nestedObject);
                }
            }

            foreach (string key in keysToMask)
            {
                jsonObject[key] = "***MASKED***";
            }
        }
    }
}
