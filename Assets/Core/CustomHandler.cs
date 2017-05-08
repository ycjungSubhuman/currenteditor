using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    class CustomHandler : HandlerFuture
    {
        public CustomHandler() : base(Params.Empty){ }
        public CustomHandler(Func<Params, IEnumerator<Params>> initial, Params ps)
            :base(ps)
        {
            SetInitialRoutine(initial);
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            return new Dictionary<string, string>();
        }
    }
}
