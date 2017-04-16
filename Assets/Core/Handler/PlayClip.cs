using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class PlayClip : HandlerFuture
    {
        public PlayClip(Params ps) : base(ps)
        {
            SetInitialRoutine(Play);
        }

        private IEnumerator<Params> Play(Params ps)
        {
            double currTime = 0;
            String clipName = ps.Get<String>("ClipName");
            bool loop = ps.Get<bool>("Loop");
            Clip clip = MonoHelper.MonoFindClip(clipName);

            clip.Audio.loop = loop;
            clip.Play();
            yield return ps;
            while(clip.Audio.isPlaying)
            {
                currTime += Time.deltaTime;
                yield return ps;
            }
            yield return null;
            yield break;
        }

        protected override Dictionary<string, object> OnRequestDefaultParamMap()
        {
            Dictionary<string, object> initial = new Dictionary<string, object>();
            initial.Add("ClipName", "");
            initial.Add("Loop", false);
            return initial;
        }
    }
}
