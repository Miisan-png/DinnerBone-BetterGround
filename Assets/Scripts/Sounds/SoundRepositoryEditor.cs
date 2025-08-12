using UnityEngine;
using UnityEditor;

public class SoundRepositoryWindow : EditorWindow
{
    private static string SOUND_REPO_PATH = "Assets/ScriptableObjects/SO_SoundRepo.asset";

    private SO_SoundRepository soundRepo;
    private int selectedTab = 0;
    private string searchQuery = "";
    private int selectedIndex = -1;
    private Vector2 listScrollPosition;
    private Vector2 propertiesScrollPosition;

    private float panelSplitRatio = 0.3f; // 30% Left, 70% Right
    private bool isResizing = false;

    private Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark Grey Border

    [MenuItem("Settings/Sound Repository")]
    public static void ShowWindow()
    {
        SoundRepositoryWindow window = GetWindow<SoundRepositoryWindow>("Sound Repository");
        window.minSize = new Vector2(600, 400);
    }

    private void OnGUI()
    {
        //soundRepo = (SO_SoundRepository)EditorGUILayout.ObjectField("Sound Repository", soundRepo, typeof(SO_SoundRepository), false);
        soundRepo =  AssetDatabase.LoadAssetAtPath<SO_SoundRepository>(SOUND_REPO_PATH);

        if (soundRepo == null) return;

        EditorGUILayout.Space();
        selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Sounds", "BGM", "Dialogue" });
        EditorGUILayout.Space();

        SerializedObject serializedRepo = new SerializedObject(soundRepo);
        SerializedProperty listProperty = GetSelectedList(serializedRepo);
        if (listProperty == null) return;

        float totalWidth = position.width;
        float leftPanelWidth = totalWidth * panelSplitRatio;
        float rightPanelWidth = totalWidth * (1 - panelSplitRatio);

        EditorGUILayout.BeginHorizontal();

        // Left Panel (List View) - 30%
        GUILayout.BeginVertical(GUILayout.Width(leftPanelWidth));
        DrawSearchBar();
        DrawList(listProperty);
        DrawAddButton(listProperty);
        GUILayout.EndVertical();

        // Border between panels
        DrawBorder(new Rect(leftPanelWidth, 75, 2, position.height));

        // Resizable Divider
        DrawResizeHandle();

        // Right Panel (Properties) - 70%
        GUILayout.BeginVertical();
        DrawProperties(listProperty);
        GUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        serializedRepo.ApplyModifiedProperties();
    }

    private void DrawSearchBar()
    {
        searchQuery = EditorGUILayout.TextField("Search:", searchQuery);
        EditorGUILayout.Space();
    }

    private void DrawList(SerializedProperty listProperty)
    {
        listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition, GUILayout.Height(position.height - 120));

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
            SerializedProperty nameProp = element.FindPropertyRelative("SoundName");

            if (!string.IsNullOrEmpty(searchQuery) && !nameProp.stringValue.ToLower().Contains(searchQuery.ToLower()))
                continue;

            GUIStyle style = (i == selectedIndex) ? EditorStyles.boldLabel : EditorStyles.label;

            if (GUILayout.Button(nameProp.stringValue, style))
            {
                selectedIndex = i;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawAddButton(SerializedProperty listProperty)
    {
        if (GUILayout.Button("Add New Sound"))
        {
            listProperty.arraySize++;
            selectedIndex = listProperty.arraySize - 1;
        }
    }

    private void DrawProperties(SerializedProperty listProperty)
    {
        if (selectedIndex < 0 || selectedIndex >= listProperty.arraySize) return;

        SerializedProperty element = listProperty.GetArrayElementAtIndex(selectedIndex);

        propertiesScrollPosition = EditorGUILayout.BeginScrollView(propertiesScrollPosition);
        EditorGUILayout.LabelField("Sound Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(element, true);

        if (GUILayout.Button("Remove Sound", GUILayout.Width(120)))
        {
            listProperty.DeleteArrayElementAtIndex(selectedIndex);
            selectedIndex = Mathf.Clamp(selectedIndex - 1, 0, listProperty.arraySize - 1);
        }
        EditorGUILayout.EndScrollView();
    }

    private SerializedProperty GetSelectedList(SerializedObject serializedRepo)
    {
        switch (selectedTab)
        {
            case 0: return serializedRepo.FindProperty("SoundList");
            case 1: return serializedRepo.FindProperty("BGMList");
            case 2: return serializedRepo.FindProperty("DialogueList");
            default: return null;
        }
    }

    private void DrawResizeHandle()
    {
        Rect dragRect = new Rect(position.width * panelSplitRatio, 0, 5, position.height);
        EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.ResizeHorizontal);

        if (Event.current.type == EventType.MouseDown && dragRect.Contains(Event.current.mousePosition))
        {
            isResizing = true;
        }
        if (isResizing)
        {
            panelSplitRatio = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.2f, 0.8f);
        }
        if (Event.current.type == EventType.MouseUp)
        {
            isResizing = false;
        }

        GUILayout.Space(5);
    }

    private void DrawBorder(Rect rect)
    {
        EditorGUI.DrawRect(rect, borderColor);
    }
}
