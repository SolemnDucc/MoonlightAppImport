using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MoonlightAppImport.Models
{
    public class App
    {
        [JsonProperty("allow-client-commands")]
        public bool allowclientcommands { get; set; }

        [JsonProperty("image-path")]
        public string imagepath { get; set; }
        public string name { get; set; }
        public string uuid { get; set; }

        [JsonProperty("auto-detach")]
        public bool? autodetach { get; set; }
        public string cmd { get; set; }

        [JsonProperty("prep-cmd")]
        public List<PrepCmd> prepcmd { get; set; }

        [JsonProperty("wait-all")]
        public bool? waitall { get; set; }
    }

    public class Env
    {
    }

    public class PrepCmd
    {
        public string @do { get; set; }
        public bool elevated { get; set; }
        public string undo { get; set; }
    }

    public class MoonlightApps
    {
        public List<App> apps { get; set; }
        public string current_app { get; set; }
        public Env env { get; set; }
        public int version { get; set; }
    }
}
