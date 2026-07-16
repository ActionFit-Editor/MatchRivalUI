#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ActionFit.MatchRival.UI.Editor
{
    public static class MatchRivalUIPackageMenu
    {
        private const string MenuRoot = "Tools/Package/ActionFit Match Rival UI/";
        private const string ReadmePath = "Packages/com.actionfit.match-rival.ui/README.md";

        [MenuItem(MenuRoot + "Create Demo", false, 80)]
        private static void CreateDemo()
        {
            var demo = new GameObject("Match Rival UI Demo");
            Undo.RegisterCreatedObjectUndo(demo, "Create Match Rival UI Demo");
            demo.AddComponent<MatchRivalBootstrap>();
            Selection.activeGameObject = demo;
        }

        [MenuItem(MenuRoot + "README", false, 907)]
        private static void OpenReadme()
        {
            var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(ReadmePath);
            if (readme == null)
            {
                EditorUtility.DisplayDialog("Package README", $"README was not found.\n{ReadmePath}", "OK");
                return;
            }

            Selection.activeObject = readme;
            AssetDatabase.OpenAsset(readme);
        }
    }
}
#endif
