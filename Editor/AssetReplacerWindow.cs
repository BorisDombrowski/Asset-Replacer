using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetReplacerWindow : EditorWindow
{
    private Object _assetToReplace;
    private Object _replaceBy;

    private static List<System.Type> _forbiddenTypes = new List<System.Type>
        {
            typeof(DefaultAsset),
            typeof(SceneAsset),
            typeof(MonoScript),
            typeof(GameObject)
        };

    [MenuItem("Window/Custom/Asset Replacer")]
    public static void ShowWindow()
    {
        GetWindow<AssetReplacerWindow>("Asset Replacer");
    }

    private void OnGUI()
    {
        GUILayout.Label(new GUIContent("Asset to replace:", "Asset you want to replace."));
        _assetToReplace = EditorGUILayout.ObjectField(_assetToReplace, typeof(Object), false);

        EditorGUILayout.Space(10f);

        GUILayout.Label(new GUIContent("Replace by:", "Asset which you want to replace it."));
        _replaceBy = EditorGUILayout.ObjectField(_replaceBy, typeof(Object), false);

        EditorGUILayout.Space(10f);

        var notNull = _assetToReplace != null && _replaceBy != null;

        if (notNull)
        {
            var sameType = _assetToReplace.GetType() == _replaceBy.GetType();
            var notForbiddenTypes = HasPremittedType(_assetToReplace.GetType()) && HasPremittedType(_replaceBy.GetType());
            var notSameAsset = _assetToReplace != _replaceBy;

            if (sameType && notSameAsset && notForbiddenTypes)
            {
                if (GUILayout.Button("Replace"))
                {
                    var pathToReplace = AssetDatabase.GetAssetPath(_assetToReplace);
                    var metaToReplace = AssetDatabase.GetTextMetaFilePathFromAssetPath(pathToReplace);

                    var pathReplaceBy = AssetDatabase.GetAssetPath(_replaceBy);
                    var metaReplaceBy = AssetDatabase.GetTextMetaFilePathFromAssetPath(pathReplaceBy);

                    Replace(metaToReplace, metaReplaceBy);
                }
            }
            else
            {
                if (!sameType)
                {
                    EditorGUILayout.HelpBox($"Asset types are not matches", MessageType.Error);
                }

                if (!notSameAsset)
                {
                    EditorGUILayout.HelpBox($"You have been selected the same asset", MessageType.Error);
                }

                if(!notForbiddenTypes)
                {
                    EditorGUILayout.HelpBox($"One of selected object has forbidden type", MessageType.Error);
                }
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Clear"))
        {
            ClearWindow();
        }

        EditorGUILayout.Space(10f);
    }

    private void Replace(string metaToReplace, string metaReplaceBy)
    {
        var guidToReplace = GetMetaGUID(metaToReplace);
        var guidReplaceBy = GetMetaGUID(metaReplaceBy);

        ReplaceMetaGUID(metaToReplace, guidReplaceBy);
        ReplaceMetaGUID(metaReplaceBy, guidToReplace);

        AssetDatabase.Refresh();
    }

    private string GetMetaGUID(string meta)
    {
        var lines = File.ReadAllLines(meta);
        var guid = lines[1].Split(' ')[1];
        return guid;
    }

    private void ReplaceMetaGUID(string metaPath, string GUID)
    {
        var file = File.ReadAllLines(metaPath);
        var guidString = $"guid: {GUID}";
        file[1] = guidString;
        File.WriteAllLines(metaPath, file);
    }

    private bool HasPremittedType(System.Type assetType)
    {
        return !_forbiddenTypes.Contains(assetType);
    }

    private void ClearWindow()
    {
        _assetToReplace = null;
        _replaceBy = null;
    }
}
