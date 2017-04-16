using Assets.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Timeline.Utility
{
    public class MonoHelper
    {
        public static Dictionary<string, Action> Updates = new Dictionary<string, Action>();
        public static void MonoStartCoroutine(Func<IEnumerator> coroutine)
        {
            GameObject root = GameObject.Find("UnityCurrentRootObject");
            root.GetComponent<MonoBehaviour>().StartCoroutine(coroutine());
        }

        public static void MonoRegisterUpdateFunction(string name, Action action)
        {
            Updates.Add(name, action);
        }

        public static Clip MonoFindClip(String clipName)
        {
            GameObject root = GameObject.Find("UnityCurrentRootObject");
            Clip clip;
            try
            {
                clip = root.GetComponent<RootScript>().Clips[clipName];
            } catch(KeyNotFoundException e)
            {
                clip = null;
            }
            return clip;
        }
    }
}
