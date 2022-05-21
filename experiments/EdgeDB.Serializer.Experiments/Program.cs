using EdgeDB;
using EdgeDB.Models;
using System.Text;


var e = BitConverter.IsLittleEndian;

unsafe
{
    var packet = Serializer.Deserialize();

    var aa = *packet;

    if (ServerMessageType.ServerKeyData == aa)
    {
        var p = *(DummyPacket*)packet;

        var str = p.GetContent();
    }
}

byte[] one = new byte[] { 0x05, 0x0, 0x0, 0x0, 0x48, 0x65, 0x6C, 0x6C, 0x6F };
byte[] two = new byte[] { 0x05, 0x0, 0x0, 0x0, 0x48, 0x65, 0x6C, 0x6C, 0x6F };

unsafe
{
    fixed (byte* onept = one)
    fixed (byte* twopt = two)
    fixed (byte** arr = new byte*[2])
    {
        arr[0] = onept;
        arr[1] = twopt;

        var t = GetStringArray(arr, 2);
    }
}




unsafe string[] GetStringArray(byte** array, int count)
{
    string[] arr = new string[count];

    for (int i = 0; i != arr.Length; i++)
    {
        var ptr = array[i];

        var length = (*(int*)ptr);
        var strRaw = ptr += 4;
        arr[i] = Encoding.UTF8.GetString(strRaw, length);
    }

    return arr;
}