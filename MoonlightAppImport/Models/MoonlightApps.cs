using System.Collections.Generic;

namespace MoonlightAppImport.Models
{
    public class App
    {
        public string name { get; set; }
        public string uuid { get; set; }
    }

    public class MoonlightApps
    {
        public List<App> apps { get; set; }
    }
}
