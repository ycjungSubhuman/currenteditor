using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class Wait : HandlerFuture
    {
        public Wait() : base(Params.Empty)
        {
        }

        public Wait(Params ps) : base(ps)
        {
            SetInitialRoutine(WaitFor);
        }

        private IEnumerator<Params> WaitFor(Params ps)
        {
            double currTime = 0;
            Duration duration = ps.GetDuration("Duration");
            while (duration.InDuration(currTime))
            {
                currTime += Time.deltaTime;
                yield return ps;
            }

            //end
            yield return null;
            yield break;
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            return new Dictionary<string, string>()
            {
                {"Duration", "inf"}
            };
        }
    }
}
