using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AksCheckNewVersion.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Should_GetLatestVersionFromJson()
        {
            string json = File.ReadAllText("get-versions-result.json");

            dynamic dynJson = JsonConvert.DeserializeObject(json);

            Version latestVersion = AksCheckNewVersion.GetLatestVersion(json);

            Version expectedVersion = new Version("1.10.3");
            Assert.Equal(expectedVersion, latestVersion);
        }

        [Fact]
        public void Should_GetLocationsWhereK8sIsSupportedFromJson2()
        {
            string json = File.ReadAllText("get-provider-containerservice.json");            

            var locations = AksCheckNewVersion.GetLocationsWhichSupportAKS(json);

            Assert.Equal(12, locations.Count());
        }

        [Fact]
        public void Should_HaveNoChanges()
        {         
            Version latestStoredVersion = new Version("10.1.3");
            Version latestVersion = new Version("10.1.3");
            string supportedLocation = "West Europe";           

            var message = AksCheckNewVersion.GetAksUpdateForSingleLocation(latestStoredVersion, supportedLocation, latestVersion );

            Assert.Null(message);
        }

        [Fact]
        public void Should_IdentifyNewLocation()
        {            
            Version latestStoredVersion = new Version();
            Version latestVersion = new Version("10.1.3");
            string supportedLocation = "West Europe";

            var message = AksCheckNewVersion.GetAksUpdateForSingleLocation(latestStoredVersion, supportedLocation, latestVersion);

            Assert.Equal("New location West Europe available in Azure supporting AKS version 10.1.3", message);
        }

        [Fact]
        public void Should_IdentifyNewVersion()
        {            
            Version latestStoredVersion = new Version("10.1.1");
            Version latestVersion = new Version("10.1.3");
            string supportedLocation = "West Europe";

            var message = AksCheckNewVersion.GetAksUpdateForSingleLocation(latestStoredVersion, supportedLocation, latestVersion);

            Assert.Equal("Location West Europe in Azure has a new version of AKS available: 10.1.3", message);
        }
    }
}
