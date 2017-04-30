using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    /* Wrapper class for duration */
    public class Duration
    {
        bool isInf;
        double time;
        private Duration(bool isInf, double time) {
            this.isInf = isInf;
            this.time = time;
        }

        public static Duration FromString(String duration)
        {
            if (duration == "inf") return new Duration(true, 0);
            else return new Duration(false, Convert.ToDouble(duration));
        }

        Duration Inf {
            get
            {
                return new Duration(true, 0);
            }
        }

        public bool InDuration(double currTime)
        {
            if (isInf) return true;
            else return currTime <= time;
        }
    }
}
