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
        public KeyUpEvent(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }

        protected override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("KeyUp" + keyCode.ToString(), () => 
            {
                if (Input.GetKeyUp(keyCode))
                {
                    Trigger(Params.Empty);
                }
            }
            );
            return updates;
        }
    }
}
