using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace Assets.Core.Graph
{
    class RootGraph
    {
        private RootGraph() { }
        public IEnumerable<ScriptGraph> ScriptGraphs;
        public IEnumerable<ClipGraph> Clips;

        public static RootGraph Construct(TextReader docStream)
        {
            var yaml = new YamlStream();
            yaml.Load(docStream);

            var rootNode = (YamlMappingNode) yaml.Documents[0].RootNode;
            IEnumerable<ScriptGraph> scriptGraphs = new List<ScriptGraph>();
            IEnumerable<ClipGraph> clipGraphs = new List<ClipGraph>();

            foreach (var node in rootNode.Children)
            {
                if(((YamlScalarNode)node.Key).Value == "scriptgraphs")
                {
                    scriptGraphs = GetScriptGraphs((YamlSequenceNode)node.Value);
                }
                if (((YamlScalarNode)node.Key).Value == "clips")
                {
                    clipGraphs = GetClipGraphs((YamlSequenceNode)node.Value);
                }
            }

            var result = new RootGraph();
            result.ScriptGraphs = scriptGraphs;
            result.Clips = clipGraphs;
            return result;
        }

        public override string ToString()
        {
            new KeyValuePair<string, YamlNode> 
        }

        private static IEnumerable<ScriptGraph> GetScriptGraphs(YamlSequenceNode nodes)
        {
            List<ScriptGraph> graphs = new List<ScriptGraph>();
            foreach(var node in nodes)
            {
                StringReader reader = new StringReader(node.ToString());
                graphs.Add(ScriptGraph.Construct(reader));
            }
            return graphs;
        }

        private static IEnumerable<ClipGraph> GetClipGraphs(YamlSequenceNode nodes)
        {
            List<ClipGraph> graphs = new List<ClipGraph>();
            foreach(var node in nodes)
            {
                StringReader reader = new StringReader(node.ToString());
                graphs.Add(ClipGraph.Construct(reader));
            }
            return graphs;
        }
    }
}
