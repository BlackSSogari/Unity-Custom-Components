using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public class ComponentEditorUtils 
{
    public static Component clipboard;

    private static Texture2D _dummyTexture = null;
    public static Texture2D DummyTexture { get { if (_dummyTexture == null) _dummyTexture = CreateTexture("dummyTexture", 1, 1, Color.white); return _dummyTexture; } }

    public static Texture2D CreateTexture(string name, int w, int h, Color col)
    {
        Texture2D t = new Texture2D(w, h);
        t.hideFlags = HideFlags.DontSave;
        t.filterMode = FilterMode.Point;
        t.name = name;
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++) t.SetPixel(i, j, col);
        }
        t.Apply();
        return t;
    }
    
    public static void DrawSpacer()
    {
        EditorGUILayout.Space();
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = DummyTexture;
            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(1f, 1f, 1f, 0.1f);
            GUI.DrawTexture(new Rect(0f, rect.yMax - 2f, Screen.width, 1f), tex);
            GUI.color = Color.white;
        }
    }

    public static void MarkUndo(UnityEngine.Object target, string UndoMessage)
    {
        //Undo.RegisterSceneUndo(UndoMessage);
        Undo.RegisterCreatedObjectUndo(target, UndoMessage);

        EditorUtility.SetDirty(target);
    }

    public static string StringField(string label, string value)
    {        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField(label, "", GUILayout.Width(ComponentEditorUtils.LabelWidth - 10));

            GUILayout.Space(5);

            var displayText = value == null ? "[none]" : value;
            GUILayout.Label(displayText, "TextField", GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));            
            //GUILayout.TextField(displayText, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));            
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(3);

        return value;
    }

    public static void ReadOnlyTextField(string label, string text, float space = 0)
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (space != 0)
                GUILayout.Space(space);
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
            EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }
        EditorGUILayout.EndHorizontal();
    }

    public static Component ComponentField(string label, Component value)
    {
        return ComponentField(label, value, null);
    }

    public static Component ComponentField(string label, Component value, Type componentType)
    {

        componentType = componentType ?? typeof(MonoBehaviour);

        EditorGUILayout.BeginHorizontal();
        {

            EditorGUILayout.LabelField(label, "", GUILayout.Width(ComponentEditorUtils.LabelWidth - 10));

            GUILayout.Space(5);

            var displayText = value == null ? "[none]" : value.ToString();
            GUILayout.Label(displayText, "TextField", GUILayout.ExpandWidth(true), GUILayout.MinWidth(100));

            var evt = Event.current;
            if (evt != null)
            {
                var textRect = GUILayoutUtility.GetLastRect();
                if (evt.type == EventType.mouseDown && evt.clickCount == 2)
                {
                    if (textRect.Contains(evt.mousePosition))
                    {
                        if (GUI.enabled && value != null)
                        {
                            Selection.activeObject = value;
                            EditorGUIUtility.PingObject(value);
                            GUIUtility.hotControl = value.GetInstanceID();
                        }
                    }
                }
                else if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                {
                    if (textRect.Contains(evt.mousePosition))
                    {

                        var reference = DragAndDrop.objectReferences.First();
                        var draggedComponent = (Component)null;
                        if (reference is Transform)
                        {
                            draggedComponent = (Transform)reference;
                        }
                        else if (reference is GameObject)
                        {
                            draggedComponent =
                                ((GameObject)reference)
                                .GetComponents(componentType)
                                .FirstOrDefault();
                        }
                        else if (reference is Component)
                        {
                            draggedComponent = reference as Component;
                            if (draggedComponent == null)
                            {
                                draggedComponent =
                                    ((Component)reference)
                                    .GetComponents(componentType)
                                    .FirstOrDefault();
                            }
                        }

                        DragAndDrop.visualMode = (draggedComponent == null) ? DragAndDropVisualMode.None : DragAndDropVisualMode.Copy;

                        if (evt.type == EventType.DragPerform)
                        {
                            value = draggedComponent;
                        }

                        evt.Use();

                    }
                }
            }

            GUI.enabled = (clipboard != null);
            {
                var tooltip = (clipboard != null) ? string.Format("Paste {0} ({1})", clipboard.name, clipboard.GetType().Name) : "";
                var content = new GUIContent("Paste", tooltip);
                if (GUILayout.Button(content, "minibutton", GUILayout.Width(50)))
                {
                    value = clipboard;
                }
            }
            GUI.enabled = true;

        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(3);

        return value;

    }

    public static ComponentPropertyGroup BeginGroup(string label)
    {
        return BeginGroup(label, LabelWidth);
    }

    public static ComponentPropertyGroup BeginGroup(string label, float labelWidth)
    {
        return new ComponentPropertyGroup(label, labelWidth);
    }

    public static float LabelWidth
    {
        get { return 75f; }
        set { EditorGUIUtility.labelWidth = value; }
    
//        get
//        {

//#if UNITY_4_3
//                    return EditorGUIUtility.labelWidth;
//#else
//            var members = typeof(EditorGUIUtility).GetMember("labelWidth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
//            if (members == null || members.Length != 1)
//                return 75f;

//            var member = members[0];

//            if (member is FieldInfo)
//            {
//                return (float)((FieldInfo)member).GetValue(null);
//            }

//            if (member is PropertyInfo)
//            {
//                return (float)((PropertyInfo)member).GetValue(null, null);
//            }

//            return 75f;
//#endif

//        }
//        set
//        {

//#if UNITY_4_3
//                    EditorGUIUtility.labelWidth = value;
//#else
//            var members = typeof(EditorGUIUtility).GetMember("labelWidth", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
//            if (members == null || members.Length != 1)
//                return;

//            var member = members[0];

//            if (member is FieldInfo)
//            {
//                ((FieldInfo)member).SetValue(null, value);
//            }

//            if (member is PropertyInfo)
//            {
//                ((PropertyInfo)member).SetValue(null, value, null);
//            }
//#endif

//        }
    }
}

