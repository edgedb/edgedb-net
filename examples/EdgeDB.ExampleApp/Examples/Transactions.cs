﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    public class Transactions : IExample
    {
        public Logger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var str = await client.TransactionAsync(async (tx) =>
            {
                return await tx.QueryRequiredSingleAsync<string>("select \"Hello World!\"");
            });

            Logger!.Info(str!, LogPostfix.Examples);
        }
    }
}