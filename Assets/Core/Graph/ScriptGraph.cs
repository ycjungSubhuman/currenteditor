using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;

namespace Assets.Core.Graph
{
    class ScriptGraph
    {
        public List<Node> Nodes { get; set; }

        public static ScriptGraph Construct(TextReader docStream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var root = deserializer.Deserialize<ScriptGraph>(docStream);
            return root;
        }

        public class Node
        {
            public string Name { get; set; }
            public string Base { get; set; }
            public Dictionary<string, string> Params { get; set; }
            public Position Position { get; set; }
            public List<Succ> Succ { get; set; }
        }

        public class Position
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Succ
        {
            public string Dest { get; set; }
            public string Type { get; set; }
            public bool EndPrev { get; set; }
            public string Condition { get; set; }
            public string DataLink { get; set; }
        }
    }
}
