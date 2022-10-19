## Things to look at
- [Demo using the query builder](https://github.com/quinchs/EdgeDB.Net/blob/feat/querybuilder-v2/examples/EdgeDB.Examples.ExampleApp/Examples/QueryBuilder.cs)
- [QueryBuilder class](https://github.com/quinchs/EdgeDB.Net/blob/feat/querybuilder-v2/src/EdgeDB.Net.QueryBuilder/QueryBuilder.cs)
- [Query nodes (select, update, insert, etc...)](https://github.com/quinchs/EdgeDB.Net/tree/feat/querybuilder-v2/src/EdgeDB.Net.QueryBuilder/QueryNodes)
- [dotnet lambdas -> edgeql translators, ex: `string (string a) => a.ToLower()` -> `str_lower(a)`](https://github.com/quinchs/EdgeDB.Net/tree/feat/querybuilder-v2/src/EdgeDB.Net.QueryBuilder/Translators/Expressions)
- [EdgeDB standard library class](https://github.com/quinchs/EdgeDB.Net/blob/feat/querybuilder-v2/src/EdgeDB.Net.QueryBuilder/EdgeQL.g.cs)