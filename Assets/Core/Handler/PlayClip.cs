using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            String clipName = ps.Get<String>("ClipName");
        }

        protected override Dictionary<string, object> OnRequestDefaultParamMap()
        {
            Dictionary<string, object> initial = new Dictionary<string, object>();
            initial.Add("ClipName", "");
            return initial;
        }
    }
}
