using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CrispyCube
{
    public class RigPoseBaker : MonoBehaviour
    {
        [SerializeField] Animator targetAnimator;
        [SerializeField] Transform rigRoot;
        [SerializeField] AnimationClip sourceClip;
        [SerializeField] bool useNormalizedTime = true;
        [SerializeField] float sampleTime;
        [SerializeField] bool includeRootTransform = true;
        [SerializeField] bool bakeScale;

        public Animator TargetAnimator => targetAnimator;
        public Transform RigRoot => rigRoot != null ? rigRoot : transform;
        public AnimationClip SourceClip => sourceClip;
        public bool UseNormalizedTime => useNormalizedTime;
        public float SampleTime => sampleTime;
        public bool IncludeRootTransform => includeRootTransform;
        public bool BakeScale => bakeScale;

        public float GetResolvedSampleTime()
        {
            if (sourceClip == null)
            {
                return 0f;
            }

            if (useNormalizedTime)
            {
                return Mathf.Clamp01(sampleTime) * sourceClip.length;
            }

            return Mathf.Clamp(sampleTime, 0f, sourceClip.length);
        }

        public void Reset()
        {
            targetAnimator = GetComponent<Animator>();
            rigRoot = transform;
        }

#if UNITY_EDITOR
        struct LocalPose
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        public bool TryBakePoseFromClip()
        {
            if (sourceClip == null)
            {
                Debug.LogError("RigPoseBaker requires a source clip.", this);
                return false;
            }

            Transform root = RigRoot;
            if (root == null)
            {
                Debug.LogError("RigPoseBaker requires a rig root.", this);
                return false;
            }

            GameObject sampleTarget = targetAnimator != null ? targetAnimator.gameObject : gameObject;
            float resolvedTime = GetResolvedSampleTime();
            List<Transform> rigTransforms = CollectRigTransforms(root, includeRootTransform);
            Dictionary<Transform, LocalPose> sampledPose = new Dictionary<Transform, LocalPose>(rigTransforms.Count);

            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(sampleTarget, sourceClip, resolvedTime);
                AnimationMode.EndSampling();

                foreach (Transform rigTransform in rigTransforms)
                {
                    sampledPose[rigTransform] = new LocalPose
                    {
                        Position = rigTransform.localPosition,
                        Rotation = rigTransform.localRotation,
                        Scale = rigTransform.localScale
                    };
                }
            }
            finally
            {
                if (AnimationMode.InAnimationMode())
                {
                    AnimationMode.StopAnimationMode();
                }
            }

            Undo.RecordObjects(rigTransforms.ToArray(), "Bake Rig Pose From Clip");

            foreach (Transform rigTransform in rigTransforms)
            {
                LocalPose pose = sampledPose[rigTransform];
                rigTransform.localPosition = pose.Position;
                rigTransform.localRotation = pose.Rotation;

                if (bakeScale)
                {
                    rigTransform.localScale = pose.Scale;
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(rigTransform);
                EditorUtility.SetDirty(rigTransform);
            }

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            Debug.Log(
                $"Baked pose from clip '{sourceClip.name}' at {resolvedTime:0.###} seconds onto '{root.name}'.",
                this);

            return true;
        }

        static List<Transform> CollectRigTransforms(Transform root, bool includeRoot)
        {
            List<Transform> transforms = new List<Transform>();

            if (includeRoot)
            {
                transforms.Add(root);
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child != root)
                {
                    transforms.Add(child);
                }
            }

            return transforms;
        }
#endif
    }
}
