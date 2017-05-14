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
        public PlayClip() : base(Params.Empty){ }
        public PlayClip(Params ps) : base(ps)
        {
            SetInitialRoutine(Play);
        }

        private IEnumerator<Params> Play(Params ps)
        {
            String clipName = ps.GetString("ClipName");
            bool loop = ps.GetBool("Loop");
            Clip clip = MonoHelper.MonoFindClip(clipName);

            clip.Audio.loop = loop;
            clip.Play();
            yield return ps;
            //while(clip.Audio.isPlaying)
            while(clip.Audio.time < clip.ExpectedTime - Time.deltaTime)
            {
                clip.UpdateTime();
                yield return ps;
            }
            yield return null;
            yield break;
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            Dictionary<string, string> initial = new Dictionary<string, string>();
            initial.Add("ClipName", "");
            initial.Add("Loop", "false");
            return initial;
        }
    }
}
