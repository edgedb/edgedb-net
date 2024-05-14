using System.Linq.Expressions;

namespace EdgeDB;

internal sealed record class BinaryOpMetadata(params ExpressionType[]? Types) : IMarkerMetadata;
