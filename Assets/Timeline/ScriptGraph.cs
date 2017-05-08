using Assets.Core;
using Assets.Timeline.SubWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline
{
    class ScriptGraph : EditorWindow
    {
        int selectorIndex;
        List<NodeWindow> nodes = new List<NodeWindow>();
        List<Connection> connections = new List<Connection>();
        GUIStyle nodeStyle;
        GUIStyle inPointStyle;
        GUIStyle outPointStyle;
        NodeWindow dragged;

        Vector2 offset;
        Vector2 drag;

        Connection.Point selectedInPoint;
        Connection.Point selectedOutPoint;
        List<string> handlerNames;
        List<string> eventNames;
        
        [MenuItem ("Window/ScriptGraph")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ScriptGraph));
        }

        private void OnEnable()
        {
            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);

            var handlers = from t in Assembly.GetExecutingAssembly().GetTypes()
                                   where t.IsClass && t.BaseType==Type.GetType("Assets.Core.HandlerFuture") && t.Namespace == "Assets.Core.Handler"
                                   select t.Name;
            handlerNames = handlers.ToList();
            var events = from t in Assembly.GetExecutingAssembly().GetTypes()
                                   where t.IsClass && t.BaseType==Type.GetType("Assets.Core.EventPromise") && t.Namespace == "Assets.Core.Event"
                                   select t.Name;
            eventNames = events.ToList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            HandleSelector();
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            HandleNodes();
            HandleConnections();
            OnGUIEvent(Event.current);
            DrawConnectionPreview(Event.current);
            EditorGUILayout.EndVertical();

            if (GUI.changed) Repaint();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i=0; i< widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing *i, position.height, 0) + newOffset);
            }

            for (int i=0; i< heightDivs; i++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * i, 0) + newOffset, new Vector3(position.width, gridSpacing * i, 0) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
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

        void HandleSelector()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            IEnumerable<string> options = new List<String> { "Stub1" };

            int selected = EditorGUILayout.Popup(selectorIndex, options.ToArray(), GUILayout.Width(50f));
            if (selected != selectorIndex) //Changed

            {
                selectorIndex = selected;
                OnSelectorChange(selected);
            }
            EditorGUILayout.EndHorizontal();
        }
        void HandleNodes()
        {
            foreach (var node in nodes)
            {
                node.Draw();
            }
        }
        void HandleConnections()
        {
            foreach (var conn in connections)
            {
                conn.Draw();
            }
        }
        void HandleContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            foreach (var h in handlerNames)
            {
                genericMenu.AddItem(new GUIContent("Handlers/"+h), false, () => OnClickAddNode(mousePosition, h));
            }
            foreach (var e in eventNames)
            {
                genericMenu.AddItem(new GUIContent("Events/"+e), false, () => OnClickAddNode(mousePosition, e));
            }
            genericMenu.ShowAsContext();
        }

        private void OnGUIEvent(Event e)
        {
            drag = Vector2.zero;
            //mouse on a node
            foreach (var node in nodes)
            {
                GUI.changed = node.ProcessEvents(e);
            }

            foreach(var conn in connections)
            {
                GUI.changed = conn.OnGUIEvent(e);
            }

            if (e.keyCode == KeyCode.Return || e.type == EventType.MouseDown)
            {
                GUI.FocusControl(null);
                GUI.changed = true;
            }

            //mouse on a connection - Menu

            //mouse on empty space
            if (!nodes.Any(n => n.MouseOverlapped(e.mousePosition)) && 
            !connections.Any(c => c.connRect.Contains(e.mousePosition)))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if (e.button == 1) //right
                        {
                            HandleContextMenu(e.mousePosition);
                        }
                        break;
                    case EventType.MouseUp:
                        break;
                }
            }

            //drag
            switch(e.type)
            {
                case EventType.MouseDrag:
                    if(e.button == 2)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        private void OnDrag(Vector2 delta)
        {
            drag = delta;

            foreach (var node in nodes)
            {
                node.Drag(delta);
            }

            GUI.changed = true;
        }

        private void OnSelectorChange(int selected)
        {

        }
        private void OnClickAddNode(Vector2 mousePosition, string className)
        {
            nodes.Add(new NodeWindow(className, "New"+className, mousePosition, 250, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
        } 
        private NodeWindow SelectNode(Vector2 mousePosition)
        {
            var selected = from n in nodes
            where n.MouseOverlapped(mousePosition)
            select n;

            if (selected.Count() == 0) return null;
            else return selected.First();
        }
        private void OnClickInPoint(Connection.Point inPoint)
        {
            selectedInPoint = inPoint;

            if(selectedOutPoint != null)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
        }

        private void OnClickOutPoint(Connection.Point outPoint)
        {
            selectedOutPoint = outPoint;

            if(selectedInPoint != null)
            {
                CreateConnection();
                ClearConnectionSelection();
            }

        }

        private void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }
        private void OnClickRemoveNode(NodeWindow node)
        {
            nodes.Remove(node);
            connections = (from conn in connections
                           where conn.inPoint != node.inPoint && conn.outPoint != node.outPoint
                           select conn)
                           .ToList();
        }

        private void CreateConnection()
        {
            connections.Add(new SubWindows.Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection));
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }
    }
}
