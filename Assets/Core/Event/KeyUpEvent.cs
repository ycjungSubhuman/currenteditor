using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Event
{
    class KeyUpEvent: EventPromise
    {
        private KeyCode keyCode;
        public KeyUpEvent(): base(Params.Empty) { }
        public KeyUpEvent(Params ps)
            :base(ps)
        {
            this.keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), ps.GetString("KeyCode"));
        }

        public override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("KeyUp" + keyCode.ToString(), () => 
            {
                if (Input.GetKeyUp(keyCode))
                {
                    Debug.Log("WOEIFJOWEI");
                    Trigger(Params.Empty);
                }
            }
            );
            return updates;
        }

        public override Dictionary<string, string> GetDefaultParams()
        {
            return new Dictionary<string, string>()
            {
                { "KeyCode", KeyCode.Z.ToString() },
            };
        }
    }
}
