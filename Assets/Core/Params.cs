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

        public Params Add<T>(String name, T obj) {
            map.Add(name, obj);
            return this;
        }

        public T Get<T>(String name) {
            return (T)map[name];
        }

        public GameObject GetGameObject(String nameKey)
        {
            return GameObject.Find(map[nameKey] as String);
        }
    }
}
