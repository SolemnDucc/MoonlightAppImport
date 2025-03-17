using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonlightAppImport;
using MoonlightAppImport.Http;
using MoonlightAppImport.Models;

namespace UnitTests
{
    [TestClass]
    public class HttpClientTests
    {
        [TestMethod]
        public async Task GetGamesFromApollo()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsApollo = true,
                SkipCertificateValidation = false,
                MoonlightPath = @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient sunshineHttpClient = new ApolloHttpClient(_settings);
            MoonlightApps apps = await sunshineHttpClient.GetGamesAsync();
            Assert.IsNotNull(apps);
        }

        [TestMethod]
        public async Task GetGamesFromSunshine()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsApollo = false,
                SkipCertificateValidation = true,
                MoonlightPath = @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient sunshineHttpClient = new SunshineHttpClient(_settings);
            MoonlightApps apps = await sunshineHttpClient.GetGamesAsync();
            Assert.IsNotNull(apps);
        }

        [TestMethod]
        public async Task GetServerInfo()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsApollo = false,
                SkipCertificateValidation = true,
                MoonlightPath = @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient sunshineHttpClient = new ApolloHttpClient(_settings);
            string hostname = await sunshineHttpClient.GetServerHostnameAsync();
            Assert.IsNotNull(hostname);
        }

        [TestMethod]
        public async Task GetOnlineStatus()
        {
            MoonlightAppImportSettings _settings = new MoonlightAppImportSettings()
            {
                IsApollo = false,
                SkipCertificateValidation = true,
                MoonlightPath = @"C:\Program Files\Moonlight Game Streaming\Moonlight.exe",
                SunshineHost = "localhost",
                SunshineUsername = "Foo",
                SunshinePassword = "Bar",
            };

            IHttpClient sunshineHttpClient = new ApolloHttpClient(_settings);
            bool b = await sunshineHttpClient.IsServerOnlineAsync();
            Assert.IsTrue(b);
        }
    }
}