public class ComponentPropertyGroup : IDisposable
{

    private float savedLabelWidth = 0;

    public ComponentPropertyGroup(string label)
        : this(label, 100)
    {
    }

    public ComponentPropertyGroup(string label, float labelWidth)
    {

        savedLabelWidth = ComponentEditorUtils.LabelWidth;

        GUILayout.Label(label, "HeaderLabel");
        EditorGUI.indentLevel += 1;

        ComponentEditorUtils.LabelWidth = labelWidth;

    }

    #region IDisposable Members

    public void Dispose()
    {
        EditorGUI.indentLevel -= 1;
        ComponentEditorUtils.LabelWidth = savedLabelWidth;
    }

    #endregion

}

internal static class ReflectionExtensions
{

    /// <summary>
    /// Returns all instance fields on an object, including inherited fields
    /// </summary>
    internal static FieldInfo[] GetAllFields(this Type type)
    {

        // http://stackoverflow.com/a/1155549/154165

        if (type == null)
            return new FieldInfo[0];

        BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        return
            type.GetFields(flags)
            .Concat(GetAllFields(type.BaseType))
            .Where(f => !f.IsDefined(typeof(HideInInspector), true))
            .ToArray();

    }

    internal static object GetProperty(this object target, string property)
    {

        if (target == null)
            throw new NullReferenceException("Target is null");

        var members = target.GetType().GetMember(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (members == null || members.Length == 0)
            throw new IndexOutOfRangeException("Property not found: " + property);

        var member = members[0];

        if (member is FieldInfo)
        {
            return ((FieldInfo)member).GetValue(target);
        }

        if (member is PropertyInfo)
        {
            return ((PropertyInfo)member).GetValue(target, null);
        }

        throw new InvalidOperationException("Member type not supported: " + member.MemberType);

    }

    internal static void SetProperty(this object target, string property, object value)
    {

        if (target == null)
            throw new NullReferenceException("Target is null");

        var members = target.GetType().GetMember(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (members == null || members.Length == 0)
            throw new IndexOutOfRangeException("Property not found: " + property);

        var member = members[0];

        if (member is FieldInfo)
        {
            ((FieldInfo)member).SetValue(target, value);
            return;
        }

        if (member is PropertyInfo)
        {
            ((PropertyInfo)member).SetValue(target, value, null);
            return;
        }

        throw new InvalidOperationException("Member type not supported: " + member.MemberType);

    }

}
public static class MenuExtensions
{

    //[MenuItem("Tools/Daikon Forge/Help/Getting Started", false, 98)]
    //public static void ShowDocumentation(MenuCommand command)
    //{
    //    Help.BrowseURL("http://www.daikonforge.com/docs/Getting%20Started%20with%20DFGUI.pdf");
    //}

    //[MenuItem("Tools/Daikon Forge/Help/Daikon Forge Website")]
    //public static void ShowHelp(MenuCommand command)
    //{
    //    Help.BrowseURL("http://www.daikonforge.com/dfgui/");
    //}

    //[MenuItem("Tools/Daikon Forge/Help/Support Forums")]
    //public static void ShowSupportForums(MenuCommand command)
    //{
    //    Help.BrowseURL("http://www.daikonforge.com/dfgui/forums/");
    //}

    //[MenuItem("Tools/Daikon Forge/Help/Class Library Documentation", false, 99)]
    //public static void ShowAPIDocs(MenuCommand command)
    //{
    //    Help.BrowseURL("http://www.daikonforge.com/docs/df-gui/");
    //}

    [MenuItem("CONTEXT/Component/Copy Component Reference")]
    public static void CopyControlReference(MenuCommand command)
    {
        var control = command.context as Component;
        Debug.Log("Control reference copied: " + control.name + " (" + control.GetType().Name + ")");
        ComponentEditorUtils.clipboard = control;
    }

}