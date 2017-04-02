using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Handler
{
    class CustomHandler : HandlerFuture
    {
        public CustomHandler(Func<Params, IEnumerator<Params>> initial)
        {
            SetInitialRoutine(initial);
        }

        protected override Dictionary<string, object> OnRequestInitialParamMap()
        {
            return new Dictionary<string, object>();
        }
    }
}
