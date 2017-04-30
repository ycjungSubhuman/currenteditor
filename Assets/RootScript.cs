using Assets.Core;
using Assets.Core.Event;
using Assets.Core.Graph;
using Assets.Core.Handler;
using Assets.Midi;
using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet;

public class RootScript : MonoBehaviour {

    private Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
    public Dictionary<string, Clip> Clips { get { return clips; } }
    private Dictionary<string, ClipGraph> clipGraphs = new Dictionary<string, ClipGraph>();
    private Dictionary<string, ScriptGraph> scriptGraphs = new Dictionary<string, ScriptGraph>();

	// Use this for initialization
	void Start () {
        InitMetaData();
        Test7();
	}

    private void Test1()
    {
        //On Pressed Z, test1 will move right for 0.5 secs after moving up for 0.5 secs.
        EventPromise p = new KeyDownEvent(KeyCode.Z);
        p.StartPollUpdateGlobal();
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.0f, 0.01f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            HandlerFuture hf = new MoveConstant(newParams);
            hf.SetAfter((_) => new MoveConstant(Params.Empty.Add("Target", "test1")));
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
            .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized())
            .Add("Duration", "inf");
            HandlerFuture hf = new MoveConstant(newParams);
            EventPromise up = new KeyUpEvent(KeyCode.RightArrow);
            hf.AddExternalCondition(up, (_) => new DoNothing(), true);
            return hf;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void Test3()
    {
        //While Pressing X, testclip1 plays
        EventPromise p = new KeyDownEvent(KeyCode.X);
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
            .Add("ClipName", "testclip1")
            .Add("Loop", "true");
            HandlerFuture hf = new PlayClip(newParams);
            EventPromise up = new KeyUpEvent(KeyCode.X);
            hf.AddExternalCondition(up, (ps2) => {
                ps2.Add("ClipName", "testclip1");
                return new StopClip(ps2);
                }, false);
            return hf;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void Test4()
    {
        //On NOTE ON of CHANNEL 9 in testclip1, test1 moves right for 0.05 seconds
        EventPromise p = new ClipMidiEvent("testclip1", 9, MidiMessage.Type.NOTE_ON);
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.01f, 0.00f, 0.0f).GetSerialized())
                .Add("Duration", "0.05");
            HandlerFuture hf = new MoveConstant(newParams);
            return hf;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void Test5()
    {
        //On Pressed ->, test1 starts to move, up/right repeatedly on release, test1 stops
        EventPromise p = new KeyDownEvent(KeyCode.RightArrow);
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams1 = Params.Empty
            .Add("Target", "test1")
            .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized())
            .Add("Duration", "0.1");

            Params newParams2 = Params.Empty
            .Add("Target", "test1")
            .Add("Velocity", new Vector3(0.00f, 0.01f, 0.0f).GetSerialized())
            .Add("Duration", "0.1");
            HandlerFuture right = new MoveConstant(newParams1);
            HandlerFuture up = new MoveConstant(newParams2);

            right.SetAfter((_) => up);
            up.SetAfter((_) => right);
            EventPromise keyUp = new KeyUpEvent(KeyCode.RightArrow);
            right.AddExternalCondition(keyUp, (_) => new DoNothing(), true);
            return right;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void Test6()
    {
        //On Pressed ->, test1 starts to move right for 1 second. If pressed up while moving right, restarts this handler. After moving for 2 seconds, move up for 1 second.
        EventPromise p = new KeyDownEvent(KeyCode.RightArrow);
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams1 = Params.Empty
            .Add("Target", "test1")
            .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized())
            .Add("Duration", "1");

            Params newParams2 = Params.Empty
            .Add("Target", "test1")
            .Add("Velocity", new Vector3(0.00f, 0.01f, 0.0f).GetSerialized())
            .Add("Duration", "1");
            HandlerFuture right = new MoveConstant(newParams1);
            HandlerFuture up = new MoveConstant(newParams2);

            EventPromise upDown = new KeyDownEvent(KeyCode.UpArrow);
            right.AddExternalCondition(upDown, (_) => right, true);
            right.SetAfter((_) => up);
            return right;
        }
        );
        p.StartPollUpdateGlobal();
        p.Handler.Begin();
    }

    private void Test7()
    {
        //On Pressed Z, test1 will move right for 0.5 secs after moving up for 0.5 secs. The target is tranferred using DataLink
        EventPromise p = new KeyDownEvent(KeyCode.Z);
        p.StartPollUpdateGlobal();
        p.Handler.SetNewAfter((ps) =>
        {
            Params newParams = Params.Empty
                .Add("Target", "test1")
                .Add("Velocity", new Vector3(0.0f, 0.01f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            HandlerFuture up = new MoveConstant(newParams);
            var dataLink = new Dictionary<string, string>();
            dataLink.Add("Target", "Target");
            newParams.AddDataLInk(dataLink);
            Params params2 = newParams.GetLinkedParams()
                .Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized())
                .Add("Duration", "0.5");
            HandlerFuture right = new MoveConstant(params2);
            up.SetAfter((_) => right);
            return up;
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
            ClipGraph clipGraph = ClipGraph.Construct(clipDocStream);
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
            ScriptGraph scriptGraph = ScriptGraph.Construct(graphDocStream);
            var ser = new YamlDotNet.Serialization.Serializer();

            scriptGraphs.Add(graph.Name, scriptGraph);
            Debug.Log("Constructed a script graph : " + graph.Name);
            Debug.Log(ser.Serialize(scriptGraph));
        }
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
