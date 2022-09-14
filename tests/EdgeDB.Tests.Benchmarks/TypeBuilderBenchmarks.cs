using BenchmarkDotNet.Attributes;
using EdgeDB.Binary.Packets;
using EdgeDB.Codecs;
using EdgeDB.Serializer;
using EdgeDB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class TypeBuilderBenchmarks
    {
        public class Person
        {
            public string? Name { get; set; }

            public string? Email { get; set; }
        }
        
        internal static Codecs.Object Codec;
        internal static Data Data;
        static TypeBuilderBenchmarks()
        {
            Data = new Data(new byte[] { 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x0F, 0x64, 0x65, 0x66, 0x61, 0x75, 0x6C, 0x74, 0x3A, 0x3A, 0x50, 0x65, 0x72, 0x73, 0x6F, 0x6E, 0x00, 0x00, 0x0B, 0x86, 0x00, 0x00, 0x00, 0x10, 0x4D, 0x3A, 0xEC, 0xD0, 0x0A, 0xA1, 0x11, 0xED, 0x87, 0x37, 0xB3, 0xDB, 0x16, 0x26, 0xE0, 0x22, 0x00, 0x00, 0x0B, 0x86, 0x00, 0x00, 0x00, 0x10, 0x97, 0x2B, 0xCF, 0x8A, 0x0A, 0xA8, 0x11, 0xED, 0x93, 0x76, 0xBB, 0xA4, 0xA8, 0xC6, 0xC3, 0xF0, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x0A, 0x4A, 0x6F, 0x68, 0x6E, 0x20, 0x53, 0x6D, 0x69, 0x74, 0x68, 0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x10, 0x6A, 0x6F, 0x68, 0x6E, 0x40, 0x65, 0x78, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D });

            Codec = new Codecs.Object(new ICodec[]
            {
                new Codecs.Text(),
                new Codecs.UUID(),
                new Codecs.UUID(),
                new Codecs.Text(),
                new Codecs.Text(),
            }, new string[]
            {
                "__tname__",
                "__tid__",
                "id",
                "name",
                "email"
            });
            Codec.Initialize(typeof(Person));
        }

        [Benchmark]
        public Person? DeserializePersonNew()
        {
            return (Person?)TypeBuilder.BuildObject(typeof(Person), Codec, ref Data);
        }

        //[Benchmark]
        //public List<int> GetMissedIndexes()
        //{
        //    var collection = new List<int>(Enumerable.Range(0, 10));

        //    collection.Remove(1);
        //    collection.Remove(3);
        //    collection.Remove(4);
        //    collection.Remove(7);
        //    collection.Remove(8);

        //    return collection;
        //}

        //[Benchmark]
        //public int[] GetMissedIndexesFast()
        //{
        //    var inverseTracker = new FastInverseIndexer(10);

        //    inverseTracker.Track(1);
        //    inverseTracker.Track(3);
        //    inverseTracker.Track(4);
        //    inverseTracker.Track(7);
        //    inverseTracker.Track(8);

        //    return inverseTracker.GetIndexies();
        //}

        //public Person? DeserializePersonNew()
        //{
            
        //}
    }
}
