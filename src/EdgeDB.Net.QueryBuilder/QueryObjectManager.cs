using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a class that contains safe references to objects returned in queries.
    /// </summary>
    internal class QueryObjectManager
    {
        /// <summary>
        ///     A <see cref="HashSet{T}"/> containing the <see cref="QueryObjectReference"/>s.
        /// </summary>
        private static readonly HashSet<QueryObjectReference> _references = new();

        /// <summary>
        ///     Initializes the object manager, creating a hook to the <see cref="TypeBuilder"/>
        ///     to get any objects returned from queries.
        /// </summary>
        public static void Initialize()
        {
            TypeBuilder.OnObjectCreated += OnObjectCreated;
        }

        /// <summary>
        ///     Attempts to get the EdgeDB object id for the given instance.
        /// </summary>
        /// <param name="obj">The object instance to get the id from.</param>
        /// <param name="id">The out parameter containing the id of the provided object.</param>
        /// <returns>
        ///     <see langword="true"/> if the object instance matched one that was 
        ///     returned from a previous query, otherwise <see langword="false"/>.
        /// </returns>
        public static bool TryGetObjectId(object? obj, out Guid id)
        {
            id = default;
            if (obj == null)
                return false;

            var reference = _references.FirstOrDefault(x => x.Reference.IsAlive && x.Reference.Target == obj);
            id = reference?.ObjectId ?? default;
            return reference != null;
        }

        /// <summary>
        ///     The callback to add new references to <see cref="_references"/>.
        /// </summary>
        /// <param name="obj">The object returned from the query.</param>
        /// <param name="id">The id of the object.</param>
        private static void OnObjectCreated(object obj, Guid id)
        {
            var reference = new QueryObjectReference(id, new WeakReference(obj));
            _references.Add(reference);
        }

        /// <summary>
        ///     Represents a wrapped <see cref="WeakReference"/> containing the reference and the object id.
        /// </summary>
        private class QueryObjectReference
        {
            /// <summary>
            ///     The id of the object within the <see cref="Reference"/>.
            /// </summary>
            public readonly Guid ObjectId;

            /// <summary>
            ///     The weak reference to a returned query object.
            /// </summary>
            public readonly WeakReference Reference;

            /// <summary>
            ///     Constructs a new <see cref="QueryObjectReference"/>.
            /// </summary>
            /// <param name="objectId">The object id of the reference.</param>
            /// <param name="reference">The weak reference pointing to a returned query object.</param>
            public QueryObjectReference(Guid objectId, WeakReference reference)
            {
                ObjectId = objectId;
                Reference = reference;
            }
        }
    }
}
