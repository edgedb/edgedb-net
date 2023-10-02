using EdgeDB.Binary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace EdgeDB.Tests.Unit;

[TestClass]
public class PacketWriterTests
{
    private static readonly Dictionary<Type, Func<byte[], object>> _endiannessConverters = new()
    {
        {typeof(int), data => BinaryPrimitives.ReadInt32BigEndian(data)},
        {typeof(uint), data => BinaryPrimitives.ReadUInt32BigEndian(data)},
        {typeof(long), data => BinaryPrimitives.ReadInt64BigEndian(data)},
        {typeof(ulong), data => BinaryPrimitives.ReadUInt64BigEndian(data)},
        {typeof(float), data => BinaryPrimitives.ReadSingleBigEndian(data)},
        {typeof(double), data => BinaryPrimitives.ReadDoubleBigEndian(data)},
        {typeof(short), data => BinaryPrimitives.ReadInt16BigEndian(data)},
        {typeof(ushort), data => BinaryPrimitives.ReadUInt16BigEndian(data)},
        {typeof(byte), data => data[0]},
        {typeof(sbyte), data => (sbyte)data[0]}
    };

    private static unsafe void TestUnmanagedWrite<T>(T value)
        where T : unmanaged
    {
        var writer = new PacketWriter(sizeof(T));
        writer.Write(value);
        Assert.AreEqual(sizeof(T), writer.Index);

        // ensure endianness was correctly accounted for
        var data = writer.GetBytes().ToArray();
        Assert.AreEqual(value, _endiannessConverters[typeof(T)](data));
    }

    [TestMethod]
    public void TestDynamicSizing()
    {
        var writer = new PacketWriter(4, true);
        writer.Write((ulong)1234);
        Assert.AreEqual(16, writer.Size);
    }

    [TestMethod]
    public void TestByte()
        => TestUnmanagedWrite<byte>(0x01);

    [TestMethod]
    public void TestSByte()
        => TestUnmanagedWrite<sbyte>(-1);

    [TestMethod]
    public void TestInt16()
        => TestUnmanagedWrite((short)1234);

    [TestMethod]
    public void TestUInt16()
        => TestUnmanagedWrite((ushort)1234);

    [TestMethod]
    public void TestInt32()
        => TestUnmanagedWrite(1234);

    [TestMethod]
    public void TestUInt32()
        => TestUnmanagedWrite((uint)1234);

    [TestMethod]
    public void TestInt64()
        => TestUnmanagedWrite((long)1234);

    [TestMethod]
    public void TestUInt64()
        => TestUnmanagedWrite((ulong)1234);

    [TestMethod]
    public void TestFloat()
        => TestUnmanagedWrite((float)1234);

    [TestMethod]
    public void TestDouble()
        => TestUnmanagedWrite((double)1234);
}
