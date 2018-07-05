using System;

namespace AksCheckNewVersion
{
    public static class Settings
    {
        public const string TenantId = "tenantId";
        public const string ApplicationId = "applicationId";
        public const string SubscriptionId = "subscriptionId";
        public const string ServicePrincipalPassword = "ServicePrincipalPassword";

        public const string TableStorageName = "TableStorageName";
        public const string TableStoragePartitionKey = "TableStoragePartitionKey";
        public const string TableStorageConnectionString = "TableStorageConnectionString";

        public static string GetSetting(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
