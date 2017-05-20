using Assets.Core;
using Assets.Core.Tree;
using Assets.Timeline.SubWindows;
using Assets.Timeline.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Assets.Timeline
{
    public class ScriptGraph : EditorWindow
    {
        int selectorIndex;
        public List<NodeWindow> nodes = new List<NodeWindow>();
        public List<Connection> connections = new List<Connection>();
        GUIStyle nodeStyle;
        GUIStyle inPointStyle;
        GUIStyle outPointStyle;
        GUIStyle selectBoxStyle = null;
        NodeWindow dragged;

        Vector2 offset;
        Vector2 drag;

        Vector2 selectDragStart;
        bool selectDragOn;

        Connection.Point selectedInPoint;
        Connection.Point selectedOutPoint;
        List<string> handlerNames;
        List<string> eventNames;
        ScriptTree currTree;
        
        [MenuItem ("Window/ScriptGraph")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ScriptGraph));
        }

        string currScriptName = null;
        Dictionary<string, ScriptTree> scriptTrees;

        public void Load(ScriptTree tree, string name)
        {
            var p = GetSystem(tree.Nodes);
            nodes = p.Key;
            connections = p.Value;
        }

        private KeyValuePair<List<NodeWindow>, List<Connection>> GetSystem(List<ScriptTree.Node> rawNodes)
        {
            List<NodeWindow> nodes = new List<NodeWindow>();
            List<Connection> conns = new List<Connection>();
            foreach (var rn in rawNodes)
            {
                NodeWindow node;
                if (rn.Base == "")
                {
                    var p = GetSystem(rn.Members);
                    SubWindows.Group g = new SubWindows.Group(p.Key, p.Value, rn.Name, new Vector2((float)rn.Position.X, (float)rn.Position.Y), 250, inPointStyle, outPointStyle,
                        OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickGroup,
                        Events, OnClickDegroup, Save, GetNewName, SelectedNodes, SelectedConnections);
                    var start = g.nodes.Find(n => n.nodeName == rn.StartMember);
                    var end = g.nodes.Find(n => n.nodeName == rn.EndMember);
                    if (start != null)
                    {
                        var startConn = new Connection(start.inPoint, g.startPoint, null, null);
                        g.connections.Add(startConn);
                    }
                    if (end != null)
                    {
                        var endConn = new Connection(g.endPoint, end.outPoint, null, null);
                        g.connections.Add(endConn);
                    }
                    node = g;
                }
                else
                {
                    node = new NodeWindow(rn.Base, rn.Name, new Vector2((float)rn.Position.X, (float)rn.Position.Y),
                        250, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickGroup, GetNewName, SelectedNodes, SelectedConnections);
                    node.paramPairs = rn.Params.ToList();
                    node.nodeName = rn.Name;
                }
                nodes.Add(node);
            }

            foreach (var rn in rawNodes)
            {
                var startNode = nodes.Find(n => n.nodeName == rn.Name);
                Debug.Assert(startNode != null);
                if (rn.Succ != null)
                {
                    foreach (var succ in rn.Succ)
                    {
                        var destNode = nodes.Find(n => n.nodeName == succ.Dest);
                        Debug.Assert(destNode != null);

                        var conn = new Connection(destNode.inPoint, startNode.outPoint, OnClickRemoveConnection, Events);
                        foreach (var cond in succ.Condition)
                        {
                            conn.conditions.Add(nodes.Find(n => n.nodeName == cond));
                        }
                        conn.stopPrev = succ.EndPrev;
                        conns.Add(conn);
                    }
                }
            }
            return new KeyValuePair<List<NodeWindow>, List<Connection>>(nodes, conns);
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
            if(scriptTrees == null || currScriptName == null && scriptTrees.Count != 0)
            {
                scriptTrees = Meta.ScriptTrees;
                OnSelectorChange(0);
            }
            if(selectBoxStyle == null)
                selectBoxStyle = (GUIStyle)"TL SelectionButton PreDropGlow";
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
            EditorGUILayout.BeginHorizontal();
            HandleSelector();
            HandleAdd();
            EditorGUILayout.EndHorizontal();
            if (currScriptName != null)
            {
                DrawGrid(20, 0.2f, Color.gray);
                DrawGrid(100, 0.4f, Color.gray);
                HandleNodes();
                HandleConnections();
                OnGUIEvent(Event.current);
                if (selectDragOn)
                    SelectDrag(Event.current);
                DrawConnectionPreview(Event.current);
            }
            EditorGUILayout.EndVertical();

            Repaint();
        }

        private void HandleAdd()
        {
            if(GUILayout.Button("Add New", GUILayout.MaxWidth(200f)))
            {

            }
            if(GUILayout.Button("Remove", GUILayout.MaxWidth(200f)))
            {

            }
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
        private void SelectDrag(Event e)
        {
            float posX = Math.Min(e.mousePosition.x, selectDragStart.x);
            float posY = Math.Min(e.mousePosition.y, selectDragStart.y);
            float width = Math.Abs(e.mousePosition.x - selectDragStart.x);
            float height = Math.Abs(e.mousePosition.y - selectDragStart.y);

            Rect selectArea = new Rect(posX, posY, width, height);

            foreach (var node in nodes)
            {
                if (selectArea.Contains(new Vector2(node.rect.xMin, node.rect.yMin)) && 
                    selectArea.Contains(new Vector2(node.rect.xMax, node.rect.yMax)))
                {
                    node.Select(true);
                }
                else
                {
                    node.Select(false);
                }
            }
            GUI.Box(selectArea, new GUIContent(), selectBoxStyle);
            GUI.changed = true;
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
            IEnumerable<string> options = scriptTrees.Keys;

            int selected = EditorGUILayout.Popup(selectorIndex, options.ToArray(), GUILayout.MinWidth(200f));
            if (selected != selectorIndex) //Changed
            {
                selectorIndex = selected;
                OnSelectorChange(selected);
            }
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

            if (e.type == EventType.MouseDown && !nodes.Any(n => n.MouseOverlapped(e.mousePosition) && n.selected))
            {
                foreach (var n in nodes)
                {
                    if (!n.MouseOverlapped(e.mousePosition))
                        n.Select(false);
                    GUI.changed = true;
                }
            }

            foreach (var node in nodes)
            {
                GUI.changed = node.ProcessEvents(e);
            }

            foreach(var conn in connections)
            {
                GUI.changed = conn.OnGUIEvent(e);
            }

            if(e.type == EventType.MouseDown)
            {
                ClearConnectionSelection();
            }

            if (e.keyCode == KeyCode.Return || e.type == EventType.MouseDown)
            {
                GUI.FocusControl(null);
                GUI.changed = true;
            }

            if(!selectDragOn)
            {
                switch (e.type)
                {
                    case EventType.MouseDrag:
                        foreach (var n in nodes)
                        {
                            if (n.selected)
                            {
                                n.Drag(e.delta);
                                GUI.changed = true;
                            }
                        }
                        break;
                }
            }

            //mouse on empty space
            if (!nodes.Any(n => n.MouseOverlapped(e.mousePosition)) && 
            !connections.Any(c => c.connRect.Contains(e.mousePosition)))
            {
                switch (e.type)
                {
                    case EventType.MouseDown:
                        if(e.button == 0)
                        {
                            selectDragOn = true;
                            selectDragStart = e.mousePosition;
                        }
                        if (e.button == 1) //right
                        {
                            HandleContextMenu(e.mousePosition);
                        }
                        break;
                    case EventType.MouseUp:
                        selectDragOn = false;
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

            if((e.type == EventType.MouseUp || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Tab) && e.button != 2)
            {
                Save();
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
            Load(scriptTrees.ToList()[selected].Value, scriptTrees.ToList()[selected].Key);
            currScriptName = scriptTrees.ToList()[selected].Key;
            currTree = scriptTrees.ToList()[selected].Value;
        }
        private void OnClickAddNode(Vector2 mousePosition, string className)
        {
            string initialName = GetNewName(className, null);
            nodes.Add(new NodeWindow(className, initialName, mousePosition, 250, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickGroup, GetNewName, SelectedNodes, SelectedConnections));
            Save();
        }
        protected void Save()
        {
            ScriptTree tree = ScriptTree.Construct(this);
            Meta.SaveScripts(currScriptName, tree);
        }

        private List<NodeWindow> SelectedNodes()
        {
            return (from n in nodes
                    where n.selected
                    select n).ToList();
        }

        private List<Connection> SelectedConnections()
        {
            var snodes = SelectedNodes();
            return (from c in connections
                    where snodes.Any(n => n.inPoint == c.inPoint) && snodes.Any(n => n.outPoint == c.outPoint)
                    select c).ToList();
        }

        private string GetNewName(string baseName, NodeWindow exceptNode)
        {
            string namePattern = @"^" + baseName + @"(?:\s\(\d+\))*\s\((\d+)\)$";
            Regex regex = new Regex(namePattern);
            string initialName = baseName;
            int index = 0;
            var subPool = from node in nodes
                          where node.isWorkspace
                          from subnode in ((SubWindows.Group)node).nodes
                          select subnode;
            foreach (var name in nodes.Concat(subPool).Where(n=>n!=exceptNode).Select(n=>n.nodeName))
            {
                if (name == baseName)
                {
                    index = 1;
                }
                else
                {
                    var mc = regex.Matches(name);
                    int i = 0;
                    foreach (Match m in mc)
                    {
                        i = int.Parse(m.Groups[1].Value);
                    }
                    if(i != 0)
                        index = Math.Max(index, i + 1);
                }
            }
            if(index != 0)
                initialName = baseName + " (" + index + ")";
            return initialName;
        }

        private NodeWindow SelectNode(Vector2 mousePosition)
        {
            var selected = from n in nodes
            where n.MouseOverlapped(mousePosition)
            select n;

            if (selected.Count() == 0) return null;
            else return selected.First();
        }
        private void OnClickGroup()
        {
            var kidnappedNodes = nodes.Where(n => n.selected);
            var kidnappedConnections = connections.Where(c => kidnappedNodes.Contains(c.inPoint.masterNode) && kidnappedNodes.Contains(c.outPoint.masterNode));

            nodes.Add(new SubWindows.Group(kidnappedNodes.ToList(), kidnappedConnections.ToList(), GetNewName("Group", null), new Vector2(0, 0), 200f, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickGroup, Events, OnClickDegroup, Save, GetNewName, SelectedNodes, SelectedConnections));
            OnClickRemoveNode(null);
            Save();
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

        private List<NodeWindow> Events()
        {
            var events =from node in nodes
                        where node.isEvent
                        select node;
            return events.ToList();
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
            Save();
        }
        private void OnClickRemoveNode(NodeWindow node)
        {
            var removedNodes = from n in nodes where n.selected select n;
            foreach (var n in removedNodes)
            {
                connections = (from conn in connections
                               where conn.inPoint != n.inPoint && conn.outPoint != n.outPoint
                               select conn)
                               .ToList();
                foreach (var conn in connections)
                {
                    conn.conditions = (from evt in conn.conditions
                                       where evt != n
                                       select evt)
                                       .ToList();
                }
            }
            nodes = (from n in nodes where !n.selected select n).ToList();
            Save();
        }
        private void OnClickDegroup(SubWindows.Group g)
        {
            foreach (var n in g.nodes)
            {
                n.inPoint.onClickConnectionPoint = OnClickInPoint;
                n.outPoint.onClickConnectionPoint = OnClickOutPoint;
                n.ResetStyle();
            }
            foreach (var c in g.InterConnections)
            {
                c.onClickRemoveConnection = OnClickRemoveConnection;
            }
            nodes.AddRange(g.nodes);
            connections.AddRange(g.InterConnections);
            nodes.Remove(g);
            connections = (from c in connections
                           where c.inPoint.masterNode != g && c.outPoint.masterNode != g
                           select c).ToList();
            Save();
        }

        private void CreateConnection()
        {
            if(!connections.Any(c => c.inPoint == selectedInPoint && c.outPoint == selectedOutPoint))
                connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection, Events));
            Save();
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }
    }
}
