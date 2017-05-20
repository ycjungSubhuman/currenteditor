using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Event
{
    class SceneReady: EventPromise
    {
        public SceneReady(): base(Params.Empty) { }
        public SceneReady(Params ps)
            :base(ps)
        {
        }

        public override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("SceneReady", () =>
            {
            }
            );
            return updates;
        }

        public override Dictionary<string, string> GetDefaultParams()
        {
            return new Dictionary<string, string>()
            {
            };
        }
    }
}
