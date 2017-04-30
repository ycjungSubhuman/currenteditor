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
        public string title = "STUB";
        private bool dragged = false;

        public Connection.Point inPoint;
        public Connection.Point outPoint;

        public List<KeyValuePair<string, string>> paramPairs = new List<KeyValuePair<string, string>>();

        public GUIStyle style;

        public NodeWindow(String baseClassName, Vector2 position, float width, float height, 
            GUIStyle nodeStyle, GUIStyle inPointStyle, 
            GUIStyle outPointStyle, Action<Connection.Point> onClickInPoint, Action<Connection.Point> onClickOutPoint)
        {
            Type t = null;
            if(baseClassName.Contains("Event"))
                t = Type.GetType("Assets.Core.Event."+baseClassName);
            else
                t = Type.GetType("Assets.Core.Handler."+baseClassName);

            IDefaultParamProvider p = Activator.CreateInstance(t) as IDefaultParamProvider;
            Debug.Log(baseClassName);
            foreach(var pair in p.GetDefaultParams())
            {
                paramPairs.Add(pair);
                Debug.Log(pair.Key + "/" + pair.Value);
            }

            rect = new Rect(position.x, position.y, width, height);
            style = nodeStyle;
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
            GUI.Box(rect, title, style);
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
