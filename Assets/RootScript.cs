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
        //Test1_2();
        LoadScripts();
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
            EventPromise px = new KeyDownEvent(Params.Empty.Add("KeyCode", "X"));
            EventPromise pc = new KeyDownEvent(Params.Empty.Add("KeyCode", "C"));
            var h = new MoveConstant(rightParams);
            var v = new MoveConstant(downParams);
            v.AddAfter((_) => h);
            h.AddAfter((_) => v);

            return v;
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
            foreach (var evtp in eventMap)
            {
                var evtNode = nodes.Find(n => n.Name == evtp.Key);
                if (evtNode.Succ != null)
                {
                    foreach (var succ in evtNode.Succ)
                    {
                        var handler = ConstructHandler(new List<StackElement>(), handlerMap, eventMap, nodes, nodes.Find(n => n.Name == succ.Dest));
                        evtp.Value.Handler.SetNewAfter((_) => handler);
                        evtp.Value.StartPollUpdateGlobal();
                        evtp.Value.Handler.Begin();
                    }
                }
            }
        }
    }

    private void ConstructSingleEvents(Dictionary<string, EventPromise> eventMap, ScriptTree.Node node)
    {
        eventMap.Add(node.Name, GetSingleEvent(node));
    }

    class StackElement
    {
        public StackElement(string name, HandlerFuture handler, ScriptTree.Node tree, ScriptTree.Succ prevSucc)
        {
            this.Name = name;
            this.Handler = handler;
            this.Node = tree;
            this.PrevSucc = prevSucc;
        }
        public string Name;
        public HandlerFuture Handler;
        public ScriptTree.Node Node;
        public ScriptTree.Succ PrevSucc;
    }

    private HandlerFuture ConstructHandler(List<StackElement> stack, Dictionary<string, HandlerFuture> handlerMap, Dictionary<string, EventPromise> eventMap, List<ScriptTree.Node> nodes, ScriptTree.Node startNode)
    {
        var currHandler = GetSingleHandler(startNode);
        stack.Add(new StackElement(startNode.Name, currHandler, startNode, null));
        StackElement lastPopped = null;
        List<KeyValuePair<HandlerFuture, HandlerFuture>> cycleList = new List<KeyValuePair<HandlerFuture, HandlerFuture>>();
        while (stack.Count > 0)
        {
            if(stack.Last().Node.Succ != null)
            {
                var succ = stack.Last().Node.Succ.Where(s => !handlerMap.ContainsKey(s.Dest)).FirstOrDefault();
                if(succ != null)
                {
                    var nextNode = nodes.Find(n => n.Name == succ.Dest);
                    if(stack.Exists(e => e.Name == succ.Dest)) //non-checked cylce
                    {
                        var cyclePoints = stack.FindAll(e => e.Name == succ.Dest);
                        foreach (var p in cyclePoints)
                        {
                            cycleList.Add(new KeyValuePair<HandlerFuture, HandlerFuture>(stack.Last().Handler, p.Handler));
                        }
                    }
                    else
                    {
                        stack.Add(new StackElement(succ.Dest, GetSingleHandler(nextNode), nextNode, succ));
                        continue;
                    }
                }
            }
            lastPopped = stack.Last();
            stack = stack.Take(stack.Count - 1).ToList();
            if (stack.Count != 0)
            {
                ApplySucc(eventMap, stack.Last().Handler, lastPopped.PrevSucc, lastPopped.Handler);
            }
            handlerMap.Add(lastPopped.Name, lastPopped.Handler);
        }
        foreach (var p in cycleList)
        {
            p.Key.AddAfter((_) => p.Value);
        }
        return lastPopped.Handler;
    }

    private void ApplySucc(Dictionary<string, EventPromise> eventMap, HandlerFuture curr, ScriptTree.Succ succ, HandlerFuture next)
    {
        if(succ.Condition == null || succ.Condition.Count == 0)
        {
            curr.AddAfter((_) => next);
        }
        else if(succ.Condition.Count == 1)
        {
            curr.AddExternalCondition(eventMap[succ.Condition[0]], (_) => next, succ.EndPrev);
        }
        else
        {
            var combined = new OrEvent(eventMap[succ.Condition[0]], eventMap[succ.Condition[1]]);
            for(int i=2; i<succ.Condition.Count; i++)
            {
                combined = new OrEvent(combined, eventMap[succ.Condition[i]]);
            }
            curr.AddExternalCondition(combined, (_) => next, succ.EndPrev);
        }
    }

    private HandlerFuture GetSingleHandler(ScriptTree.Node node)
    {
        Debug.Assert(!node.Name.Contains("Event"));

        HandlerFuture handler = null;
        if (node.Members == null)
        {
            Type t = Type.GetType("Assets.Core.Handler." + node.Base);
            handler = Activator.CreateInstance(t, new Params(node.Params)) as HandlerFuture;
        }
        else
        {
            var start = node.Members.Find(m => m.Name == node.StartMember);
            handler = GetSingleHandler(start);
            var curr = start;
            while (curr.Name != node.EndMember)
            {
                Debug.Assert(curr.Succ != null && curr.Succ.Count == 1 && curr.Succ[0].Condition.Count == 0);
                var nextTree = node.Members.Find(m => m.Name == curr.Succ[0].Dest);
                var next = GetSingleHandler(nextTree);
                handler.GroupAfter((_) => next);
                curr = nextTree;
            }
        }

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
