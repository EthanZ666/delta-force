#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

[InitializeOnLoad]
public static class AutoAddScenesToBuild
{
    private static readonly string[] RequiredSceneNames =
    {
        //如果你以后新加 MapSelect/Map1/Map2就按一样的格式
        "SplashScene",
        "MainMenuScene",
        "SampleScene",
        "URP2DSceneTemplate"
    };

    static AutoAddScenesToBuild()
    {
        EnsureScenes();
    }

    private static void EnsureScenes()
    {
        // 找到项目里所有 .unity 场景文件
        var sceneGuids = AssetDatabase.FindAssets("t:Scene");
        var allScenePaths = sceneGuids
            .Select(AssetDatabase.GUIDToAssetPath)
            .ToArray();

        // 只挑我们需要的那些场景路径
        var requiredPaths = allScenePaths
            .Where(p => RequiredSceneNames.Any(n => p.EndsWith(n + ".unity")))
            .Distinct()
            .ToArray();

        if (requiredPaths.Length == 0) return;

        // 读取当前 Build Settings 列表
        var existing = EditorBuildSettings.scenes.ToList();

        bool changed = false;

        foreach (var path in requiredPaths)
        {
            if (existing.Any(s => s.path == path)) continue;
            existing.Add(new EditorBuildSettingsScene(path, true));
            changed = true;
        }

        // 如果有新增，就写回 Build Settings
        if (changed)
        {
            EditorBuildSettings.scenes = existing.ToArray();
            // 可选：打印一下确认
            // UnityEngine.Debug.Log("[AutoAddScenesToBuild] Scenes added to Build Settings.");
        }
    }
}
#endif
