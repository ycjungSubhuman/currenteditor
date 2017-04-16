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

        /* Conditions for this promise*/
        protected abstract Dictionary<string, Action> GetUpdates();

        public EventPromise() {
            handler = new CustomHandler(WaitUntilTrigger, Params.Empty);
        }

        public HandlerFuture Handler
        {
            get
            {
                return handler;
            }
        }

        /* After this method is called, the conditions for this promise will be checked every frame */
        public void StartPollUpdateGlobal()
        {
            foreach(KeyValuePair<string, Action> pair in GetUpdates())
            {
                MonoHelper.MonoRegisterUpdateFunction(pair.Key, pair.Value);
            }
        }

        /* Update single time */
        public void Update()
        {
            foreach(KeyValuePair<string, Action> pair in GetUpdates())
            {
                pair.Value();
            }
        }
        public void Reset()
        {
            triggered = false;
        }

        public void Trigger(Params input)
        {
            triggered = true;
        }

        public bool IsTriggered()
        {
            return triggered;
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
