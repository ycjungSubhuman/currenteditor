using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class AnimateCurve : HandlerFuture
    {
        public AnimateCurve() : base(Params.Empty){ }
        public AnimateCurve(Params ps)
            :base(ps)
        {
            SetInitialRoutine(Animate);
        }

        private IEnumerator<Params> Animate(Params ps)
        {
            GameObject target = ps.GetGameObject("Target");
            string curveNmae = ps.GetString("Curve");
            string varName = ps.GetString("TargetVar");
            float start = 0;
            if(target != null)
                start = target.GetComponent<Animator>().GetFloat(varName);
            float end = ps.GetFloat("TargetVal");
            float duration = ps.GetFloat("Time");

            var curve = Curve.Get(curveNmae, start, end, duration);
            while(curve.MoveNext())
            {
                target.GetComponent<Animator>().SetFloat(varName, curve.Current);
                yield return ps;
            }

            //end
            yield return null;
            yield break;
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            Dictionary<String, string> initial = new Dictionary<string, string>();
            initial.Add("Target", "");
            initial.Add("TargetVar", "");
            initial.Add("Curve", "EaseIn");
            initial.Add("TargetVal", "0.000");
            initial.Add("Time", "0.5");
            return initial;
        }
    }
}
