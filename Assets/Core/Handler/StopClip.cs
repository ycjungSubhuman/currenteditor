using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class StopClip : HandlerFuture
    {
        public StopClip() : base(Params.Empty){ }
        public StopClip(Params ps) : base(ps)
        {
            SetInitialRoutine(Stop);
        }

        private IEnumerator<Params> Stop(Params ps)
        {
            String clipName = ps.GetString("ClipName");
            Clip clip = MonoHelper.MonoFindClip(clipName);

            clip.Stop();
            yield return null;
            yield break;
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            Dictionary<string, string> initial = new Dictionary<string, string>();
            initial.Add("ClipName", "");
            return initial;
        }
    }
}
