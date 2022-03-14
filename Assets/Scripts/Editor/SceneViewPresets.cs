using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SceneViewPresets
{
    [System.Serializable]
    public class Preset
    {
        public string name;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 pivot;
        public float size;
        public bool twoD;
        public bool ortho;
        public bool isEditing { get; set; }
        public Preset Save(SceneView aView)
        {
            pos = aView.camera.transform.position;
            pivot = aView.pivot;
            rot = aView.rotation.eulerAngles;
            size = aView.size;
            twoD = aView.in2DMode;
            ortho = aView.orthographic;
            return this;
        }
        public void Restore(SceneView aView)
        {
            aView.in2DMode = twoD;
            aView.LookAt(pivot, Quaternion.Euler(rot), size, ortho);
        }
    }
    [System.Serializable]
    public class Settings
    {

        public List<Preset> presets = new List<Preset>();
        public bool m_ShowPresets = true;
        public bool m_Expanded = false;
        public Rect m_WinPos = new Rect(10, 30, 70, 40);
        public bool showCones = true;
        public bool enableMoving { get; set; }
    }
    static Color[] m_Colors = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };
    static SceneViewPresets m_Instance;
    static SceneViewPresets()
    {
        m_Instance = new SceneViewPresets();
    }
    [MenuItem("Tools/B83/Toggle SceneView Presets")]
    public static void ToggleControls()
    {
        m_Instance.settings.m_ShowPresets = !m_Instance.settings.m_ShowPresets;
        SceneView.RepaintAll();
    }

    public SceneViewPresets()
    {
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneView;
#else
        // In the past we had to use the "onSceneGUIDelegate" event
        SceneView.onSceneGUIDelegate += OnSceneView;
#endif
    }

    private bool m_Initialized = false;
    private Vector2 m_ScrollPos;
    private SceneView m_SceneView;
    private GUIStyle m_ButtonWordWrap = null;
    public Settings settings = new Settings();

    public void Initialize()
    {
        var data = EditorPrefs.GetString("B83.SceneViewPresets", "");
        if (data != "")
            settings = JsonUtility.FromJson<Settings>(data);
        m_Initialized = true;
    }
    public void SaveSettings()
    {
        EditorPrefs.SetString("B83.SceneViewPresets", JsonUtility.ToJson(settings));
    }

    void OnSceneView(SceneView aView)
    {
        if (!m_Initialized)
            Initialize();
        m_SceneView = aView;
        Event e = Event.current;
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
        
        if (e.type == EventType.KeyUp && e.alt && e.control && e.keyCode == KeyCode.P)
        {
            settings.m_ShowPresets = !settings.m_ShowPresets;
            e.Use();
        }
        if (settings.m_ShowPresets)
        {
            var tmpRect = settings.m_WinPos;
            if (settings.m_Expanded && settings.enableMoving)
                tmpRect.width += 100;
            var newPos = GUI.Window(8301, tmpRect, OnWindow, "Presets");
            if (newPos != tmpRect)
                SaveSettings();
            settings.m_WinPos.x = Mathf.Clamp(newPos.x, 15 - newPos.width, aView.position.width - 15);
            settings.m_WinPos.y = Mathf.Clamp(newPos.y, 21, aView.position.height - 15);
            if (settings.showCones && e.control)
            {
                int colorCount = 0;
                foreach (var p in settings.presets)
                {
                    var c = m_Colors[colorCount++];
                    colorCount = colorCount % m_Colors.Length;
                    c.a = 1f;
                    Handles.color = c;
                    var q = Quaternion.Euler(p.rot);
                    var dir = q * Vector3.forward;

                    q = Quaternion.LookRotation(-dir, Vector3.up);
                    bool enableCone = !p.ortho && !p.twoD && (m_SceneView.camera.transform.position - p.pos).sqrMagnitude > 2;
                    if (enableCone && Handles.Button(p.pos - q * Vector3.forward * 0.7f, q, 1, 0.5f, Handles.ConeHandleCap))
                        p.Restore(m_SceneView);
                    if (Handles.Button(p.pivot, Quaternion.identity, HandleUtility.GetHandleSize(p.pivot) * 0.1f, 0.25f, Handles.DotHandleCap))
                        p.Restore(m_SceneView);
                }
            }
        }
    }

    void OnWindow(int id)
    {
        if (m_ButtonWordWrap == null)
        {
            m_ButtonWordWrap = new GUIStyle("button");
            m_ButtonWordWrap.wordWrap = true;
        }
        Event e = Event.current;
        var tmp = GUILayout.Toggle(settings.m_Expanded, settings.m_Expanded ? "Collapse" : "Expand", "Button");
        if (tmp != settings.m_Expanded && tmp)
        {
            settings.m_Expanded = true;
            settings.m_WinPos.size = new Vector2(200, 300);
            SaveSettings();
        }
        else if (tmp != settings.m_Expanded && !tmp)
        {
            settings.m_Expanded = false;
            settings.m_WinPos.size = new Vector2(70, 40);
            SaveSettings();
        }
        if (settings.m_Expanded)
        {
            m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, GUIStyle.none, GUIStyle.none);
            int colorCount = 0;
            for (int i = 0; i < settings.presets.Count; i++)
            {
                var p = settings.presets[i];
                GUI.color = Color.white;
                GUILayout.BeginHorizontal();
                if (p.isEditing)
                {
                    p.name = EditorGUILayout.TextArea(p.name);
                    if (GUILayout.Button("done", GUILayout.Width(50)))
                    {
                        p.isEditing = false;
                        SaveSettings();
                    }
                    GUI.color = Color.red;
                    if (GUILayout.Button("u", GUILayout.Width(20)))
                    {
                        settings.presets.Remove(p);
                        SaveSettings();
                        GUIUtility.ExitGUI();
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    if (e.control)
                    {
                        GUI.color = m_Colors[colorCount++];
                        colorCount = colorCount % m_Colors.Length;
                    }

                    if (GUILayout.Button(p.name, m_ButtonWordWrap))
                    {
                        if (e.button == 0)
                            p.Restore(m_SceneView);
                        else if (e.button == 1)
                            ShowContextMenu(p);
                    }
                }
                if (settings.enableMoving)
                {
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("up", GUILayout.Width(30)))
                    {
                        var temp = settings.presets[i - 1];
                        settings.presets[i - 1] = p;
                        settings.presets[i] = temp;
                        SaveSettings();
                    }
                    GUI.enabled = i < settings.presets.Count - 1;
                    if (GUILayout.Button("down", GUILayout.Width(50)))
                    {
                        var temp = settings.presets[i + 1];
                        settings.presets[i + 1] = p;
                        settings.presets[i] = temp;
                        SaveSettings();
                    }
                    GUI.enabled = true;
                    GUI.color = Color.red;
                    if (GUILayout.Button("u", GUILayout.Width(20)))
                    {
                        settings.presets.Remove(p);
                        SaveSettings();
                        GUIUtility.ExitGUI();
                    }
                    GUI.color = Color.white;

                }
                GUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
            GUILayout.EndScrollView();
            if (settings.showCones)
                GUILayout.Label("Hold CTRL to vizualize");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New") && e.button == 0)
            {
                settings.presets.Add(new Preset { name = "new", isEditing = true }.Save(m_SceneView));
                SaveSettings();
            }
            if (settings.enableMoving && GUILayout.Button("done editing", GUILayout.Width(120)))
                settings.enableMoving = false;
            GUILayout.EndHorizontal();
        }
        GUI.DragWindow();
    }

    void ShowContextMenu(Preset aPreset)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Save current position"), false, () =>
        {
            aPreset.Save(m_SceneView);
            SaveSettings();
        });
        menu.AddItem(new GUIContent("Rename preset"), false, () => aPreset.isEditing = true);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Edit all presets"), false, () => settings.enableMoving = true);
        menu.AddItem(new GUIContent("Show camera cones"), settings.showCones, () => { settings.showCones = !settings.showCones; SaveSettings(); });
        menu.AddItem(new GUIContent("Settings/Export to file"), false, () =>
        {
            var file = EditorUtility.SaveFilePanel("SceneViewPresetSettings", "", "SceneViewPresets", "json");
            if (!string.IsNullOrEmpty(file))
                System.IO.File.WriteAllText(file, JsonUtility.ToJson(settings));
        });
        menu.AddItem(new GUIContent("Settings/Import from file"), false, () =>
        {
            var file = EditorUtility.OpenFilePanel("SceneViewPresetSettings", "", "json");
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                return;
            var newSettings = JsonUtility.FromJson<Settings>(System.IO.File.ReadAllText(file));
            if (newSettings == null)
            {
                Debug.LogError("Settings file could not be loaded. Error in the file");
                return;
            }
            settings = newSettings;
            SaveSettings();
        });
        menu.AddItem(new GUIContent("Settings/Import Presets from file (keep current presets)"), false, () =>
        {
            var file = EditorUtility.OpenFilePanel("SceneViewPresetSettings", "", "json");
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                return;
            var newSettings = JsonUtility.FromJson<Settings>(System.IO.File.ReadAllText(file));
            if (newSettings == null)
            {
                Debug.LogError("Settings file could not be loaded. Error in the file");
                return;
            }
            settings.presets.AddRange(newSettings.presets);
            SaveSettings();
        });
        menu.AddItem(new GUIContent("About/Created by Bunny83"), false, null);
        menu.AddSeparator("About/");
        menu.AddItem(new GUIContent("About/For more information see"), false, null);
        menu.AddItem(new GUIContent("About/UnityAnswers"), false, () => Application.OpenURL("https://answers.unity.com/questions/1515748/how-to-save-scene-view-camera-perspectives.html"));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Preset"), false, () =>
        {
            settings.presets.Remove(aPreset);
            SaveSettings();
        });
        menu.ShowAsContext();
    }
}