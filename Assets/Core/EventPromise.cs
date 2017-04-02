using Assets.Core.Handler;
using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public abstract class EventPromise
    {
        private HandlerFuture handler;
        private bool triggered = false;

        protected abstract Dictionary<string, Action> OnRequestInitialUpdates();

        public EventPromise() {
            handler = new CustomHandler(WaitUntilTrigger, Params.Empty);
            foreach(KeyValuePair<string, Action> pair in OnRequestInitialUpdates())
            {
                MonoHelper.MonoRegisterUpdateFunction(pair.Key, pair.Value);
            }
        }

        public HandlerFuture Handler
        {
            get
            {
                return handler;
            }
        }

        public void Trigger(Params input)
        {
            triggered = true;
        }

        //This may not be necessary, so not implementing right now
        public void Fail(Exception e)
        {
            throw new NotImplementedException();
        }

        private IEnumerator<Params> WaitUntilTrigger(Params dummy)
        {
            while (true)
            {
                if (triggered)
                {
                    yield return null;
                    triggered = false;
                }
                else
                {
                    yield return Params.Empty;
                }
            }
        }
    }
}
