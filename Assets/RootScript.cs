using Assets.Core;
using Assets.Core.Event;
using Assets.Core.Handler;
using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet;

public class RootScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
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
    }
	
	// Update is called once per frame
	void Update () {
		foreach(Action action in MonoHelper.Updates.Values)
        {
            action();
        }
	}
}
