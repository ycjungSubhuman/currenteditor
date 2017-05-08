﻿using Assets.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline.SubWindows
{
    public class NodeWindow
    {
        public Rect rect;
        private bool dragged = false;

        public Connection.Point inPoint;
        public Connection.Point outPoint;

        public List<KeyValuePair<string, string>> paramPairs = new List<KeyValuePair<string, string>>();

        public GUIStyle style;
        private string baseClassName;
        public string nodeName;
        const float paramMarginX = 6f;
        const float paramMarginY = 3f;
        const float nameMargin = 30f;
        bool isEvent;
        Action<NodeWindow> onClickRemove;

        public NodeWindow(String baseClassName, string initialName, Vector2 position, float width, 
            GUIStyle inPointStyle, 
            GUIStyle outPointStyle, Action<Connection.Point> onClickInPoint, Action<Connection.Point> onClickOutPoint, Action<NodeWindow> onClickRemove)
        {
            Type t = null;
            this.baseClassName = baseClassName;
            this.onClickRemove = onClickRemove;
            nodeName = initialName;
            if (baseClassName.Contains("Event"))
            {
                t = Type.GetType("Assets.Core.Event." + baseClassName);
                style = (GUIStyle)"flow node 2";
                isEvent = true;
            }
            else
            {
                t = Type.GetType("Assets.Core.Handler." + baseClassName);
                style = (GUIStyle)"flow node 1";
                isEvent = false;
            }

            IDefaultParamProvider p = Activator.CreateInstance(t) as IDefaultParamProvider;
            foreach(var pair in p.GetDefaultParams())
            {
                paramPairs.Add(pair);
            }

            style.border = new RectOffset(12, 12, 12, 12);
            rect = new Rect(position.x, position.y, width, 40f + paramPairs.Count*20f);
            inPoint = new Connection.Point(this, Connection.Point.Type.IN, inPointStyle, onClickInPoint);
            outPoint = new Connection.Point(this, Connection.Point.Type.OUT, outPointStyle, onClickOutPoint);
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public void Draw()
        {
            if(!isEvent)
                inPoint.Draw();
            outPoint.Draw();
            GUI.Box(rect, "", style);
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 18f), new GUIContent(baseClassName), titleStyle);

            nodeName = GUI.TextField(new Rect(rect.x + nameMargin, rect.y+20f, rect.width - 2*nameMargin, 18f), nodeName);
            DrawParams();
        }

        private void DrawParams()
        {
            for(int i=0; i<paramPairs.Count; i++)
            {
                string name = paramPairs[i].Key;
                string value = paramPairs[i].Value;
                string newValue = value;
                GUI.Label(new Rect(rect.x+paramMarginX, rect.y+40f+i*20f, rect.width/2 -2*paramMarginX, 16f), new GUIContent(name));
                newValue = GUI.TextField(new Rect(rect.x + paramMarginX + rect.width / 2, rect.y + 40f+i*20f, rect.width / 2 - 2 * paramMarginX, 16f), newValue);
                paramPairs[i] = new KeyValuePair<string, string>(name, newValue);
            }
        }

        public bool ProcessEvents(Event e)
        {
            switch(e.type)
            {
                case EventType.MouseDrag:
                    if(dragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
                case EventType.MouseDown:
                    if(e.button == 0 && MouseOverlapped(e.mousePosition))
                    {
                        GUI.changed = true;
                        dragged = true;
                    }
                    if(e.button == 1 && MouseOverlapped(e.mousePosition))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete"), false, () => OnClickDelete());
                        menu.ShowAsContext();
                    }
                    break;
                case EventType.MouseUp:
                    if(e.button == 0)
                    {
                        GUI.changed = true;
                        dragged = false;
                    }
                    break;
            }

            return false;
        }
        private void OnClickDelete()
        {
            onClickRemove(this);
        }
        public bool MouseOverlapped(Vector2 mousePosition)
        {
            return rect.Contains(mousePosition);
        }
    }
}
