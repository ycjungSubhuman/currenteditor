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
        public Rect connRect;
        public bool stopPrev = false;
        private Func<List<NodeWindow>> getEvents;
        public List<NodeWindow> conditions = new List<NodeWindow>();

        public Connection(Connection.Point inPoint, Connection.Point outPoint, Action<Connection> onClickRemoveConnection, Func<List<NodeWindow>> getEvents)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            this.onClickRemoveConnection = onClickRemoveConnection;
            this.getEvents = getEvents;
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
                    inTangent += Vector2.up * 200f;
                    outTangent += Vector2.up * 200f;
                }
                else
                {
                    inTangent += Vector2.down * 200f;
                    outTangent += Vector2.down * 200f;
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


            for(int j=0; j<conditions.Count; j++)
            {
                NodeWindow evt = conditions[j];
                float condWidth = GUI.skin.label.CalcSize(new GUIContent(evt.nodeName)).x + 10f;
                Vector2 condPos = (i + o + inTangent + outTangent - new Vector2(condWidth, condHeight)) * 0.5f;
                GUI.Label(new Rect(condPos.x, condPos.y+30f+j*(condHeight+2f), condWidth, condHeight), new GUIContent(evt.nodeName), (GUIStyle)"AssetLabel");
            }

            Vector2 wh = new Vector2(btnWidth, btnHeight);
            connRect = new Rect((i + o + inTangent + outTangent - wh * 0.5f) * 0.5f, wh);
            if(onClickRemoveConnection != null && getEvents != null)
                GUI.Box(connRect, new GUIContent(), (GUIStyle)"flow var 4");

            if(stopPrev)
                GUI.Box(new Rect(connRect.x, connRect.y - 20f, 200f, connRect.height), new GUIContent("Stop"), (GUIStyle)"OL Minus");
        }
        const float btnWidth = 20f;
        const float btnHeight = 20f;
        const float condHeight = 20f;

        public bool OnGUIEvent(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if(connRect.Contains(e.mousePosition) && onClickRemoveConnection != null && getEvents != null)
                    {
                        HandleContextMenu();
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void HandleContextMenu()
        {
            GenericMenu menu = new GenericMenu();
            if (outPoint.masterNode != null && !outPoint.masterNode.isEvent)
            {
                menu.AddItem(new GUIContent("Stop Previous On Transition"), stopPrev, () => stopPrev = !stopPrev);
                foreach (var evt in getEvents())
                {
                    menu.AddItem(new GUIContent("Add(Remove) Condition/" + evt.nodeName), conditions.Contains(evt),
                        () =>
                        {
                            if (conditions.Contains(evt))
                                conditions.Remove(evt);
                            else
                                conditions.Add(evt);
                        });
                }
                menu.AddSeparator("");
            }
            menu.AddItem(new GUIContent("Delete?/Delete"), false, () => onClickRemoveConnection(this));
            menu.ShowAsContext();
        }

        public class Point
        {
            public enum Type { IN, OUT }

            public Rect rect;
            public Type type;
            public NodeWindow masterNode;
            public GUIStyle style;
            public Action<Connection.Point> onClickConnectionPoint;

            public Point(NodeWindow masterNode, Point.Type type, GUIStyle style, Action<Connection.Point> onClick)
            {
                this.masterNode = masterNode;
                this.type = type;
                this.style = style;
                this.onClickConnectionPoint = onClick;
                rect = new Rect(0, 0, 20f, 20f);
            }

            public virtual void Draw()
            {
                rect.y = masterNode.rect.y + (masterNode.rect.height * 0.5f) - this.rect.height * 0.5f;

                switch(type)
                {
                    case Type.IN:
                        rect.x = masterNode.rect.x - this.rect.width + 0f;
                        break;
                    case Type.OUT:
                        rect.x = masterNode.rect.x + masterNode.rect.width - 0f;
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

        public class GroupPoint : Point
        {
            NodeWindow groupNode;
            public GroupPoint(NodeWindow groupNode, Type type, GUIStyle style, Action<Point> onClick) : base(null, type, style, onClick)
            {
                this.groupNode = groupNode;
            }

            public override void Draw()
            {
                rect.y = groupNode.rect.y + (groupNode.rect.height * 0.5f) - this.rect.height * 0.5f;

                switch (type)
                {
                    case Type.IN:
                        rect.x = groupNode.rect.x + groupNode.rect.width - this.rect.width;
                        break;
                    case Type.OUT:
                        rect.x = groupNode.rect.x;
                        break;
                }

                if (GUI.Button(this.rect, "", style))
                {
                    if (onClickConnectionPoint != null)
                    {
                        onClickConnectionPoint(this);
                    }
                }
            }
        }
    }
}
