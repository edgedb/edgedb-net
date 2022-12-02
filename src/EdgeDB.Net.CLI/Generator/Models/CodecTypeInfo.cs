using EdgeDB.CLI.Generator.Results;
using System;
namespace EdgeDB.CLI.Generator.Models
{
    public enum CodecType
    {
        Array,
        Set,
        Object,
        Tuple,
        Scalar
    }

    /// <summary>
    ///     Represents an expanded form of a <see cref="ICodec"/>, containing
    ///     ease-to-parse information about a codec.
    /// </summary>
    internal class CodecTypeInfo
    {
        /// <summary>
        ///     Gets the type that this codec info represents.
        /// </summary>
        public CodecType Type { get; set; }

        /// <summary>
        ///     Gets or sets the optional name of the codec.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the optional dotnet type name that represents what the
        ///     codec serializes/deserializes.
        /// </summary>
        public string? TypeName { get; set; }

        /// <summary>
        ///     Gets or sets the namespace of the given type info.
        /// </summary>
        /// <remarks>
        ///     This property is always <see langword="null"/> when the type info is not
        ///     representing a scalar type.
        /// </remarks>
        public string? Namespace { get; set; }

        /// <summary>
        ///     Gets or sets the child <see cref="CodecTypeInfo"/>s for this parent <see cref="CodecTypeInfo"/>.
        /// </summary>
        public IEnumerable<CodecTypeInfo>? Children { get; set; }

        /// <summary>
        ///     Gets the paret <see cref="CodecTypeInfo"/>.
        /// </summary>
        public CodecTypeInfo? Parent { get; set; }

        /// <summary>
        ///     Checks whether or not the current <see cref="CodecTypeInfo"/>'s body is equal to
        ///     the given <see cref="CodecTypeInfo"/>.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="CodecTypeInfo"/> to check against.
        /// </param>
        /// <returns>
        ///     <see langword="true"/> if the <see cref="CodecTypeInfo"/>'s body matches the
        ///     current <see cref="CodecTypeInfo"/>; otherwise <see langword="false"/>.
        /// </returns>
        public bool BodyEquals(CodecTypeInfo info)
        {
            return Type == info.Type &&
                   (info.Children?.SequenceEqual(Children ?? Array.Empty<CodecTypeInfo>()) ?? false);
        }

        /// <summary>
        ///     Gets a unique name for the current <see cref="CodecTypeInfo"/>.
        /// </summary>
        /// <returns>
        ///     A unique name representing the current <see cref="CodecTypeInfo"/>.
        /// </returns>
        public string GetUniqueTypeName()
        {
            List<string?> path = new() { TypeName };
            var p = Parent;
            while (p is not null)
            {
                path.Add(p.TypeName);
                p = p.Parent;
            }
            path.Reverse();
            return string.Join("", path.Where(x => x is not null));
        }

        public IQueryResult Build()
        {
            return Type switch
            {
                CodecType.Object => new ClassResult(this),
                _ => new ScalarResult(this)
            };
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} ({TypeName})";
        }
    }
}

