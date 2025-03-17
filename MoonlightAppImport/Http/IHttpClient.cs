using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonlightAppImport.Models;

namespace MoonlightAppImport.Http
{
    public interface IHttpClient : IDisposable
    {
        Task<MoonlightApps> GetGamesAsync();
        Task<string> GetServerHostnameAsync();
        Task<bool> IsServerOnlineAsync();
    }
}
