using Assets.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public NodeWindow(String baseClassName, Vector2 position, float width, float height, 
            GUIStyle inPointStyle, 
            GUIStyle outPointStyle, Action<Connection.Point> onClickInPoint, Action<Connection.Point> onClickOutPoint)
        {
            Type t = null;
            this.baseClassName = baseClassName;
            if (baseClassName.Contains("Event"))
            {
                t = Type.GetType("Assets.Core.Event." + baseClassName);
                style = (GUIStyle)"flow node hex 3";
            }
            else
            {
                t = Type.GetType("Assets.Core.Handler." + baseClassName);
                style = (GUIStyle)"flow node 1";
            }

            IDefaultParamProvider p = Activator.CreateInstance(t) as IDefaultParamProvider;
            foreach(var pair in p.GetDefaultParams())
            {
                paramPairs.Add(pair);
            }

            style.border = new RectOffset(12, 12, 12, 12);
            rect = new Rect(position.x, position.y, width, height);
            inPoint = new Connection.Point(this, Connection.Point.Type.IN, inPointStyle, onClickInPoint);
            outPoint = new Connection.Point(this, Connection.Point.Type.OUT, outPointStyle, onClickOutPoint);
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public void Draw()
        {
            inPoint.Draw();
            outPoint.Draw();
            GUI.Box(rect, "", style);
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 18f), new GUIContent(baseClassName), titleStyle);
            DrawParams();
        }

        String test = "";
        float paramMarginX = 1f;
        float paramMarginY = 0.7f;
        private void DrawParams()
        {
            test = GUI.TextField(new Rect(rect.x+paramMarginX, rect.y+18f, rect.width/2-2*paramMarginX, 16f), test);
            test = GUI.TextField(new Rect(rect.x+paramMarginX, rect.y+18*2+paramMarginY, rect.width/2-2*paramMarginX, 14f), test);
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
                        Debug.Log("Drag start");
                        GUI.changed = true;
                        dragged = true;
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
        public bool MouseOverlapped(Vector2 mousePosition)
        {
            return rect.Contains(mousePosition);
        }
    }
}
