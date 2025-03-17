using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoonlightAppImport.Models
{
    public class IPValidator
    {
        public static bool ValidateAndResolve(string input)
        {
            // Try to parse as an IPv4 address
            if (IPAddress.TryParse(input, out IPAddress address) && address.AddressFamily == AddressFamily.InterNetwork)
            {
                return true;
            }

            try
            {
                // Attempt to resolve as a domain name or localhost
                IPHostEntry hostEntry = Dns.GetHostEntry(input);
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
