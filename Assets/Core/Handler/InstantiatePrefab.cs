﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Core.Handler
{
    class InstantiatePrefab: HandlerFuture
    {
        public InstantiatePrefab() : base(Params.Empty){ }
        public InstantiatePrefab(Params ps)
            :base(ps)
        {
            SetInitialRoutine(Move);
        }

        private IEnumerator<Params> Move(Params ps)
        {
            string prefabName = ps.GetString("Prefab");
            GameObject prefab = Resources.Load<GameObject>(prefabName);
            GameObject.Instantiate(prefab);
            yield return null;
            yield break;
        }

        protected override Dictionary<string, string> OnRequestDefaultParamMap()
        {
            Dictionary<String, string> initial = new Dictionary<string, string>();
            initial.Add("Prefab", "");
            return initial;
        }
    }
}
