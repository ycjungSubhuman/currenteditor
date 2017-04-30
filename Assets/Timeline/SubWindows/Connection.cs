using Assets.Timeline.SubWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline.SubWindows
{
    public class Connection
    {
        public Point inPoint;
        public Point outPoint;
        public Action<Connection> onClickRemoveConnection;

        public Connection(Connection.Point inPoint, Connection.Point outPoint, Action<Connection> onClickRemoveConnection)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            this.onClickRemoveConnection = onClickRemoveConnection;
        }

        Vector2 inTangent = Vector2.left * 80f;
        Vector2 outTangent = Vector2.right * 80f;
        public void Draw()
        {
            Vector2 i = inPoint.rect.center;
            Vector2 o = outPoint.rect.center;
            inTangent = Vector2.left * 80f;
            outTangent = Vector2.right * 80f;

            if(i.x <= o.x)
            {
                if(i.y < o.y)
                {
                    inTangent += Vector2.up * 40f;
                    outTangent += Vector2.up * 100f;
                }
                else
                {
                    inTangent += Vector2.down * 40f;
                    outTangent += Vector2.down * 100f;
                }
            }

            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + inTangent,
                outPoint.rect.center + outTangent,
                Color.white,
                null,
                2f
                );
        }

        public bool Removed()
        {
            return Handles.Button((inPoint.rect.center + outPoint.rect.center + inTangent + outTangent) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap);
        }

        public class Point
        {
            public enum Type { IN, OUT }

            public Rect rect;
            public Type type;
            public NodeWindow masterNode;
            private GUIStyle style;
            private Action<Connection.Point> onClickConnectionPoint;

            public Point(NodeWindow masterNode, Point.Type type, GUIStyle style, Action<Connection.Point> onClick)
            {
                this.masterNode = masterNode;
                this.type = type;
                this.style = style;
                this.onClickConnectionPoint = onClick;
                rect = new Rect(0, 0, 10f, 20f);
            }

            public void Draw()
            {
                rect.y = masterNode.rect.y + (masterNode.rect.height * 0.5f) - this.rect.height * 0.5f;

                switch(type)
                {
                    case Type.IN:
                        rect.x = masterNode.rect.x - this.rect.width + 8f;
                        break;
                    case Type.OUT:
                        rect.x = masterNode.rect.x + masterNode.rect.width - 8f;
                        break;
                }

                if(GUI.Button(this.rect, "", style))
                {
                    if(onClickConnectionPoint != null)
                    {
                        onClickConnectionPoint(this);
                    }
                }
            }
        }
    }
}
