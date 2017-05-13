using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class OrEvent : EventPromise
    {
        EventPromise a;
        EventPromise b;
        public OrEvent(EventPromise a, EventPromise b) : base(Params.Empty)
        {
            this.a = a;
            this.b = b;
        }

        public override Dictionary<string, string> GetDefaultParams()
        {
            return new Dictionary<string, string>();
        }

        public override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> result = new Dictionary<string, Action>();
            var allUpdates = a.GetUpdates().Concat(b.GetUpdates()).ToDictionary(x=>x.Key, y=>y.Value);
            result.Add("OR:"+allUpdates.Aggregate("", (acc, p) => acc + p.Key), () =>
            {
                foreach(var up in allUpdates)
                {
                    up.Value();
                }
                if(a.IsTriggered() || b.IsTriggered())
                {
                    Trigger(Params.Empty);
                }
            }
            );
            return result;
        }
    }
}
