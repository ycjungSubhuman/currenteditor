using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public static class Curve
    {
        public static Dictionary<string, Func<float, float, float, IEnumerator<float>>> curveMap = new Dictionary<string, Func<float, float, float, IEnumerator<float>>>()
        {
            {"easein", EaseIn },
            {"easeout", EaseOut },
            {"linear", Linear }
        };

        public static IEnumerator<float> Get(string type, float start, float end, float duration)
        {
            return curveMap[type.ToLower()](start, end, duration);
        }

        private static IEnumerator<float> EaseIn(float start, float end, float duration)
        {
            float startTime = Time.time;
            float a = (end - start) / (duration * duration);
            yield return start;

            while(Time.time <= startTime + duration)
            {
                var t = Time.time;
                yield return a * (t - startTime) * (t - startTime) + start;
            }
        }

        private static IEnumerator<float> EaseOut(float start, float end, float duration)
        {
            float startTime = Time.time;
            float a = (start - end) / (duration * duration);
            yield return start;

            while(Time.time <= startTime + duration)
            {
                var t = Time.time;
                yield return a * (t - startTime - duration) * (t - startTime - duration) + end;
            }
        }

        private static IEnumerator<float> Linear(float start, float end, float duration)
        {
            float startTime = Time.time;
            float a = (end - start) / duration;
            yield return start;

            while(Time.time <= startTime + duration)
            {
                var t = Time.time;
                yield return a * (t - startTime) + start;
            }
        }
    }
}
