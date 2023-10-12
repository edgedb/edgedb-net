using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EdgeDB;

internal static class ConnectionExtensions
{
    public static X509Certificate2? GetCertificate(this EdgeDBConnection connection)
    {
        if (connection.TLSCertificateAuthority is null)
            return null;

        return new X509Certificate2(Encoding.ASCII.GetBytes(connection.TLSCertificateAuthority));
    }
}
