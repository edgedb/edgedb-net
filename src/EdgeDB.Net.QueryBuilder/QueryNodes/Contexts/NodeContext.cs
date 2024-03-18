using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a <see cref="QueryNode"/>'s context.
    /// </summary>
    internal abstract class NodeContext
    {
        /// <summary>
        ///     Gets or sets whether or not the node should be set as a global sub query.
        /// </summary>
        public bool SetAsGlobal { get; set; }

        /// <summary>
        ///     Gets the name of the global variable the node should be set to 
        ///     if <see cref="SetAsGlobal"/> is true.
        /// </summary>
        public string? GlobalName { get; init; }

        /// <summary>
        ///     Gets the current type the node is building for.
        /// </summary>
        public Type CurrentType { get; init; }

        /// <summary>
        ///     Gets whether or not the current type is a json variable.
        /// </summary>
        public bool IsJsonVariable
            => ReflectionUtils.IsSubclassOfRawGeneric(typeof(JsonCollectionVariable<>), CurrentType);

        /// <summary>
        ///     Gets a collection of child queries.
        /// </summary>
        internal Dictionary<string, SubQuery> ChildQueries { get; } = new();

        /// <summary>
        ///     Constructs a new <see cref="NodeContext"/>.
        /// </summary>
        /// <param name="currentType">The type that the node is building for.</param>
        public NodeContext(Type currentType)
        {
            CurrentType = currentType;
        }
    }
}
