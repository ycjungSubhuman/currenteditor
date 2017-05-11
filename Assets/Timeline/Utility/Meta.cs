using Assets.Core.Tree;
using Assets.Midi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Timeline.Utility
{
    public class Meta
    {
        public static Dictionary<string, ClipTree> ClipTrees
        {
            get
            {
                Dictionary<string, ClipTree> clipGraphs = new Dictionary<string, ClipTree>();
                var rootText = Resources.Load<TextAsset>("meta/root");
                var docStream = new StringReader(rootText.text);
                RootGraph root = RootGraph.Construct(docStream);
                foreach (var clip in root.Clips)
                {
                    //load clip
                    var clipText = Resources.Load<TextAsset>("meta/" + clip.Path);
                    var clipDocStream = new StringReader(clipText.text);
                    ClipTree clipGraph = ClipTree.Construct(clipDocStream);

                    clipGraphs.Add(clip.Name, clipGraph);
                }
                return clipGraphs;
            }
        }
        public static Dictionary<string, ScriptTree> ScriptTrees
        {
            get
            {
                Dictionary<string, ScriptTree> scriptTrees = new Dictionary<string, ScriptTree>();
                var rootText = Resources.Load<TextAsset>("meta/root");
                var docStream = new StringReader(rootText.text);
                RootGraph root = RootGraph.Construct(docStream);
                if (root.Scriptgraphs != null)
                {
                    foreach (var script in root.Scriptgraphs)
                    {
                        var graphText = Resources.Load<TextAsset>("meta/" + script.Path);
                        Debug.Log(graphText);
                        var graphDocStream = new StringReader(graphText.text);
                        ScriptTree scriptTree = ScriptTree.Construct(graphDocStream);
                        var ser = new YamlDotNet.Serialization.Serializer();

                        scriptTrees.Add(script.Name, scriptTree);
                    }
                    return scriptTrees;
                }
                else
                {
                    return new Dictionary<string, ScriptTree>();
                }
            }
        }

        const string path = "Assets/Resources/meta/";
        // Use only in editor mode
        public static void SaveScripts(string name, ScriptTree tree)
        {
            var rootText = Resources.Load<TextAsset>("meta/root");
            var docStream = new StringReader(rootText.text);
            RootGraph root = RootGraph.Construct(docStream);
            var scriptGraph = root.Scriptgraphs.Find(sg => sg.Name == name);
            string fullPath = path + scriptGraph.Path + ".yaml";
            var sr = new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention())
                .Build();

            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(sr.Serialize(tree));
                }
            }
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
