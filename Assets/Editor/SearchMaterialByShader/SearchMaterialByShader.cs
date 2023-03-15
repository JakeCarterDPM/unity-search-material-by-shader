// Created by Jake Carter - 2023/03/15. Using Unity 2021.3.20f1
// Modification is allowed. Crediting is required.
// Purpose: Searches materials by shader.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SearchMaterialByShader : EditorWindow
{
    private Shader selectedShader;
    private List<Material> foundMaterials = new();
    private Vector2 scrollPos;
    private bool showResults = false;
    private int currentPage = 0;
    private const int materialsPerPage = 10;
    private const int buttonsPerRow = 10;
    private Editor materialPreviewObjectEditor;
    private GUIStyle bgColor;

    [MenuItem("Tools/Search Material By Shader", false, 22)]
    public static void ShowWindow()
    {
        SearchMaterialByShader window = GetWindow<SearchMaterialByShader>(false, "Search Material By Shader");
        Vector2 windowSize = new(700, 580);
        Rect windowRect = new(Screen.width / 2, Screen.height / 2, windowSize.x, windowSize.y);
        window.position = windowRect;
        window.minSize = new Vector2(300, 300);
        window.Show();
    }
    private void CreateGUI()
    {
        bgColor = new();
        selectedShader = Shader.Find("Standard");
    }

    private void OnGUI()
    {
        //INDEXING
        int startIndex = currentPage * materialsPerPage;
        int endIndex = Mathf.Min(startIndex + materialsPerPage, foundMaterials.Count);

        //SETTINGS
        if (!showResults)
        {
            GUILayout.Space(25f);
            GUILayout.Label($"Target Shader:");
            selectedShader = EditorGUILayout.ObjectField(selectedShader, typeof(Shader), false, GUILayout.Height(50)) as Shader;

            if (GUILayout.Button("Find Materials", GUILayout.Height(100)))
            {
                string shaderName = selectedShader.name;
                FindShader(shaderName);
                showResults = true;
                currentPage = 0;
            }
        }

        if (showResults)
        {
            selectedShader = EditorGUILayout.ObjectField("Taget Shader: ", selectedShader, typeof(Shader), false, GUILayout.Height(25)) as Shader;

            if (GUILayout.Button("Find Materials"))
            {
                string shaderName = selectedShader.name;
                FindShader(shaderName);
                showResults = true;
                currentPage = 0;
            }

            if (GUILayout.Button("Select All - On Page"))
            {
                if (showResults)
                {
                    List<Material> materialsOnPage = foundMaterials.GetRange(startIndex, endIndex - startIndex);
                    Selection.objects = materialsOnPage.ToArray();
                }
            }
            if (GUILayout.Button("Select All - All Pages"))
            {
                Selection.objects = foundMaterials.ToArray();
            }

            //PAGE NAVIGATION        
            int pageCount = Mathf.CeilToInt((float)foundMaterials.Count / materialsPerPage);
            GUILayout.Space(10f);
            GUILayout.Label($"Current Page: {currentPage + 1}/{pageCount}", GUILayout.Height(20));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            for (int i = 0; i < pageCount; i++)
            {
                if (i % buttonsPerRow == 0)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                }
                GUI.enabled = i != currentPage;
                if (GUILayout.Button((i + 1).ToString(), GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                {
                    currentPage = i;
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();



            GUILayout.Label("Found Materials:");

            //SCROLL VIEW
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.ExpandHeight(true));

            //LISTED MATERIALS
            for (int i = startIndex; i < endIndex; i++)
            {
                Material mat = foundMaterials[i];
                if (mat == null) continue;
                EditorGUILayout.BeginHorizontal();

                materialPreviewObjectEditor = Editor.CreateEditor(mat);
                materialPreviewObjectEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), bgColor);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(mat).Replace("Assets/", ""), GUILayout.Height(20));

                if (GUILayout.Button(mat.name, GUILayout.Height(80)))
                {
                    Selection.activeObject = mat;
                    EditorGUIUtility.PingObject(mat);
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10f);
            }

            EditorGUILayout.EndScrollView();
        }
        GUILayout.FlexibleSpace(); // Add flexible space to push the button to the bottom
        GUILayout.BeginHorizontal(); // Begin horizontal group to align button to the right
        GUILayout.FlexibleSpace(); // Add another flexible space to align button to the right
        if (GUILayout.Button("Created by Jake Carter", EditorStyles.linkLabel, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
        {
            Application.OpenURL("https://jcfolio.weebly.com/");
        }
        GUILayout.EndHorizontal(); // End horizontal group
    }


    private void FindShader(string shaderName)
    {
        int count = 0;
        foundMaterials.Clear();
        string[] allMaterialPaths = AssetDatabase.FindAssets("t:Material");
        foreach (string path in allMaterialPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(path);
            if (assetPath.StartsWith("Assets/"))
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (mat != null && mat.shader != null && !string.IsNullOrEmpty(mat.shader.name))
                {
                    if (mat.shader.name.Equals(shaderName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundMaterials.Add(mat);
                        count++;
                    }
                }
            }
        }
        foundMaterials.Sort((a, b) => a.name.CompareTo(b.name));
    }
}