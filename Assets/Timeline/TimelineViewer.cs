using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Timeline.Data;
using Assets.Timeline;
using Assets.Timeline.Utility;

public class TimelineViewer : EditorWindow {
    int selectorIndex = -1;
    Vector2 scrollVector = new Vector2();
    ClipFetcher clipFetcher = new ClipFetcher();

    [MenuItem ("Window/TimelineViewer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TimelineViewer));
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(150f));
        HandleSelector(); //Dropdown selector for selecting clips to edit
        HandleChannels();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        HandleTimeGraduration();
        HandleNotes();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
    void HandleSelector()
    {
        if(selectorIndex == -1)
        {
            clipFetcher.Load();
            selectorIndex = 0;
        }
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        IEnumerable<string> options = from clip in clipFetcher.GetClips()
                                      select clip.ToString();

        int selected = EditorGUILayout.Popup(selectorIndex, options.ToArray());
        if (selected != selectorIndex) //Changed
        {
            selectorIndex = selected;
            OnSelectorChange(selected);
            clipFetcher.Load();
        }
        EditorGUILayout.EndHorizontal();
    }
    void HandleChannels()
    {
        scrollVector = EditorGUILayout.BeginScrollView(scrollVector, GUIStyle.none, GUIStyle.none);
        for (int i = 0; i < 20; i++)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(50f));
            GUILayout.Label("TEST"+i, GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
    void HandleTimeGraduration()
    {
        GUIStyle graduation = new GUIStyle(GUI.skin.box);
        graduation.alignment = TextAnchor.MiddleLeft;
        EditorGUILayout.LabelField(Graduation.GetGraduationString(0f, 5f, 0.5f), graduation, GUILayout.ExpandWidth(true), GUILayout.Height(22f));
    }
    void HandleNotes()
    {
        scrollVector = EditorGUILayout.BeginScrollView(scrollVector, false, true);
        for (int i = 0; i < 20; i++)
        {
            GUIStyle graduationStyle = new GUIStyle(GUI.skin.box);
            graduationStyle.wordWrap = false;
            graduationStyle.clipping = TextClipping.Clip;
            EditorGUILayout.BeginHorizontal(graduationStyle, GUILayout.ExpandWidth(true), GUILayout.Height(50f));
            GUILayout.Label("");
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    /** Callbacks */

    private void OnSelectorChange(int selected)
    {
        //Switch current editing target clip according to |selectorIndex|

    }
}
