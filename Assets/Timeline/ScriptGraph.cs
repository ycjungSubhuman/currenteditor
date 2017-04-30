using Assets.Timeline.SubWindows;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Connection.Point selectedInPoint;
        Connection.Point selectedOutPoint;
        
        [MenuItem ("Window/ScriptGraph")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ScriptGraph));
        }

        private void OnEnable()
        {
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = 
                EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            nodeStyle.border = new RectOffset(12, 12, 12, 12);

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            HandleSelector();
            HandleNodes();
            HandleConnections();
            EditorGUILayout.EndVertical();

            if (GUI.changed) Repaint();
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
            OnGUIEvent(Event.current);
        }
        void HandleConnections()
        {
            foreach (var conn in connections)
            {
                conn.Draw();
            }

            connections = (from conn in connections
                          where !conn.Removed()
                          select conn).ToList();
        }
        void HandleContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("New Node"), false, () => OnClickAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void OnGUIEvent(Event e)
        {
            //mouse on a node
            foreach (var node in nodes)
            {
                GUI.changed = node.ProcessEvents(e);
            }

            //mouse on a node - Menu

            //mouse on empty space
            if (!nodes.Any(n => n.MouseOverlapped(e.mousePosition)))
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
        } 

        private void OnSelectorChange(int selected)
        {

        }
        private void OnClickAddNode(Vector2 mousePosition)
        {
            nodes.Add(new NodeWindow("MoveConstant", mousePosition, 200, 50, nodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint));
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
