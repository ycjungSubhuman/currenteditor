using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Timeline.Utility;

namespace Assets.Core.Handler
{
    class MoveConstant : HandlerFuture
    {
        public MoveConstant() : base(Params.Empty){ }
        public MoveConstant(Params ps)
            :base(ps)
        {
            SetInitialRoutine(Move);
        }

        private IEnumerator<Params> Move(Params ps)
        {
            GameObject target = ps.GetGameObject("Target");
            Vector3 velocity = ps.GetVector3("Velocity");
            double currTime = 0;
            Duration duration = ps.GetDuration("Duration");
            while (duration.InDuration(currTime))
            {
                currTime += Time.deltaTime;
                if(target != null)
                    target.GetComponent<Transform>().Translate(velocity);
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
            initial.Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f).GetSerialized());
            initial.Add("Duration", "0.5");
            return initial;
        }
    }
}
