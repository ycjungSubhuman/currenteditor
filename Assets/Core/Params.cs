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
        private Dictionary<string, string> dataLink = new Dictionary<string, string>();

        private Params() { }
        private Params(Dictionary<string, object> map)
        {
            this.map = map;
        }

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

        // DUMMY. Only supports direct link
        // TODO: Add Support for modules to pipeline data
        public void AddDataLInk(Dictionary<string, string> dataLink)
        {
            this.dataLink = dataLink;
        }

        public Params GetLinkedParams()
        {
            var result = new Dictionary<string, object>();
            foreach (var pair in dataLink)
            {
                result[pair.Value] = map[pair.Key];
            }
            return new Params(result);
        }
    }
}
