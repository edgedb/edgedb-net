namespace EdgeDB.Utils;

internal static class HexConverter
{
    public static byte[] FromHex(string hex)
    {
        hex = hex.Replace("-", "");
        var raw = new byte[hex.Length / 2];
        for (var i = 0; i < raw.Length; i++)
        {
            raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return raw;
    }

    public static string ToHex(byte[] arr)
        => BitConverter.ToString(arr).Replace("-", "");
}
