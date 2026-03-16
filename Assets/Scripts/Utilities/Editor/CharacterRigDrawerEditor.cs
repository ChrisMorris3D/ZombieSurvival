using UnityEditor;
using UnityEngine;

namespace CrispyCube
{
    [InitializeOnLoad]
    public static class CharacterRigDrawerEditor
    {
        static CharacterRigDrawerEditor()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            CharacterRigDrawer[] rigDrawers = Object.FindObjectsByType<CharacterRigDrawer>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (CharacterRigDrawer rigDrawer in rigDrawers)
            {
                if (rigDrawer == null || !rigDrawer.isActiveAndEnabled)
                {
                    continue;
                }

                if (rigDrawer.DrawOnlyWhenSelected && Selection.activeGameObject != rigDrawer.gameObject)
                {
                    continue;
                }

                Transform root = rigDrawer.RigRoot;
                if (root == null)
                {
                    continue;
                }

                DrawJointHandles(rigDrawer, root);
            }
        }

        static void DrawJointHandles(CharacterRigDrawer rigDrawer, Transform joint)
        {
            if (joint == null || (!rigDrawer.IncludeInactive && !joint.gameObject.activeInHierarchy))
            {
                return;
            }

            float handleSize = HandleUtility.GetHandleSize(joint.position) * rigDrawer.JointRadius;

            Handles.color = rigDrawer.JointColor;
            if (Handles.Button(joint.position, joint.rotation, handleSize, handleSize, DrawWireSphereHandleCap))
            {
                Selection.activeTransform = joint;
                EditorGUIUtility.PingObject(joint);
            }

            foreach (Transform child in joint)
            {
                if (child == null || (!rigDrawer.IncludeInactive && !child.gameObject.activeInHierarchy))
                {
                    continue;
                }

                Handles.color = rigDrawer.BoneColor;
                Handles.DrawLine(joint.position, child.position);
                DrawJointHandles(rigDrawer, child);
            }
        }

        static void DrawWireSphereHandleCap(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Layout:
                    HandleUtility.AddControl(controlId, HandleUtility.DistanceToCircle(position, size));
                    break;

                case EventType.Repaint:
                    Matrix4x4 previousMatrix = Handles.matrix;
                    Color previousColor = Handles.color;

                    Handles.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.right, size);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.up, size);
                    Handles.DrawWireDisc(Vector3.zero, Vector3.forward, size);

                    Handles.matrix = previousMatrix;
                    Handles.color = previousColor;
                    break;
            }
        }
    }
}
