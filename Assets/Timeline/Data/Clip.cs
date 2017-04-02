using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline.Data
{
    class Clip : ScriptableObject
    {
        private String clipTitle;

        //STUB Constructor
        public Clip(String clipTitle)
        {
            this.clipTitle = clipTitle;
        }

        [MenuItem("Assets/Create/Timeline/New Clip")]
        public static void Create()
        {
            //TODO: Create *.clp in Assets folder
        }

        public override string ToString()
        {
            return clipTitle;
        }
    }
}
