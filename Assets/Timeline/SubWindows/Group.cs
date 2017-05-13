﻿using System;
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
                n.inPoint.style = new GUIStyle();
                n.outPoint.onClickConnectionPoint = null;
                n.outPoint.style = new GUIStyle();
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

        public void OnClickRemoveConnection(Connection c)
        {
            connections.Remove(c);
            save();
        }
        protected override void AddMenuItems(GenericMenu menu)
        {
            base.AddMenuItems(menu);
            menu.AddItem(new GUIContent("Degroup"), false, ()=>onClickDegroup(this));
        }

        private void DrawConnectionPreview(Event e)
        {

            Vector2 i;
            Vector2 o;
            Vector2 inTangent = Vector2.left * 80f;
            Vector2 outTangent = Vector2.right * 80f;

            if (selectedInPoint != null && selectedOutPoint == null)
            {
                i = selectedInPoint.rect.center;
                o = e.mousePosition;
            }
            else if (selectedInPoint == null && selectedOutPoint != null)
            {
                i = e.mousePosition;
                o = selectedOutPoint.rect.center;
            }
            else return;

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
                i,
                o,
                i + inTangent,
                o + outTangent,
                Color.white,
                null,
                2f
                );
            GUI.changed = true;
        }

        private void OnClickStartPoint(Connection.Point startPoint)
        {
            var duplicates = from c in connections
                             where c.outPoint == startPoint
                             select c;
            bool changed = false;
            foreach (var d in duplicates)
            {
                if (selectedInPoint != null)
                {
                    d.inPoint = selectedInPoint;
                    ClearConnectionSelection();
                    changed = true;
                }
            }

            if(!changed)
                OnClickOutPoint(startPoint);
        }

        private void OnClickEndPoint(Connection.Point endPoint)
        {
            var duplicates = from c in connections
                             where c.inPoint == endPoint
                             select c;
            bool changed = false;
            foreach (var d in duplicates)
            {
                if(selectedOutPoint != null)
                {
                    d.outPoint = selectedOutPoint;
                    ClearConnectionSelection();
                    changed = true;
                }
            }

            if(!changed)
                OnClickInPoint(endPoint);
        }

        private void OnClickInPoint(Connection.Point inPoint)
        {
            selectedInPoint = inPoint;

            if(selectedOutPoint != null)
            {
                if (selectedInPoint == endPoint || selectedOutPoint == startPoint)
                    CreateGroupInterfaceConnection();
                else
                    CreateConnection();
                ClearConnectionSelection();
            }
        }

        private void OnClickOutPoint(Connection.Point outPoint)
        {
            selectedOutPoint = outPoint;

            if(selectedInPoint != null)
            {
                if (selectedInPoint == endPoint || selectedOutPoint == startPoint)
                    CreateGroupInterfaceConnection();
                else
                    CreateConnection();
                ClearConnectionSelection();
            }
        }

        private void CreateConnection()
        {
            if(!connections.Any(c => c.inPoint == selectedInPoint && c.outPoint == selectedOutPoint))
                connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection, () => new List<NodeWindow>(){ }));
            save();
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private void CreateGroupInterfaceConnection()
        {
            if (!connections.Any(c => c.inPoint == selectedInPoint && c.outPoint == selectedOutPoint))

            {
                var dupStart = (from c in connections
                                where c.outPoint == startPoint && c.inPoint == selectedInPoint
                                select c).SingleOrDefault();
                var dupEnd = (from c in connections
                              where c.inPoint == endPoint && c.outPoint == selectedOutPoint
                              select c).SingleOrDefault();
                if (dupStart == null && dupEnd == null)
                    connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection, getEvents));
                else if (dupEnd == null)
                    dupStart.inPoint = selectedInPoint;
                else
                    dupEnd.outPoint = selectedOutPoint;
            }
            save();
        }

        public override void Draw()
        {
            base.Draw();
            startPoint.Draw();
            endPoint.Draw();
            DrawConnectionPreview(Event.current);
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
            if (e.type == EventType.MouseDown)
            {
                ClearConnectionSelection();
            }
            return a;
        }
    }
}
