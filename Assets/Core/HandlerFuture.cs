using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public abstract class HandlerFuture
    {
        private Func<Params, IEnumerator<Params>> routine;
        private Params initialParams = Params.Empty;
        private HandlerFuture nextHandler;

        /** Param Initializer */
        protected abstract Dictionary<String, object> OnRequestInitialParamMap();

        public HandlerFuture()
        {
            foreach (KeyValuePair<String, object> pair in OnRequestInitialParamMap())
            {
                initialParams.Add<object>(pair.Key, pair.Value);
            }
        }

        protected void SetInitialRoutine(Func<Params, IEnumerator<Params>> func)
        {
            this.routine = func;
        }
        /* Set Next Handler to be run after this ends */
        public void SetAfter(Func<Params, HandlerFuture> genNextHander) 
        {
            throw new NotImplementedException();
        }

        /* Set Next Handler to be run after this ends, but this one runs on another coroutine */
        public void SetNewAfter(Func<Params, HandlerFuture> genNextHandler)
        {
            Func<Params, IEnumerator<Params>> prevRoutine = routine;
            routine = (ps) => NewAfter(ps, prevRoutine(ps), genNextHandler);
        }

        private IEnumerator<Params> NewAfter(Params ps, IEnumerator<Params> prevRoutine, Func<Params, HandlerFuture> genNextHandler)
        {
            while (true)
            {
                prevRoutine.MoveNext();
                if(prevRoutine.Current == null)
                {
                    HandlerFuture nextNewHandler = genNextHandler(ps);
                    nextNewHandler.Begin();
                }
                yield return prevRoutine.Current;
            }
        }

        public EventPromise GetInternalCondition(String name)
        {
            throw new NotImplementedException();
        }

        public void AddExternalCondition(EventPromise external, bool stopOnTrigger)
        {
            //TODO: Implement stopOnTrigger
            if (stopOnTrigger) throw new NotImplementedException();
            routine = (ps) =>
            {
                external.Handler.Step();
                return Step();
            };
        }
        public void Begin()
        {
            MonoHelper.MonoStartCoroutine(Step);
        }
        private Func<Params, IEnumerator<Params>> GetRoutine()
        {
            return routine;
        }
        public IEnumerator<Params> Step()
        {
            return GetRoutine()(initialParams);
        }
    }
}
