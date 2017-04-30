using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    interface IDefaultParamProvider
    {
        Dictionary<string, string> GetDefaultParams();
    }
}
