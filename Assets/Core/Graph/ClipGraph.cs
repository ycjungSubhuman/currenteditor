using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Assets.Core.Graph
{
    class ClipGraph
    {
        public string Midi { get; set; }
        public string Audio { get; set; }

        public static ClipGraph Construct(TextReader docStream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var root = deserializer.Deserialize<ClipGraph>(docStream);
            return root;
        }
    }
}
