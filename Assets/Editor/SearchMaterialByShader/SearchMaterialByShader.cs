// Created by Jake Carter - 2023/03/15. Using Unity 2021.3.20f1
// Modification is allowed. Crediting is required.
// Purpose: Searches materials by shader.
// Version 1.1.1 - Filter for embedded materials update.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SearchMaterialByShader : EditorWindow
{
    //Options
    private Shader selectedShader;
    private bool includeEmbeddedMaterials = false;
    private bool onlyEmbeddedMaterials = false;

    //Results
    private List<Material> foundMaterials = new();
    private List<Editor> materialPreviews = new();

    //Window Management
    private Vector2 scrollPos;
    private bool showResults = false;
    private int currentPage = 0;
    private const int materialsPerPage = 10;
    private const int buttonsPerRow = 10;
    private GUIStyle bgColor;
    private int heightSelectedShaderPicker = 50;
    private readonly int heightSelectedShaderPickerInitial = 50;
    private readonly int heightSelectedShaderPickerResults = 25;
    private int heightFindMaterialsBtn = 100;
    private readonly int heightFindMaterialsBtnInital = 100;
    private readonly int heightFindMaterialsBtnResults = 25;
    private int startIndex = 0;
    private int endIndex = 0;
    private int pageCount = 0;
    private string currentPageTxt;

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
        MenuChangeCache();
    }
    private void MenuChangeCache()
    {
        if (showResults)
        {
            //Page Navigation
            startIndex = currentPage * materialsPerPage;
            endIndex = Mathf.Min(startIndex + materialsPerPage, foundMaterials.Count);
            pageCount = Mathf.CeilToInt((float)foundMaterials.Count / materialsPerPage);
            currentPageTxt = $"Current Page: {currentPage + 1}/{pageCount}";

            //Material Previews
            materialPreviews.Clear();
            materialPreviews = new Editor[foundMaterials.Count].ToList();
            for (int i = startIndex; i < endIndex; i++)
            {
                Material mat = foundMaterials[i];
                Editor materialPreviewObjectEditor = Editor.CreateEditor(mat);
                materialPreviews[i] = materialPreviewObjectEditor;
            }

            //GUI Formatting
            heightSelectedShaderPicker = heightSelectedShaderPickerResults;
            heightFindMaterialsBtn = heightFindMaterialsBtnResults;
        }
        else
        {
            //GUI Formatting
            heightSelectedShaderPicker = heightSelectedShaderPickerInitial;
            heightFindMaterialsBtn = heightFindMaterialsBtnInital;
        }
    }
    private void FindMaterials()
    {
        FindMaterialsWithShader(selectedShader.name);
        showResults = true;
        currentPage = 0;
        MenuChangeCache();
    }
    private void SelectMaterial(Material mat)
    {
        Selection.activeObject = mat;
        EditorGUIUtility.PingObject(mat);
    }
    private void SelectAllMaterials()
    {
        Selection.objects = foundMaterials.ToArray();
    }
    private void SelectAllMaterialsOnPage()
    {
        List<Material> materialsOnPage = foundMaterials.GetRange(startIndex, endIndex - startIndex);
        Selection.objects = materialsOnPage.ToArray();
    }
    private void SelectEditableMaterials()
    {
        List<Material> editableMaterials = new List<Material>();

        foreach (Material mat in foundMaterials)
        {
            if (!AssetDatabase.IsMainAsset(mat))
            {
                // Add only editable materials to the list
                editableMaterials.Add(mat);
            }
        }

        if (editableMaterials.Count > 0)
        {
            Selection.objects = editableMaterials.ToArray();
        }
        else
        {
            Debug.LogWarning("No editable materials found.");
        }
    }


    private void OnGUI()
    {
        //Main Buttons
        GUILayout.Space(15);
        GUILayout.Label($"Target Shader:");
        selectedShader = EditorGUILayout.ObjectField(selectedShader, typeof(Shader), false, GUILayout.Height(heightSelectedShaderPicker)) as Shader;

        //Options
        GUILayout.Space(5);
        GUILayout.Label($"Options:");
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

        //Include
        includeEmbeddedMaterials = GUILayout.Toggle(
            includeEmbeddedMaterials,
            new GUIContent("Include Embedded Materials (Un-editable).", "For materials that are embedded into files such as a 3D model, which can't be edited directly. Fix for these mats is to \"Extract Materials...\" under the materials tab for that model."),
            GUILayout.ExpandWidth(true));

        //Only
        if (includeEmbeddedMaterials)
        {
            onlyEmbeddedMaterials = GUILayout.Toggle(
                onlyEmbeddedMaterials,
                new GUIContent("Only Embedded Materials.", "Doesn't include editable materials in results."),
                GUILayout.ExpandWidth(true));
        }
        else
        {
            onlyEmbeddedMaterials = false;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        if (GUILayout.Button("Find Materials", GUILayout.Height(heightFindMaterialsBtn)))
        {
            FindMaterials();
        }

        //Results Panel
        if (showResults)
        {
            //Selection Buttons
            if (GUILayout.Button("Select All - On Page"))
            {
                SelectAllMaterialsOnPage();
            }
            if (GUILayout.Button("Select All - All Pages"))
            {
                SelectAllMaterials();
            }

            //Page Navigation
            GUILayout.Space(10f);
            GUILayout.Label(currentPageTxt, GUILayout.Height(20));
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
                    MenuChangeCache();
                }
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            //Found Materials List
            GUILayout.Label($"Found Materials: {foundMaterials.Count}");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.ExpandHeight(true));
            for (int i = startIndex; i < endIndex; i++)
            {
                Material mat = foundMaterials[i];
                if (mat == null)
                {
                    MenuChangeCache();
                    continue;
                }
                EditorGUILayout.BeginHorizontal();
                materialPreviews[i].OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), bgColor);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(mat), GUILayout.Height(20));
                if (GUILayout.Button(mat.name, GUILayout.Height(80)))
                {
                    SelectMaterial(mat);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10f);
            }
            EditorGUILayout.EndScrollView();
        }

        //Credits 
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("v1.1.1 Created by Jake Carter", EditorStyles.linkLabel, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
        {
            Application.OpenURL("https://jcfolio.weebly.com/");
        }
        GUILayout.EndHorizontal();
    }

    private void FindMaterialsWithShader(string shaderName)
    {
        int count = 0;
        foundMaterials.Clear();
        string[] allMaterialPaths = AssetDatabase.FindAssets("t:Material");

        foreach (string path in allMaterialPaths)
        {
            //Mats within Assets only.
            string assetPath = AssetDatabase.GUIDToAssetPath(path);
            if (!assetPath.StartsWith("Assets/"))
                continue;

            //Various checks.
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null || mat.shader == null || string.IsNullOrEmpty(mat.shader.name))
                continue;
            if (!mat.shader.name.Equals(shaderName, StringComparison.OrdinalIgnoreCase))
                continue;

            //Check if is embedded.
            if (IsMaterialEmbedded(assetPath))
            {
                //Check if including embedded mats.
                if (includeEmbeddedMaterials)
                {
                    foundMaterials.Add(mat);
                    count++;
                }
            }
            else
            {
                //Check if looking for embedded mats only.
                if (!onlyEmbeddedMaterials)
                {
                    foundMaterials.Add(mat);
                    count++;
                }
            }
        }
        foundMaterials.Sort((a, b) => a.name.CompareTo(b.name));
    }


    private bool IsMaterialEmbedded(string assetPath)
    {
        Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
        return assetType != typeof(Material);
    }
}