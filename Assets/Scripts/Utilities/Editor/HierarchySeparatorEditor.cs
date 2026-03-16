using UnityEditor;
using UnityEngine;

namespace CrispyCube
{
    [InitializeOnLoad]
    public static class HierarchySeparatorEditor
    {
        const string SeparatorLabel = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
        const string EditorOnlyTag = "EditorOnly";


        static HierarchySeparatorEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= DrawSeparator;
            EditorApplication.hierarchyWindowItemOnGUI += DrawSeparator;
        }

        [MenuItem("GameObject/━━━ Add Line ━━━", false, 10)]
        static void CreateSeparator(MenuCommand menuCommand)
        {
            GameObject separatorObject = new GameObject(SeparatorLabel);
            GameObjectUtility.SetParentAndAlign(separatorObject, menuCommand.context as GameObject);
            separatorObject.tag = EditorOnlyTag;
            Undo.RegisterCreatedObjectUndo(separatorObject, "Create Line Separator");
            Selection.activeGameObject = separatorObject;
        }

        static void DrawSeparator(int instanceId, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.EntityIdToObject(instanceId) as GameObject;
            if (gameObject == null || gameObject.name != SeparatorLabel)
            {
                return;
            }
        }
    }
}
