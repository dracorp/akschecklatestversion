using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AksCheckNewVersion
{
    public static class AzureTableStorage
    {        
        public static async Task<IDictionary<string, Version>> GetStoredLatestVersions()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.GetSetting(Settings.TableStorageConnectionString));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Settings.GetSetting(Settings.TableStorageName));

            await table.CreateIfNotExistsAsync();
            TableQuery<AksLatestVersionEntity> query = new TableQuery<AksLatestVersionEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Settings.GetSetting(Settings.TableStoragePartitionKey)));

            TableContinuationToken continuationToken = null;
            TableQuerySegment<AksLatestVersionEntity> tableQueryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

            var locations = new Dictionary<string, Version>();

            foreach (AksLatestVersionEntity entity in tableQueryResult)
            {
                locations.Add(entity.RowKey, new Version(entity.LatestVersion));
            }
            return locations;
        }

        public static async Task AddOrUpdateLatestVersion(string location, string latestVersion)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.GetSetting(Settings.TableStorageConnectionString));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Settings.GetSetting(Settings.TableStorageName));

            TableOperation tableOperation = TableOperation.InsertOrReplace(new AksLatestVersionEntity(Settings.TableStoragePartitionKey, location, latestVersion));
            await table.ExecuteAsync(tableOperation);
        }
    }
}
