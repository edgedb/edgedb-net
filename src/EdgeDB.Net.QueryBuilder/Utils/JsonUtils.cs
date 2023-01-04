using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a node within a depth map.
    /// </summary>
    internal readonly struct DepthNode
    {
        /// <summary>
        ///     The type of the node, this type represents the nodes value.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        ///     The value of the node.
        /// </summary>
        public readonly JObject JsonNode;

        /// <summary>
        ///     Gets the 0-based depth of the current node.
        /// </summary>
        public readonly int Depth;

        /// <summary>
        ///     Constructs a new <see cref="DepthNode"/>.
        /// </summary>
        /// <param name="type">The type of the node.</param>
        /// <param name="node">The node containing the value.</param>
        /// <param name="depth">The depth of the node.</param>
        public DepthNode(Type type, JObject node, int depth)
        {
            Type = type;
            JsonNode = node;
            Depth = depth;
        }
    }

    internal struct NodeCollection : IEnumerable<DepthNode>
    {
        private readonly Dictionary<int, int> _depthIndex;
        private readonly List<DepthNode> _nodes;

        public NodeCollection()
        {
            _nodes = new();
            _depthIndex = new();
        }

        public void Add(DepthNode node)
        {
            if (_depthIndex.ContainsKey(node.Depth))
                _depthIndex[node.Depth]++;
            else
                _depthIndex[node.Depth] = 0;

            _nodes.Add(node);
        }

        public void AddRange(IEnumerable<DepthNode> nodes)
            => _nodes.AddRange(nodes);

        public int GetNodeRelativeDepthIndex(DepthNode node)
            => _depthIndex[node.Depth];

        public int GetCurrentDepthIndex(int depth)
            => _depthIndex.TryGetValue(depth, out var v) ? v : 0;

        public IEnumerator<DepthNode> GetEnumerator()
            => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _nodes.GetEnumerator();

        public List<DepthNode> ToList()
            => _nodes;
    }

    internal class JsonUtils
    {
        /// <summary>
        ///     The regex used to resolve json paths.
        /// </summary>
        private static readonly Regex _pathResolverRegex = new(@"\[\d+?](?>\.(.*?)$|$)");

        public static List<DepthNode> BuildDepthMap(string mappingName, IJsonVariable jsonVariable)
        {
            var elements = jsonVariable.GetObjectsAtDepth(0);

            var nodes = new NodeCollection();

            foreach (var element in elements)
            {
                var type = ResolveTypeFromPath(jsonVariable.InnerType, element.Path);
                var node = new DepthNode(type, element, 0);
                nodes.Add(node);
                GetNodes(mappingName, node, jsonVariable, nodes);
            }

            return nodes.ToList();
        }

        private static void GetNodes(string mappingName, DepthNode node, IJsonVariable jsonValue, NodeCollection nodes)
        {
            var currentDepth = node.Depth;

            foreach (var prop in node.JsonNode.Properties())
            {
                if (prop.Value is JObject jObject)
                {
                    // if its a sub-object, add it to the next depth level
                    var mapIndex = currentDepth + 1;

                    // resolve the objects link type from its path
                    var type = ResolveTypeFromPath(jsonValue.InnerType, prop.Path);

                    var childNode = new DepthNode(type, jObject, mapIndex);
                    nodes.Add(childNode);

                    // get each sub node of the child
                    GetNodes(mappingName, childNode, jsonValue, nodes);

                    // mutate the node
                    node.JsonNode[prop.Name] = new JObject()
                    {
                        new JProperty($"{mappingName}_depth_index", nodes.GetNodeRelativeDepthIndex(childNode)),
                    };
                }
                else if (prop.Value is JArray jArray && jArray.All(x => x is JObject))
                {
                    // if its an array, add it to the next depth level
                    var mapIndex = currentDepth + 1;

                    // resolve the objects link type from its path
                    var type = ResolveTypeFromPath(jsonValue.InnerType, prop.Path);

                    var indx = nodes.GetCurrentDepthIndex(mapIndex);

                    foreach(var element in jArray)
                    {
                        var subNode = new DepthNode(type, (JObject)element, mapIndex);
                        nodes.Add(subNode);
                        GetNodes(mappingName, subNode, jsonValue, nodes);
                    }

                    // populate the mutable one with the location of the nested object
                    node.JsonNode[prop.Name] = new JObject()
                    {
                        new JProperty($"{mappingName}_depth_from", indx),
                        new JProperty($"{mappingName}_depth_to", indx + jArray.Count)
                    };
                }
            }
        }

        /// <summary>
        ///     Resolves the type of a property given the string json path.
        /// </summary>
        /// <param name="rootType">The root type of the json variable</param>
        /// <param name="path">The path used to resolve the type of the property.</param>
        /// <returns></returns>
        public static Type ResolveTypeFromPath(Type rootType, string path)
        {
            // match our path resolving regex
            var match = _pathResolverRegex.Match(path);

            // if the first group is empty, were dealing with a index
            // only. We can safely return the root type.
            if (string.IsNullOrEmpty(match.Groups[1].Value))
                return rootType;

            // split the main path up
            var pathSections = match.Groups[1].Value.Split('.');

            // iterate over it, pulling each member out and getting the member type.
            Type result = rootType;
            for (int i = 0; i != pathSections.Length; i++)
            {
                result = ResolveTypeFromPath(result, pathSections[i]);
                //result = result.GetMember(pathSections[i]).First(x => x is PropertyInfo or FieldInfo)!.GetMemberType();
            }    

            if (EdgeDBTypeUtils.IsLink(result, out var isMultiLink, out var innerType) && isMultiLink)
                return innerType!;

            // return the final type.
            return result;
        }
    }
}
