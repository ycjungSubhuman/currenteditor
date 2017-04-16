using Assets.Core;
using Assets.Core.Event;
using Assets.Core.Graph;
using Assets.Core.Handler;
using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet;

public class RootScript : MonoBehaviour {

    private Dictionary<string, ClipGraph> clips = new Dictionary<string, ClipGraph>();
    private Dictionary<string, ScriptGraph> scripts = new Dictionary<string, ScriptGraph>();

	// Use this for initialization
	void Start () {
        InitMetaData();
        Test2();
        Test1();
	}

    private void Test1()
    {
        //On Pressed Z, test1 will move right for 3 secs
        EventPromise p = new KeyDownEvent(KeyCode.Z);
        p.StartPollUpdateGlobal();
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.0f, 0.01f, 0.0f))
                .Add("Duration", Duration.FromString("0.5"));
            HandlerFuture hf = new MoveConstant(newParams);
            hf.SetAfter((_) => new MoveConstant(Params.Empty));
            return hf;
        }
        );
        p.Handler.Begin();
    }

    private void Test2()
    {
        //On Pressed ->, test1 starts to move, on release, test1 stops
        EventPromise p = new KeyDownEvent(KeyCode.RightArrow);
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
            .Add("Target", "test1")
            .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f))
            .Add("Duration", Duration.FromString("inf"));
            HandlerFuture hf = new MoveConstant(newParams);
            EventPromise up = new KeyUpEvent(KeyCode.RightArrow);
            hf.AddExternalCondition(up, (_) => new DoNothing(), true);
            return hf;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void InitMetaData()
    {
        var rootText = Resources.Load<TextAsset>("meta/root");
        var docStream = new StringReader(rootText.text);
        RootGraph root = RootGraph.Construct(docStream);
        foreach(var clip in root.Clips)
        {
            var clipText = Resources.Load<TextAsset>("meta/"+clip.Path);
            var clipDocStream = new StringReader(clipText.text);
            ClipGraph clipGraph = ClipGraph.Construct(clipDocStream);
            var ser = new YamlDotNet.Serialization.Serializer();

            clips.Add(clip.Name, clipGraph);
            Debug.Log("Constructed a clip graph : " + clip.Name);
            Debug.Log(ser.Serialize(clipGraph));
        }
        foreach(var graph in root.Scriptgraphs)
        {
            var graphText = Resources.Load<TextAsset>("meta/"+graph.Path);
            Debug.Log(graphText);
            var graphDocStream = new StringReader(graphText.text);
            ScriptGraph scriptGraph = ScriptGraph.Construct(graphDocStream);
            var ser = new YamlDotNet.Serialization.Serializer();

            scripts.Add(graph.Name, scriptGraph);
            Debug.Log("Constructed a script graph : " + graph.Name);
            Debug.Log(ser.Serialize(scriptGraph));
        }
    }
	
	// Update is called once per frame
	void Update () {
		foreach(Action action in MonoHelper.Updates.Values)
        {
            action();
        }
	}
}
