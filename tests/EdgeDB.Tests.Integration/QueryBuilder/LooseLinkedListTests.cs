using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

namespace EdgeDB.Tests.Integration.QueryBuilder;

[TestClass]
public class LooseLinkedListTests
{
    [TestMethod]
    public void TestAdds()
    {
        var list = new LooseLinkedList<Value>();

        ref var abc = ref list.AddLast(new Value("ABC"));
        ref var def = ref list.AddLast(new Value("DEF"));
        ref var ghi = ref list.AddLast("GHI");

        Assert.AreEqual(3, list.Count);

        Assert.AreEqual("ABC", list.First.Value);
        Assert.AreEqual("DEF", list.First.Next.Value);
        Assert.AreEqual("GHI", list.Last.Value);

        Assert.IsTrue(abc.Next.ReferenceEquals(ref def));
        Assert.IsTrue(def.Next.ReferenceEquals(ref ghi));
        Assert.IsTrue(Unsafe.IsNullRef(ref abc.Previous));
        Assert.IsTrue(Unsafe.IsNullRef(ref ghi.Next));

        Assert.IsTrue(ghi.Previous.ReferenceEquals(ref def));
        Assert.IsTrue(def.Previous.ReferenceEquals(ref abc));

        ref var jkl = ref list.AddAfter(ref abc, "JKL");

        Assert.IsTrue(abc.Next.ReferenceEquals(ref jkl));
        Assert.IsTrue(jkl.Previous.ReferenceEquals(ref abc));
        Assert.IsTrue(jkl.Next.ReferenceEquals(ref def));
        Assert.IsTrue(def.Previous.ReferenceEquals(ref jkl));

        ref var mno = ref list.AddBefore(ref ghi, "MNO");
        Assert.IsTrue(def.Next.ReferenceEquals(ref mno));
        Assert.IsTrue(mno.Previous.ReferenceEquals(ref def));
        Assert.IsTrue(mno.Next.ReferenceEquals(ref ghi));
        Assert.IsTrue(ghi.Previous.ReferenceEquals(ref mno));


    }

    [TestMethod]
    public void TestDeletes()
    {
        var list = new LooseLinkedList<Value>();

        ref var abc = ref list.AddLast("ABC");
        ref var def = ref list.AddLast("DEF");
        ref var ghi = ref list.AddLast("GHI");
        ref var jkl = ref list.AddLast("JKL");
        ref var mno = ref list.AddLast("MNO");
        ref var pqr = ref list.AddLast("PQR");

        list.Remove(ref ghi, 2);

        Assert.AreEqual(4, list.Count);
        Assert.IsTrue(def.Next.ReferenceEquals(ref mno));
        Assert.IsTrue(mno.Previous.ReferenceEquals(ref def));

    }
}
