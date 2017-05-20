using Assets.Core.Handler;
using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public abstract class EventPromise: IDefaultParamProvider
    {
        private HandlerFuture handler;
        private bool triggered = false;

        /* Conditions for this promise*/
        public abstract Dictionary<string, Action> GetUpdates();
        private Params initialParams = Params.Empty;

        public EventPromise(Params ps) {
            handler = new CustomHandler(WaitUntilTrigger, Params.Empty);
            foreach (var pair in GetDefaultParams())
            {
                if(ps.ContainsKey(pair.Key))
                    initialParams.Add(pair.Key, ps.GetString(pair.Key));
                else 
                    initialParams.Add(pair.Key, pair.Value);
            }
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
        public virtual void Reset()
        {
            triggered = false;
        }

        public void Trigger(Params input)
        {
            Debug.Log("woeif");
            triggered = true;
        }

        public bool IsTriggered()
        {
            return triggered;
        }

        public EventPromise Clone()
        {
            var cloned = (EventPromise)this.MemberwiseClone();
            cloned.handler = new CustomHandler(WaitUntilTrigger, Params.Empty);
            return cloned;
        }

        private IEnumerator<Params> WaitUntilTrigger(Params dummy)
        {
            while (true)
            {
                if (triggered)
                {
                    triggered = false;
                    Debug.Log("woeifj");
                    yield return null;
                    triggered = false;
                }
                else
                {
                    yield return Params.Empty;
                }
            }
        }

        public abstract Dictionary<string, string> GetDefaultParams();
    }
}
