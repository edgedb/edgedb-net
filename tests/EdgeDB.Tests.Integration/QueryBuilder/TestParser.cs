using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeDB.Tests.Integration.QueryBuilder;

public class TestParser
{
    public string Content { get; private set; }
    public IReadOnlyDictionary<string, object?>? Variables { get; private set; }

    public TestParser(CompiledQuery compiled)
    {
        Content = compiled.Query;
        Variables = compiled.Variables;
    }

    public void EOF()
    {
        Assert.IsTrue(string.IsNullOrEmpty(Content));
    }

    public TestParser Expect(string s)
    {
        Assert.IsTrue(s.Length <= Content.Length, "Expected content exceeds the length of the remaining content");

        var part = Content[..s.Length];

        Assert.AreEqual(s, part);

        Content = Content[s.Length..];
        return this;
    }

    public TestParser Identifier(Action<string> func)
    {
        var ident = new string(Content.TakeWhile(x => x is not ' ' and not ',').ToArray());
        func(ident);
        Content = Content[ident.Length..];

        return this;
    }

    public TestParser Variable()
    {
        Assert.IsNotNull(Variables);
        return Identifier(ident => Assert.IsTrue(Variables.ContainsKey(ident)));
    }
}
