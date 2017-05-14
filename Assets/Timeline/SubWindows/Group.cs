using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline.SubWindows
{
    class Group : NodeWindow
    {
        const float initialPaddingX = 30f;
        const float initialPaddingY = 20f;

        Connection.Point selectedInPoint = null;
        Connection.Point selectedOutPoint = null;

        public Connection.Point startPoint;
        public Connection.Point endPoint;
        public List<NodeWindow> nodes;
        public List<Connection> connections;
        Action<Group> onClickDegroup;
        Action save;
        public List<Connection> InterConnections {
            get
            {
                return connections.Where(c => c.inPoint != endPoint && c.outPoint != startPoint).ToList();
            }
        }
        Func<List<NodeWindow>> getEvents;
        Func<string, NodeWindow, string> getNewName;
        public Group(List<NodeWindow> initialNodes, List<Connection> initialConnections, string initialName, Vector2 position, float width, GUIStyle inPointStyle, GUIStyle outPointStyle, 
            Action<Connection.Point> onClickInPoint, Action<Connection.Point> onClickOutPoint, Action<NodeWindow> onClickRemove, Action onClickGroup, Func<List<NodeWindow>> getEvents, Action<Group> onClickDegroup, Action save, Func<string, NodeWindow, string> getNewName, Func<List<NodeWindow>> getSelectedNodes, Func<List<Connection>> getSelectedConnections)
            :base("", initialName, position, 200f, inPointStyle, outPointStyle, onClickInPoint, onClickOutPoint, onClickRemove, onClickGroup, getNewName, getSelectedNodes, getSelectedConnections)
        {
            this.nodes = initialNodes;
            this.connections = initialConnections;
            this.getNewName = getNewName;
            this.save = save;
            this.isWorkspace = true;
            this.getEvents = getEvents;
            this.onClickDegroup = onClickDegroup;
            foreach(var c in connections)
            {
                c.onClickRemoveConnection = null;
            }
            foreach(var n in nodes)
            {
                n.inPoint.onClickConnectionPoint = null;
                n.outPoint.onClickConnectionPoint = null;
                n.style = (GUIStyle)"VCS_StickyNote";
                n.selectedStyle = (GUIStyle)"VCS_StickyNote";
            }
           if(initialName.Count() != 0)
            {
                float minX = initialNodes.Aggregate(float.MaxValue, (acc, n) => Math.Min(acc, n.rect.xMin));
                float minY = initialNodes.Aggregate(float.MaxValue, (acc, n) => Math.Min(acc, n.rect.yMin));
                float maxX = initialNodes.Aggregate(float.MinValue, (acc, n) => Math.Max(acc, n.rect.xMax));
                float maxY = initialNodes.Aggregate(float.MinValue, (acc, n) => Math.Max(acc, n.rect.yMax));
                float w = Math.Abs(minX - maxX);
                float h = Math.Abs(minY - maxY);
                startPoint = new Connection.GroupPoint(this, Connection.Point.Type.OUT, new GUIStyle(), (_)=> { });
                endPoint = new Connection.GroupPoint(this, Connection.Point.Type.IN, new GUIStyle(), (_)=> { });

                rect = new Rect(minX - initialPaddingX, minY - initialPaddingY, w+2*initialPaddingX, h+2*initialPaddingY);
            }
            var emptyStartPoint = nodes.Find(n => !connections.Any(c => c.inPoint == n.inPoint)).inPoint;
            var emptyEndPoint = nodes.Find(n => !connections.Any(c => c.outPoint == n.outPoint)).outPoint;
            connections.Add(new Connection(emptyStartPoint, startPoint, null, null));
            connections.Add(new Connection(endPoint, emptyEndPoint, null, null));
            Debug.Log(connections.Count);
        }

        protected override void AddMenuItems(GenericMenu menu)
        {
            base.AddMenuItems(menu);
            menu.AddItem(new GUIContent("Degroup"), false, ()=>onClickDegroup(this));
        }

        public override void Draw()
        {
            base.Draw();
            startPoint.Draw();
            endPoint.Draw();
            GUI.changed = true;
        }

        public override void Drag(Vector2 delta)
        {
            base.Drag(delta);
            foreach (var n in nodes)
            {
                n.Drag(delta);
            }
        }

        protected override void DrawTitle()
        {
        }

        protected override void DrawName()
        {
            editNodeName = GUI.TextField(new Rect(rect.x + 5f, rect.y-20f, 100f, 18f), editNodeName);
        }

        protected override void DrawParams()
        {
            foreach (var n in nodes)
            {
                n.Draw();
                n.ProcessEvents(Event.current);
            }
            foreach (var c in connections)
            {
                c.Draw();
                c.OnGUIEvent(Event.current);
            }
        }

        public override bool ProcessEvents(Event e)
        {
            bool a = base.ProcessEvents(e);
            foreach (var c in connections)
            {
                c.OnGUIEvent(e);
            }
            return a;
        }
    }
}
