using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;

namespace AksCheckNewVersion
{
    public static class AksCheckNewVersion
    {
        [FunctionName("AksCheckNewVersion")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                string subscriptionId = Settings.GetSetting(Settings.SubscriptionId);
                if (string.IsNullOrWhiteSpace(subscriptionId))
                    throw new Exception("No subscriptionId found");

                string token = await AzureApi.GetAuthorizationToken();

                var storedLatestVersionPerLocation = await AzureTableStorage.GetStoredLatestVersions();

                var supportedlocations = GetLocationsWhichSupportAKS(await AzureApi.GetAksLocations(token, subscriptionId));

                var messages = await GetAksUpdates(supportedlocations, token, subscriptionId, storedLatestVersionPerLocation);

                string jsonToReturn = JsonConvert.SerializeObject(messages);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(jsonToReturn, Encoding.UTF8, "application/json")
                };
            }
            catch (Exception ex)
            {
                log.Error("Exception occured: " + ex.ToString());
                return null;
            }
        }

        public static async Task<IList<string>> GetAksUpdates(IEnumerable<string> supportedLocations, string token, string subscriptionId, IDictionary<string, Version> storedLatestVersionPerLocation)
        {
            var messages = new List<string>();

            foreach (var supportedLocation in supportedLocations)
            {
                var location = supportedLocation.Replace(" ", "").ToLower();
                Version storedLatestVersion = new Version();

                if (storedLatestVersionPerLocation.ContainsKey(location))
                    storedLatestVersion = storedLatestVersionPerLocation[location];

                Version latestVersion = GetLatestVersion(await AzureApi.GetAksVersions(token, subscriptionId, location));

                string message = GetAksUpdateForSingleLocation(storedLatestVersion, supportedLocation, latestVersion);
                if (message != null)
                {
                    await AzureTableStorage.AddOrUpdateLatestVersion(location, latestVersion.ToString());
                    messages.Add(message);
                }
            }

            return messages;
        }

        public static string GetAksUpdateForSingleLocation(Version storedLatestVersion, string supportedLocation, Version latestVersion)
        {
            if (storedLatestVersion == new Version()) //First time
            {
                return $"New location {supportedLocation} available in Azure supporting AKS version {latestVersion}";
            }
            else if (latestVersion > storedLatestVersion)
            {
                return $"Location {supportedLocation} in Azure has a new version of AKS available: {latestVersion}";
            }
            
            return null;
        }

        public static IEnumerable<string> GetLocationsWhichSupportAKS(string jsonForContainerService)
        {
            JObject providerJson = JObject.Parse(jsonForContainerService);
            return providerJson.SelectToken("$..resourceTypes[?(@.resourceType=='managedClusters')].locations").Children().Select(l => l.Value<string>());
        }

        public static Version GetLatestVersion(string json)
        {
            dynamic dynJson = JsonConvert.DeserializeObject(json);

            List<Version> versions = new List<Version>();

            foreach (var item in dynJson.properties.orchestrators)
            {
                string orchestratorVersion = item.orchestratorVersion.ToString();
                versions.Add(new Version(orchestratorVersion));
            }

            return versions.Max();
        }
    }
}
