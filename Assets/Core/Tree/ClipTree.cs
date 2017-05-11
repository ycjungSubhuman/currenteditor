using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Assets.Core.Tree
{
    public class ClipTree
    {
        public string Midi { get; set; }
        public string Audio { get; set; }

        public static ClipTree Construct(TextReader docStream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var root = deserializer.Deserialize<ClipTree>(docStream);
            return root;
        }
    }
}
