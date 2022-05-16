using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class ConnectionExtensions
    {
        public static X509Certificate2? GetCertificate(this EdgeDBConnection connection)
        {
            if (connection.TLSCertificateAuthority is null)
                return null;

            return new X509Certificate2(Encoding.ASCII.GetBytes(connection.TLSCertificateAuthority));
        }
    }
}
