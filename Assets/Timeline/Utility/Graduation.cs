using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Timeline.Utility
{
    static class Graduation
    {
        public static String GetGraduationString(float start, float end, float step)
        {
            String result = "";
            for(float marker = 0; marker <= end; marker += step)
            {
                result += marker.ToString();
                result += "    ";
            }
            return result;
        }
    }
}
