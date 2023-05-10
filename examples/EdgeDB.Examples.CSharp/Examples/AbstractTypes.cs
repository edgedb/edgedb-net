using EdgeDB.State;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class AbstractTypes : IExample
    {
        public ILogger? Logger { get; set; }

        // corresponding schema type:
        // abstract type AbstractThing {
        //   required property name -> str {
        //     constraint exclusive;
        //   }
        // }
        public abstract class AbstractThing
        {
            public string? Name { get; init; }
        }

        // corresponding schema type:
        // type Thing extending AbstractThing {
        //   required property description -> str;
        // }
        public class Thing : AbstractThing
        {
            public string? Description { get; init; }
        }

        // corresponding schema type:
        // type OtherThing extending AbstractThing {
        //   required property attribute -> str;
        // }
        public class OtherThing : AbstractThing
        {
            public string? Attribute { get; init; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var str = "11.996639232149645";

            var a = decimal.Parse(str);
            var b = double.Parse(str);

            // select the abstract type from the schema.
            // Note that the type builder will 'discover' the types that inherit
            // our C# abstract type.
            var result = await client.QueryAsync<AbstractThing>("select AbstractThing { name }");

            // select only 'Thing' types
            var things = result.Where(x => x is Thing);

            // select only 'OtherThing' types
            var otherThings = result.Where(x => x is OtherThing);

            // combining with the 'is' edgeql operator allows full polymorphism that
            // reflects back into C#, the returning collection contains both 'Thing' and
            // 'OtherThing' types we defined. I'd like to see an ORM try doing that :D
            var isResult = await client.QueryAsync<AbstractThing>("select AbstractThing { name, [is Thing].description, [is OtherThing].attribute }");
        }
    }
}
