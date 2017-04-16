using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Handler
{
    class DoNothing: HandlerFuture
    {
        public DoNothing() : base(Params.Empty)
        {
            SetInitialRoutine(Nothing);
        }

        private IEnumerator<Params> Nothing(Params _)
        {
            yield return null;
            yield break;
        }

        protected override Dictionary<string, object> OnRequestDefaultParamMap()
        {
            return new Dictionary<string, object>();
        }
    }
}
