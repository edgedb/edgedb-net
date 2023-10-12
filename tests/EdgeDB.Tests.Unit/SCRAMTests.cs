using EdgeDB.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EdgeDB.Tests.Unit;

[TestClass]
public class SCRAMTests
{
    public const string SCRAM_METHOD = "SCRAM-SHA-256";
    public const string SCRAM_USERNAME = "user";
    public const string SCRAM_PASSWORD = "pencil";
    public const string SCRAM_CLIENT_NONCE = "rOprNGfwEbeRWgbNEkqO";
    public const string SCRAM_SERVER_NONCE = "rOprNGfwEbeRWgbNEkqO%hvYDpWUa2RaTCAfuxFIlj)hNlF$k0";
    public const string SCRAM_SALT = "W22ZaJ0SNY7soEsUEjb6gQ==";

    private static readonly EdgeDBBinaryClient _client =
        new EdgeDBTcpClient(new EdgeDBConnection(), new EdgeDBConfig(), null!);

    [TestMethod]
    public void TestSCRAM()
    {
        var scram = new Scram(Convert.FromBase64String(SCRAM_CLIENT_NONCE));

        var clientFirst = scram.BuildInitialMessage(SCRAM_USERNAME);
        Assert.AreEqual($"n,,n={SCRAM_USERNAME},r={SCRAM_CLIENT_NONCE}", clientFirst);

        var serverFirst = CreateServerFirstMessage();
        var clientFinal = scram.BuildFinalMessage(serverFirst, SCRAM_PASSWORD);

        Assert.AreEqual($"c=biws,r={SCRAM_SERVER_NONCE},p=dHzbZapWIk4jUhN+Ute9ytag9zjfMHgsqmmiz7AndVQ=",
            clientFinal.final);
    }

    private static string CreateServerFirstMessage() => $"r={SCRAM_SERVER_NONCE},s={SCRAM_SALT},i=4096";
}
