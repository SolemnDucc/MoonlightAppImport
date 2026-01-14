using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonlightAppImport;
using MoonlightAppImport.Http;
using MoonlightAppImport.Models;

namespace UnitTests
{
    [TestClass]
    public class ApolloHttpClientTests
    {
        [TestMethod]
        public async Task GetGamesFromApollo()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsEnabled = true,
                AddMetadata = true,
                PingHost = true,
                ServerType = ServerType.Apollo,
                SkipCertificateValidation = true,
                MoonlightPath = "Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient client = new ApolloHttpClient(_settings);
            MoonlightApps apps = await client.GetGamesAsync();
            Assert.IsNotNull(apps);
        }

        [TestMethod]
        public async Task GetServerInfo()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsEnabled = true,
                AddMetadata = true,
                PingHost = true,
                ServerType = ServerType.Apollo,
                SkipCertificateValidation = true,
                MoonlightPath = "Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient client = new ApolloHttpClient(_settings);
            string hostname = await client.GetServerHostnameAsync();
            Assert.IsNotNull(hostname);
        }

        [TestMethod]
        public async Task GetOnlineStatus()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsEnabled = true,
                AddMetadata = true,
                PingHost = true,
                ServerType = ServerType.Apollo,
                SkipCertificateValidation = true,
                MoonlightPath = "Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient client = new ApolloHttpClient(_settings);
            bool b = await client.IsServerOnlineAsync();
            Assert.IsTrue(b);
        }
    }
}
