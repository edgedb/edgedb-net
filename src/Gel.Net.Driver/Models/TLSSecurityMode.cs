
namespace Gel;

/// <summary>
///     Represents the TLS security mode the client will follow.
/// </summary>
public enum TLSSecurityMode
{
    /// <summary>
    ///     Certificates and hostnames will be verified.
    /// </summary>
    /// <remarks>
    ///     This is the default behavior
    /// </remarks>
    Strict,

    /// <summary>
    ///     Verify certificates but not hostnames.
    /// </summary>
    NoHostnameVerification,

    /// <summary>
    ///     Client libraries will trust self-signed TLS certificates. useful for self-signed or custom certificates.
    /// </summary>
    Insecure,

    /// <summary>
    ///     The default value, equivalent to <see cref="Strict" />
    /// </summary>
    Default = Strict
}

internal class TLSSecurityModeParser
{
    internal static bool TryParse(string text, bool emptyAsDefault, out TLSSecurityMode? tlsSecurity)
    {
        // Capitalized text does not conform to other libraries,
        // but is supported for backwards compatibility.
        switch (text)
        {
            case "Strict" or "strict":
                tlsSecurity = TLSSecurityMode.Strict;
                return true;
            case "NoHostnameVerification" or "no_host_verification":
                tlsSecurity = TLSSecurityMode.NoHostnameVerification;
                return true;
            case "Insecure" or "insecure" or "insecure_dev_mode":
                tlsSecurity = TLSSecurityMode.Insecure;
                return true;
            case "Default" or "default":
                tlsSecurity = null;
                return true;
            case "":
                tlsSecurity = null;
                return emptyAsDefault;
        }
        tlsSecurity = null;
        return false;
    }

}
