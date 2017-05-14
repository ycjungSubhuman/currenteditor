using Assets.Timeline.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public abstract class HandlerFuture: IDefaultParamProvider
    {
        private Func<Params, IEnumerator<Params>> routine;
        private Params initialParams = Params.Empty;

        /** Default Param Initializer */
        protected abstract Dictionary<String, string> OnRequestDefaultParamMap();

        public HandlerFuture(Params ps)
        {
            foreach (KeyValuePair<String, string> pair in OnRequestDefaultParamMap())
            {
                if(ps.ContainsKey(pair.Key))
                    initialParams.Add(pair.Key, ps.GetString(pair.Key));
                else 
                    initialParams.Add(pair.Key, pair.Value);
            }
        }

        protected void SetInitialRoutine(Func<Params, IEnumerator<Params>> func)
        {
            this.routine = func;
        }
        /* Set Next Handler to be run after this ends */
        public void AddAfter(Func<Params, HandlerFuture> genNextHandler) 
        {
            Func<Params, IEnumerator<Params>> prevRoutine = routine;
            routine = (ps) => After(ps, prevRoutine(ps), genNextHandler);
        }

        public void GroupAfter(Func<Params, HandlerFuture> genNextHandler)
        {
            Func<Params, IEnumerator<Params>> prevRoutine = routine;
            routine = (ps) => GroupAfter(ps, prevRoutine(ps), genNextHandler);
        }

        /* Set Next Handler to be run after this ends, but this one runs on another coroutine */
        public void SetNewAfter(Func<Params, HandlerFuture> genNextHandler)
        {
            Func<Params, IEnumerator<Params>> prevRoutine = routine;
            routine = (ps) => NewAfter(ps, prevRoutine(ps), genNextHandler);
        }

        private IEnumerator<Params> After(Params ps, IEnumerator<Params> prevRoutine, Func<Params, HandlerFuture> genNextHandler)
        {
            while (true)
            {
                prevRoutine.MoveNext();
                if(prevRoutine.Current == null)
                {
                    HandlerFuture nextNewHandler = genNextHandler(ps);
                    nextNewHandler.Begin();
                    yield return null;
                    yield break;
                }
                yield return prevRoutine.Current;
            }
        }

        private IEnumerator<Params> GroupAfter(Params ps, IEnumerator<Params> prevRoutine, Func<Params, HandlerFuture> genNextHandler)
        {
            IEnumerator<Params> nextRoutine = null;
            while (true)
            {
                if (nextRoutine != null)
                {
                    nextRoutine.MoveNext();
                    yield return nextRoutine.Current;
                }
                else
                {
                    prevRoutine.MoveNext();
                    if (prevRoutine.Current == null)
                    {
                        nextRoutine = genNextHandler(ps).GetCoroutine();
                        prevRoutine.Dispose();
                        nextRoutine.MoveNext();
                        yield return nextRoutine.Current;
                        continue;
                    }
                    yield return prevRoutine.Current;
                }
            }
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

        public void AddExternalCondition(EventPromise externalPromise, Func<Params, HandlerFuture> genNextHandler, bool stopOnTrigger)
        {
            Func<Params, IEnumerator<Params>> prevRoutine = routine;
            externalPromise.Handler.AddAfter(genNextHandler);
            routine = (ps) => CheckingExternalCondition(ps, prevRoutine(ps), externalPromise, genNextHandler, stopOnTrigger);
        }
        
        private IEnumerator<Params> CheckingExternalCondition(Params ps, IEnumerator<Params> prevRoutine, EventPromise externalPromise, Func<Params, HandlerFuture> genNextHandler, bool stopOnTrigger)
        {
            externalPromise.Reset();
            while(true)
            {
                prevRoutine.MoveNext();
                if (externalPromise.IsTriggered())
                {
                    externalPromise.Reset();
                    genNextHandler(ps).Begin();
                    if (stopOnTrigger)
                    {
                        yield break;
                    }
                }
                externalPromise.Update();
                yield return prevRoutine.Current;
                if (prevRoutine.Current == null) yield break;
            }
        }

        public void Begin()
        {
            MonoHelper.MonoStartCoroutine(GetCoroutine);
        }
        private Func<Params, IEnumerator<Params>> GetRoutine()
        {
            return routine;
        }
        public IEnumerator<Params> GetCoroutine()
        {
            return GetRoutine()(initialParams);
        }

        public Dictionary<string, string> GetDefaultParams()
        {
            return OnRequestDefaultParamMap();
        }
    }
}
