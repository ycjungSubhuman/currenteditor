using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class InterpolateMove : HandlerFuture
    {
        public InterpolateMove() : base(Params.Empty){ }
        public InterpolateMove(Params ps)
            :base(ps)
        {
            SetInitialRoutine(Animate);
        }

        private IEnumerator<Params> Animate(Params ps)
        {
            GameObject target = ps.GetGameObject("Target");
            string curveNmae = ps.GetString("Curve");
            Vector3 start = target.GetComponent<Transform>().position;
            Vector3 end = ps.GetVector3("Delta") + start;
            float duration = ps.GetFloat("Time");

            var curve1 = Curve.Get(curveNmae, start.x, end.x, duration);
            var curve2 = Curve.Get(curveNmae, start.y, end.y, duration);
            var curve3 = Curve.Get(curveNmae, start.z, end.z, duration);
            while (curve1.MoveNext() && curve2.MoveNext() && curve3.MoveNext())
            {
                target.GetComponent<Transform>().position = new Vector3(curve1.Current, curve2.Current, curve3.Current);
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
            initial.Add("Curve", "EaseIn");
            initial.Add("Delta", "0.100|0.000|0.000");
            initial.Add("Time", "0.5");
            return initial;
        }
    }
}
