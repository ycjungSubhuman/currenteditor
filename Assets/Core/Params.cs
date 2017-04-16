using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public class Params
    {
        private Dictionary<String, object> map = new Dictionary<String, object>();

        public static Params Empty
        {
            get
            {
                return new Params();
            }
        }

        public Params Add(String name, object obj) {
            map[name]= obj;
            return this;
        }

        public T Get<T>(String name) {
            return (T)map[name];
        }

        public bool ContainsKey(String name)
        {
            return map.Keys.Contains(name);
        }

        public GameObject GetGameObject(String nameKey)
        {
            return GameObject.Find(map[nameKey] as String);
        }
    }
}
