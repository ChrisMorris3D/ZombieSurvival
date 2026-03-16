using UnityEditor;
using UnityEngine;

namespace CrispyCube
{
    [CustomEditor(typeof(RigPoseBaker))]
    public class RigPoseBakerEditor : Editor
    {
        SerializedProperty targetAnimatorProperty;
        SerializedProperty rigRootProperty;
        SerializedProperty sourceClipProperty;
        SerializedProperty useNormalizedTimeProperty;
        SerializedProperty sampleTimeProperty;
        SerializedProperty includeRootTransformProperty;
        SerializedProperty bakeScaleProperty;

        void OnEnable()
        {
            targetAnimatorProperty = serializedObject.FindProperty("targetAnimator");
            rigRootProperty = serializedObject.FindProperty("rigRoot");
            sourceClipProperty = serializedObject.FindProperty("sourceClip");
            useNormalizedTimeProperty = serializedObject.FindProperty("useNormalizedTime");
            sampleTimeProperty = serializedObject.FindProperty("sampleTime");
            includeRootTransformProperty = serializedObject.FindProperty("includeRootTransform");
            bakeScaleProperty = serializedObject.FindProperty("bakeScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(targetAnimatorProperty);
            EditorGUILayout.PropertyField(rigRootProperty);
            EditorGUILayout.PropertyField(sourceClipProperty);
            EditorGUILayout.PropertyField(useNormalizedTimeProperty);

            RigPoseBaker baker = (RigPoseBaker)target;
            AnimationClip clip = baker.SourceClip;

            if (useNormalizedTimeProperty.boolValue)
            {
                float normalizedTime = Mathf.Clamp01(sampleTimeProperty.floatValue);
                normalizedTime = EditorGUILayout.Slider("Normalized Time", normalizedTime, 0f, 1f);
                sampleTimeProperty.floatValue = normalizedTime;

                if (clip != null)
                {
                    EditorGUILayout.LabelField("Resolved Time", $"{normalizedTime * clip.length:0.###} s");
                }
            }
            else
            {
                float maxTime = clip != null ? clip.length : 0f;
                float time = Mathf.Max(0f, sampleTimeProperty.floatValue);

                if (clip != null)
                {
                    time = EditorGUILayout.Slider("Sample Time", time, 0f, maxTime);
                }
                else
                {
                    time = EditorGUILayout.FloatField("Sample Time", time);
                }

                sampleTimeProperty.floatValue = time;
            }

            EditorGUILayout.PropertyField(includeRootTransformProperty);
            EditorGUILayout.PropertyField(bakeScaleProperty);

            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(clip == null))
            {
                if (GUILayout.Button("Bake Pose From Clip"))
                {
                    baker.TryBakePoseFromClip();
                }
            }

            if (clip == null)
            {
                EditorGUILayout.HelpBox("Assign an AnimationClip to sample a pose from.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This samples the clip in the editor, captures each rig transform's local pose, and writes it back onto the current object or prefab instance.",
                    MessageType.None);
            }
        }
    }
}
