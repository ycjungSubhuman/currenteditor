﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Event
{
    class KeyDownEvent : EventPromise
    {
        private KeyCode keyCode;
        public KeyDownEvent(KeyCode keyCode)
        {
            this.keyCode = keyCode;
        }

        protected override Dictionary<string, Action> GetUpdates()
        {
            Dictionary<string, Action> updates = new Dictionary<string, Action>();
            updates.Add("KeyDown" + keyCode.ToString(), () => 
            {
                if (Input.GetKeyDown(keyCode))
                {
                    Trigger(Params.Empty);
                }
            }
            );
            return updates;
        }
    }
}
