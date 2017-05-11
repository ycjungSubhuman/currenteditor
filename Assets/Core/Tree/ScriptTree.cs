using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.RepresentationModel;
using Assets.Timeline;
using Assets.Timeline.SubWindows;

namespace Assets.Core.Tree
{
    public class ScriptTree
    {
        public List<Node> Nodes { get; set; }

        public static ScriptTree Construct(TextReader docStream)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var root = deserializer.Deserialize<ScriptTree>(docStream);
            return root;
        }

        public static ScriptTree Construct(ScriptGraph graph)
        {
            ScriptTree result = new ScriptTree();
            result.Nodes = ConstructNodes(graph.nodes, graph.connections);
            return result;
        }

        private static List<Node> ConstructNodes(List<NodeWindow> nodeWindows, List<Connection> connections)
        {
            return nodeWindows.Select(node =>
            {
                Node result = new Node();
                result.Base = node.baseClassName;
                result.Name = node.nodeName;
                if (result.Base == "") // Group node
                {
                    Group g = node as Group;
                    result.Members = ConstructNodes(g.nodes, g.InterConnections);
                    result.StartMember = (from c in g.connections
                                          where c.outPoint == g.startPoint
                                          select c.inPoint.masterNode.nodeName)
                                          .SingleOrDefault();
                    result.EndMember = (from c in g.connections
                                        where c.inPoint == g.endPoint
                                        select c.outPoint.masterNode.nodeName)
                                          .SingleOrDefault();
                }
                else
                {
                    result.Params = new Dictionary<string, string>();
                    foreach (var p in node.paramPairs)
                    {
                        result.Params.Add(p.Key, p.Value);
                    }
                }
                result.Position = new Position(node.rect.position.x, node.rect.position.y);
                result.Succ = new List<Succ>();
                var succConns = from c in connections
                            where c.outPoint.masterNode == node
                            select c;
                foreach (var c in succConns)
                {
                    Succ succ = new Succ();
                    succ.Condition = c.conditions.Select(n => n.nodeName).ToList();
                    succ.EndPrev = c.stopPrev;
                    succ.Dest = c.inPoint.masterNode.nodeName;
                    result.Succ.Add(succ);
                }
                return result;
            }).ToList();
        }

        public class Node
        {
            public string Name { get; set; }
            public string Base { get; set; }
            public Dictionary<string, string> Params { get; set; }
            public Position Position { get; set; }
            public List<Succ> Succ { get; set; }
            public List<Node> Members { get; set; }
            public string StartMember { get; set; }
            public string EndMember { get; set; }
        }

        public class Position
        {
            public Position() { }
            public Position(double x, double y)
            {
                X = x;
                Y = y;
            }
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Succ
        {
            public string Dest { get; set; }
            public bool EndPrev { get; set; }
            public List<string> Condition { get; set; }
        }
    }
}
