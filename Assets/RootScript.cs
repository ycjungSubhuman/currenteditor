using Assets.Core;
using Assets.Core.Event;
using Assets.Core.Handler;
using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //On Pressed Z, test1 will move right for 3 secs
        EventPromise p = new KeyDownEvent(KeyCode.Z);
        p.Handler.SetNewAfter((ps) => new MoveConstant());
        p.Handler.Begin();
	}
	
	// Update is called once per frame
	void Update () {
		foreach(Action action in MonoHelper.Updates.Values)
        {
            action();
        }
	}
}
