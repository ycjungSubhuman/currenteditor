using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Timeline.Utility
{
    public static class Vector3Helper
    {
        public static string GetSerialized(this Vector3 input)
        {
            return String.Format("{0:0.000}|{1:0.000}|{2:0.000}",input.x, input.y, input.z);
        }

        private static string pattern = @"^([-]?[0-9]+(?:\.[0-9]+)?)\|([-]?[0-9]+(?:\.[0-9]+)?)\|([-]?[0-9]+(?:\.[0-9]+)?)$";
        public static Vector3 SerializedToVector3(string serialized)
        {
            Regex regex = new Regex(pattern);
            MatchCollection mc = regex.Matches(serialized);

            float x = 0, y = 0, z = 0;
            foreach(Match m in mc)
            {
                x = float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture.NumberFormat);
                y = float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture.NumberFormat);
                z = float.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture.NumberFormat);
            }

            return new Vector3(x, y, z);
        }
    }
}
