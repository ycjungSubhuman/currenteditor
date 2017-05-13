using Assets.Core;
using Assets.Core.Event;
using Assets.Core.Tree;
using Assets.Core.Handler;
using Assets.Midi;
using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet;
using System.Linq;

public class RootScript : MonoBehaviour {

    private Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
    public Dictionary<string, Clip> Clips { get { return clips; } }
    private Dictionary<string, ClipTree> clipGraphs = new Dictionary<string, ClipTree>();
    private Dictionary<string, ScriptTree> scriptGraphs = new Dictionary<string, ScriptTree>();

	// Use this for initialization
	void Start () {
        InitMetaData();
        Test1_2();
	}

    private void Test1_2()
    {
        //On Pressed Z, test1 will (move right + move down) for 0.5 secs after moving up for 0.5 secs.
        EventPromise p = new KeyDownEvent(Params.Empty.Add("KeyCode", "Z"));
        p.StartPollUpdateGlobal();
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.0f, 0.01f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            HandlerFuture hf = new MoveConstant(newParams);
            Params rightParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            Params downParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.0f, -0.01f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            Params leftParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(-0.01f, 0.0f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            hf.GroupAfter((_) => new MoveConstant(leftParams));

            hf.AddAfter((_) => new MoveConstant(rightParams));
            hf.AddAfter((_) => new MoveConstant(downParams));
            return hf;
        }
        );
        p.Handler.Begin();
    }

    private void InitMetaData()
    {
        var rootText = Resources.Load<TextAsset>("meta/root");
        var docStream = new StringReader(rootText.text);
        RootGraph root = RootGraph.Construct(docStream);
        foreach(var clip in root.Clips)
        {
            //load clip
            var clipText = Resources.Load<TextAsset>("meta/"+clip.Path);
            var clipDocStream = new StringReader(clipText.text);
            ClipTree clipGraph = ClipTree.Construct(clipDocStream);
            var ser = new YamlDotNet.Serialization.Serializer();

            clipGraphs.Add(clip.Name, clipGraph);
            var midiFile = Resources.Load<TextAsset>("clips/"+clipGraph.Midi);
            Debug.Log(clipGraph.Midi);

            AudioSource audioSource = AddAudioSourceToScene(clipGraph.Audio);

            //parse midi
            Midi midi = new MidiParser().Parse(midiFile.bytes);
            //debug
            Debug.Log(midi.Tracks[2].Bpm);
            foreach(var msg in midi.Tracks[2].Messages)
            {
                Debug.Log(msg);
            }
            Debug.Log(ser.Serialize(clipGraph));
            clips.Add(clip.Name, new Assets.Core.Clip(audioSource, midi));
        }
        foreach(var graph in root.Scriptgraphs)
        {
            var graphText = Resources.Load<TextAsset>("meta/"+graph.Path);
            Debug.Log(graphText);
            var graphDocStream = new StringReader(graphText.text);
            ScriptTree scriptGraph = ScriptTree.Construct(graphDocStream);
            var ser = new YamlDotNet.Serialization.Serializer();

            scriptGraphs.Add(graph.Name, scriptGraph);
            Debug.Log("Constructed a script graph : " + graph.Name);
            Debug.Log(ser.Serialize(scriptGraph));
        }
    }
    private void LoadScripts()
    {
        foreach (var p in Meta.ScriptTrees) {
            var nodes = p.Value.Nodes;
            Dictionary<string, HandlerFuture> handlerMap = new Dictionary<string, HandlerFuture>();
            Dictionary<string, EventPromise> eventMap = new Dictionary<string, EventPromise>();

            foreach (var node in nodes)
            {
                if (node.Base.Contains("Event"))
                {
                    ConstructSingleEvents(eventMap, node);
                }
            }
            foreach (var node in nodes)
            {
                if (node.Base == "") //group
                {
                }
                else if (node.Base.Contains("Event"))
                {
                }
                else //handler
                {
                    var constructed = ConstructHandler(handlerMap, eventMap, nodes, node);
                }
            }
        }
    }

    private void ConstructSingleEvents(Dictionary<string, EventPromise> eventMap, ScriptTree.Node node)
    {
        eventMap.Add(node.Name, GetSingleEvent(node));
    }

    private HandlerFuture ConstructHandler(Dictionary<string, HandlerFuture> handlerMap, Dictionary<string, EventPromise> eventMap, List<ScriptTree.Node> nodes, ScriptTree.Node startNode)
    {
        var currHandler = GetSingleHandler(startNode);

        if(startNode.Succ == null)
        {
            return currHandler;
        }
        else
        {
            var tNode = startNode;
            foreach (var succ in tNode.Succ)
            {
                HandlerFuture nextHandler = null;
                if (handlerMap.ContainsKey(succ.Dest))
                {
                    nextHandler = handlerMap[succ.Dest];
                }
                else
                {
                    nextHandler = ConstructHandler(handlerMap, eventMap, nodes, nodes.Find(n => n.Name == succ.Dest));
                }
                ApplySucc(currHandler, succ, nextHandler);
                handlerMap.Add(succ.Dest, nextHandler);
            }
            return currHandler;
        }
    }

    private void ApplySucc(HandlerFuture curr, ScriptTree.Succ succ, HandlerFuture next)
    {
    }

    private HandlerFuture GetSingleHandler(ScriptTree.Node node)
    {
        Debug.Assert(!node.Name.Contains("Event") && node.Name != "");

        Type t = Type.GetType("Assets.Core.Handler." + node.Base);
        var handler = Activator.CreateInstance(t, new Params(node.Params)) as HandlerFuture;

        return handler;
    }

    private EventPromise GetSingleEvent(ScriptTree.Node node)
    {
        Debug.Assert(node.Name.Contains("Event"));

        Type t = Type.GetType("Assets.Core.Event." + node.Base);
        var evt = Activator.CreateInstance(t, new Params(node.Params)) as EventPromise;
        return evt;
    }

    private AudioSource AddAudioSourceToScene(String audioFilePath)
    {
        var audioClip = Resources.Load<AudioClip>("clips/" + audioFilePath);
        GameObject audioObject = new GameObject();
        audioObject.AddComponent<AudioSource>();
        AudioSource audioSource = audioObject.GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        return audioSource;
    }
	
	// Update is called once per frame
	void Update () {
		foreach(Action action in MonoHelper.Updates.Values)
        {
            action();
        }
	}
}
