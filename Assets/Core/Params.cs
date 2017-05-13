using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core
{
    public class Params
    {
        private Dictionary<String, string> map = new Dictionary<String, string>();
        private Dictionary<string, string> dataLink = new Dictionary<string, string>();

        private Params() { }
        public Params(Dictionary<string, string> map)
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

        public Params Add(String name, string obj) {
            map[name]= obj;
            return this;
        }

        public bool ContainsKey(String name)
        {
            return map.Keys.Contains(name);
        }

        public GameObject GetGameObject(String nameKey)
        {
            return GameObject.Find(map[nameKey] as String);
        }

        public string GetString(String nameKey)
        {
            return map[nameKey];
        }

        public int GetInt(String nameKey)
        {
            return int.Parse(map[nameKey]);
        }

        public bool GetBool(String nameKey)
        {
            return bool.Parse(nameKey);
        }

        public Duration GetDuration(string nameKey)
        {
            return Duration.FromString(map[nameKey]);
        }

        public Vector3 GetVector3(String nameKey)
        {
            return Vector3Helper.SerializedToVector3(map[nameKey]);
        }

        // DUMMY. Only supports direct link
        // TODO: Add Support for modules to pipeline data
        public void AddDataLInk(Dictionary<string, string> dataLink)
        {
            this.dataLink = dataLink;
        }

        public Params GetLinkedParams()
        {
            var result = new Dictionary<string, string>();
            foreach (var pair in dataLink)
            {
                result[pair.Value] = map[pair.Key];
            }
            return new Params(result);
        }
    }
}
