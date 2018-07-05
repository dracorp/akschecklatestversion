using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace AksCheckNewVersion
{
    public class AksLatestVersionEntity : TableEntity
    {
        public AksLatestVersionEntity(string partitionKey, string rowKey, string latestVersion)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
            this.LatestVersion = latestVersion;
        }

        public AksLatestVersionEntity() { }

        public string LatestVersion { get; set; }
    }
}
