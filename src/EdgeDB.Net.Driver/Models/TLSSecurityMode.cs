using Newtonsoft.Json;

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

internal class TLSSecurityModeParser : JsonConverter<TLSSecurityMode?>
{
    internal static bool TryParse(string text, bool parseEmptyAsNull, out TLSSecurityMode? tlsSecurity)
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
                return parseEmptyAsNull;
        }
        tlsSecurity = null;
        return false;
    }

    public static TLSSecurityMode Parse(string text, bool emptyAsDefault = false)
    {
        if (TryParse(text, emptyAsDefault, out TLSSecurityMode? tlsSecurity))
        {
            return tlsSecurity ?? TLSSecurityMode.Default;
        }
        else
        {
            throw new ConfigurationException(
                $"Invalid TLS Security: \"{text}\", "
                + "must be one of \"insecure\", \"no_host_verification\", \"strict\", or \"default\"");
        }
    }

    // Json conversion
    public override TLSSecurityMode? ReadJson(
        JsonReader reader,
        Type objectType,
        TLSSecurityMode? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
         if (reader.TokenType == JsonToken.String)
        {
            return Parse((string)reader.Value!, true);
        }
        else
        {
            throw new JsonException("Expected String.");
        }
    }

    public override void WriteJson(
        JsonWriter writer, TLSSecurityMode? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
