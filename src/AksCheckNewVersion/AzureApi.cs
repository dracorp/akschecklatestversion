using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AksCheckNewVersion
{
    public static class AzureApi
    {
        private static HttpClient httpClient = new HttpClient();

        private const string AuthenticateURI = "https://login.microsoftonline.com/{0}/oauth2/token";
        private const string AuthenticateBody = "grant_type=client_credentials&client_id={0}&resource=https%3A%2F%2Fmanagement.core.windows.net%2F&client_secret={1}";

        public static async Task<string> GetAuthorizationToken()
        {
            string tenantId = Settings.GetSetting(Settings.TenantId);
            string applicationId = Settings.GetSetting(Settings.ApplicationId);
            string password = WebUtility.UrlEncode(Settings.GetSetting(Settings.ServicePrincipalPassword));

            string authenticateRequestUri = string.Format(AuthenticateURI, tenantId);
            string authenticateBody = string.Format(AuthenticateBody, applicationId, password);
            string authenticateContentType = "application/x-www-form-urlencoded";

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(authenticateContentType));

            using (HttpContent body = new StringContent(authenticateBody))
            {
                body.Headers.ContentType = new MediaTypeHeaderValue(authenticateContentType);

                using (HttpResponseMessage response = await httpClient.PostAsync(authenticateRequestUri, body))
                using (HttpContent content = response.Content)
                {
                    Task<string> result = content.ReadAsStringAsync();
                    JObject jsonBody = JObject.Parse(result.Result);
                    string token = jsonBody.GetValue("access_token").ToString();
                    return token;
                }
            }
        }

        public static async Task<string> GetAksLocations(string token, string subscriptionId)
        {
            var providersURI = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.ContainerService?api-version=2018-02-01";
            var json = await ExecuteGetOnAzureApi(providersURI, token);
            return json;
        }

        public static async Task<string> GetAksVersions(string token, string subscriptionId, string location)
        {
            var aksVersionsUri = $"https://management.azure.com/subscriptions/{subscriptionId}/providers/Microsoft.ContainerService/locations/{location}/orchestrators?api-version=2017-09-30&resource-type=managedClusters";
            var json = await ExecuteGetOnAzureApi(aksVersionsUri, token);
            return json;
        }

        private static async Task<string> ExecuteGetOnAzureApi(string uri, string token)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", token));

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            using (HttpContent content = response.Content)
            {
                return await content.ReadAsStringAsync();
            }
        }
    }
}
