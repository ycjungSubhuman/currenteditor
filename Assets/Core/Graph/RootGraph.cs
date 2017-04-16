using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Assets.Core.Graph
{
    class RootGraph
    {
        public List<Paths> Scriptgraphs { get; set; }
        public List<Paths> Clips { get; set; }

        public static RootGraph Construct(TextReader docStream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var root = deserializer.Deserialize<RootGraph>(docStream);
            return root;
        }

        public class Paths
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }
    }
}
