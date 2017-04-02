using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Timeline.SubWindows
{
    class NodeWindow
    {
        public Rect rect;
        public string title = "STUB";

        public GUIStyle style;

        public NodeWindow(Vector2 position, float width, float height, GUIStyle nodeStyle)
        {
            rect = new Rect(position.x, position.y, width, height);
            style = nodeStyle;
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public void Draw()
        {
            GUI.Box(rect, title, style);
        }

        public bool ProcessEvents(Event e)
        {
            return false;
        }
    }
}
