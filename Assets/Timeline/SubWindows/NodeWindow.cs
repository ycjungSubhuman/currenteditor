﻿using Assets.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
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
        public GUIStyle selectedStyle;
        public string baseClassName;
        public string nodeName;
        public string editNodeName;
        const float paramMarginX = 6f;
        const float paramMarginY = 3f;
        const float nameMargin = 30f;
        public bool isEvent;
        public bool isWorkspace;
        public bool selected = false;
        public int linkDialogIndex = 0;
        public bool linkDialogOn;
        public Rect linkDialogRect;
        Action<NodeWindow> onClickRemove;
        Action onClickGroup;
        Func<string, NodeWindow, string> getNewName;
        Func<List<NodeWindow>> getSelectedNodes;
        Func<List<Connection>> getSelectedConnections;
        GUIStyle inPointStyle;
        GUIStyle outPointStyle;

        public NodeWindow(String baseClassName, string initialName, Vector2 position, float width, 
            GUIStyle inPointStyle, 
            GUIStyle outPointStyle, Action<Connection.Point> onClickInPoint, Action<Connection.Point> onClickOutPoint, Action<NodeWindow> onClickRemove, Action onClickGroup, Func<string, NodeWindow, string> getNewName, Func<List<NodeWindow>> getSelectedNodes, Func<List<Connection>> getSelectedConnections)
        {
            Type t = null;
            this.baseClassName = baseClassName;
            this.onClickRemove = onClickRemove;
            this.onClickGroup = onClickGroup;
            this.getNewName = getNewName;
            this.getSelectedNodes = getSelectedNodes;
            this.getSelectedConnections = getSelectedConnections;
            this.inPointStyle = inPointStyle;
            this.outPointStyle = outPointStyle;
            nodeName = initialName;
            editNodeName = nodeName;
            if (baseClassName.Contains("Event"))
            {
                t = Type.GetType("Assets.Core.Event." + baseClassName);
                isEvent = true;
            }
            else if (baseClassName=="")
            {
                style = (GUIStyle)"flow node 0";
                isWorkspace = true;
            }
            else //Handler
            {
                t = Type.GetType("Assets.Core.Handler." + baseClassName);
                isEvent = false;
            }
            ResetStyle();

            if (t != null)
            {
                IDefaultParamProvider p = Activator.CreateInstance(t) as IDefaultParamProvider;
                foreach (var pair in p.GetDefaultParams())
                {
                    paramPairs.Add(pair);
                }
            }

            style.border = new RectOffset(12, 12, 12, 12);
            rect = new Rect(position.x, position.y, width, 40f + paramPairs.Count*20f);
            inPoint = new Connection.Point(this, Connection.Point.Type.IN, inPointStyle, onClickInPoint);
            outPoint = new Connection.Point(this, Connection.Point.Type.OUT, outPointStyle, onClickOutPoint);
        }
        public void ResetStyle()
        {
            if (baseClassName.Contains("Event"))
            {
                style = (GUIStyle)"flow node 2";
                selectedStyle = (GUIStyle)"flow node 2 on";
            }
            else if (baseClassName=="")
            {
                style = (GUIStyle)"flow node 0";
                selectedStyle = (GUIStyle)"flow node 0 on";
            }
            else //Handler
            {
                style = (GUIStyle)"flow node 1";
                selectedStyle = (GUIStyle)"flow node 1 on";
            }
            if (inPoint != null && outPoint != null)
            {
                inPoint.style = inPointStyle;
                outPoint.style = outPointStyle;
            }
        }

        public virtual void Drag(Vector2 delta)
        {
            rect.position += delta;
        }

        public virtual void Draw()
        {
            if(!isEvent)
                inPoint.Draw();
            outPoint.Draw();
            if (selected)
                GUI.Box(rect, "", selectedStyle);
            else
                GUI.Box(rect, "", style);
            DrawTitle();
            DrawName();
            DrawParams();
            if(linkDialogOn)
                DrawLinkDialog();
        }

        protected virtual void DrawName()
        {
            editNodeName = GUI.TextField(new Rect(rect.x + nameMargin, rect.y+20f, rect.width - 2*nameMargin, 18f), editNodeName);
        }

        protected virtual void DrawTitle()
        {
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 18f), new GUIContent(baseClassName), titleStyle);
        }

        const float buttonWidth = 30f;
        protected virtual void DrawParams()
        {
            for(int i=0; i<paramPairs.Count; i++)
            {
                string name = paramPairs[i].Key;
                string value = paramPairs[i].Value;
                string newValue = value;
                Rect labelRect = new Rect(rect.x + paramMarginX, rect.y + 40f + i * 20f, rect.width / 3 - 2 * paramMarginX, 16f);
                Rect textRect = new Rect(rect.x + paramMarginX + rect.width / 3, rect.y + 40f + i * 20f, rect.width *2 / 3 - 2 * paramMarginX, 16f);
                GUI.Label(labelRect, new GUIContent(name));
                newValue = GUI.TextField(textRect, newValue);
                /*if(GUI.Button(new Rect(textRect.x + textRect.width, textRect.y, buttonWidth, 18f), new GUIContent("Lnk")))
                {
                    linkDialogIndex = i;
                    linkDialogOn = true;
                    linkDialogRect = new Rect(textRect.x + textRect.width, textRect.y, 200f, 300f);
                }*/
                paramPairs[i] = new KeyValuePair<string, string>(name, newValue);
            }
        }
        private void DrawLinkDialog()
        {
            GUI.Box(linkDialogRect, new GUIContent());
        }

        public void Select(bool selected)
        {
            this.selected = selected;
        }

        protected void SaveName()
        {
            this.nodeName = getNewName(editNodeName, this);
            editNodeName = nodeName;
        }

        public virtual bool ProcessEvents(Event e)
        {
            switch(e.type)
            {
                case EventType.MouseDown:
                    if(e.button == 0 && MouseOverlapped(e.mousePosition))
                    {
                        GUI.changed = true;
                        dragged = true;
                        selected = true;
                    }
                    if(e.button == 1 && MouseOverlapped(e.mousePosition))
                    {
                        GUI.changed = true;
                        selected = true;
                        GenericMenu menu = new GenericMenu();
                        AddMenuItems(menu);
                        menu.ShowAsContext();
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
            if(e.type == EventType.MouseDown || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Tab)
            {
                SaveName();
            }

            return false;
        }
        private bool IsSelectedNodesLinear()
        {
            var snodes = getSelectedNodes();
            var sconns = getSelectedConnections();
            return snodes.All(n => !n.isEvent) &&
                sconns.All(c => c.conditions.Count == 0) &&
                (sconns.Count == snodes.Count() - 1) &&
                (new HashSet<Connection.Point>(sconns.Select(c => c.inPoint)).Count == sconns.Count) &&
                (new HashSet<Connection.Point>(sconns.Select(c => c.outPoint)).Count == sconns.Count);

        }
        protected virtual void AddMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Delete Node?/Delete"), false, () => OnClickDelete());
            if (IsSelectedNodesLinear() && getSelectedNodes().Count > 1)
            {
                menu.AddItem(new GUIContent("Make Group"), false, () => onClickGroup());
            }
            else if(getSelectedNodes().Count > 1)
            {
                menu.AddDisabledItem(new GUIContent("Make Group(Select linear, uncut handlers without any conditions)"));
            }
        }

        private void OnClickDelete()
        {
            onClickRemove(this);
        }
        public bool MouseOverlapped(Vector2 mousePosition)
        {
            return rect.Contains(mousePosition);
        }
    }
}
