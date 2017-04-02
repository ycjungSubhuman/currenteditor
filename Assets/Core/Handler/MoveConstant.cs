using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class MoveConstant : HandlerFuture
    {
        public MoveConstant()
        {
            SetInitialRoutine(Move);
        }

        public IEnumerator<Params> Move(Params ps)
        {
            Debug.Log("MoveConstant Called");
            GameObject target = ps.GetGameObject("Target");
            Vector3 velocity = ps.Get<Vector3>("Velocity");
            double timer = 0;
            Double duration = ps.Get<Double>("Duration");
            while (timer < duration)
            {
                timer += Time.deltaTime;
                target.GetComponent<Transform>().Translate(velocity);
                yield return ps;
            }

            //end
            yield return null;
            yield break;
        }

        protected override Dictionary<string, object> OnRequestInitialParamMap()
        {
            Dictionary<String, object> initial = new Dictionary<string, object>();
            initial.Add("Target", "test1");
            initial.Add("Velocity", new Vector3(0.01f, 0.0f, 0.0f));
            initial.Add("Duration", 0.5);
            return initial;
        }
    }
}
