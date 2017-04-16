using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class MoveConstant : HandlerFuture
    {
        public MoveConstant(Params ps)
            :base(ps)
        {
            SetInitialRoutine(Move);
        }

        private IEnumerator<Params> Move(Params ps)
        {
            GameObject target = ps.GetGameObject("Target");
            Vector3 velocity = ps.Get<Vector3>("Velocity");
            double currTime = 0;
            Duration duration = ps.Get<Duration>("Duration");
            while (duration.InDuration(currTime))
            {
                currTime += Time.deltaTime;
                target.GetComponent<Transform>().Translate(velocity);
                yield return ps;
            }

            //end
            yield return null;
            yield break;
        }

        protected override Dictionary<string, object> OnRequestDefaultParamMap()
        {
            Dictionary<String, object> initial = new Dictionary<string, object>();
            initial.Add("Target", "test1");
            initial.Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f));
            initial.Add("Duration", Duration.FromString("0.5"));
            return initial;
        }
    }
}
