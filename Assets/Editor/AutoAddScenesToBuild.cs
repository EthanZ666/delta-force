#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class AutoAddScenesToBuild
{
    private static readonly string[] RequiredSceneNames =
    {
        "SplashScene",
        "MainMenuScene",
        "MapSelectScene",
        "SettingsScene",
        "SampleScene",
        "URP2DSceneTemplate",
    };

    static AutoAddScenesToBuild()
    {
        UpdateBuildSettings();
    }

    private static void UpdateBuildSettings()
    {
        var allScenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToList();

        var requiredPaths = new HashSet<string>();
        foreach (var sceneName in RequiredSceneNames)
        {
            var match = allScenePaths.FirstOrDefault(p =>
                System.IO.Path.GetFileNameWithoutExtension(p) == sceneName);

            if (!string.IsNullOrEmpty(match))
                requiredPaths.Add(match);
        }

        var current = EditorBuildSettings.scenes.ToList();

        // 保留已有 + 加入缺的
        foreach (var path in requiredPaths)
        {
            if (!current.Any(s => s.path == path))
                current.Add(new EditorBuildSettingsScene(path, true));
        }

        // 把 RequiredSceneNames 排在前面（可选但很舒服）
        current = current
            .OrderByDescending(s => RequiredSceneNames.Contains(System.IO.Path.GetFileNameWithoutExtension(s.path)))
            .ThenBy(s => s.path)
            .ToList();

        EditorBuildSettings.scenes = current.ToArray();
    }
}
#endif
