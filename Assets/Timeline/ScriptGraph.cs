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
        GUIStyle nodeStyle;
        NodeWindow dragged;
        
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
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            HandleSelector();
            HandleNodes();
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
        void HandleContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("New Node"), false, () => OnClickAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void OnGUIEvent(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if(e.button == 1) //right
                    {
                        HandleContextMenu(e.mousePosition);
                    }
                    break;
                case EventType.MouseUp:
                    break;
            }
        } 

        private void OnSelectorChange(int selected)
        {

        }
        private void OnClickAddNode(Vector2 mousePosition)
        {
            nodes.Add(new NodeWindow(mousePosition, 200, 50, nodeStyle));
        } 
    }
}
