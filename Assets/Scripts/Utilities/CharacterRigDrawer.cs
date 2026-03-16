using UnityEngine;

namespace CrispyCube
{
    public class CharacterRigDrawer : MonoBehaviour
    {
        [SerializeField] Transform rigRoot;
        [SerializeField] Color boneColor = Color.cyan;
        [SerializeField] Color jointColor = Color.yellow;
        [SerializeField] float jointRadius = 0.025f;
        [SerializeField] bool drawOnlyWhenSelected;
        [SerializeField] bool includeInactive = true;

        public Transform RigRoot => rigRoot != null ? rigRoot : transform;
        public Color BoneColor => boneColor;
        public Color JointColor => jointColor;
        public float JointRadius => Mathf.Max(0.001f, jointRadius);
        public bool DrawOnlyWhenSelected => drawOnlyWhenSelected;
        public bool IncludeInactive => includeInactive;

        void Reset()
        {
            rigRoot = transform;
        }

        void OnDrawGizmos()
        {
            if (drawOnlyWhenSelected)
            {
                return;
            }

            DrawRig();
        }

        void OnDrawGizmosSelected()
        {
            if (!drawOnlyWhenSelected)
            {
                return;
            }

            DrawRig();
        }

        void DrawRig()
        {
            Transform root = RigRoot;
            if (root == null)
            {
                return;
            }

            DrawJointHierarchy(root);
        }

        void DrawJointHierarchy(Transform joint)
        {
            if (joint == null || (!includeInactive && !joint.gameObject.activeInHierarchy))
            {
                return;
            }

            foreach (Transform child in joint)
            {
                if (child == null || (!includeInactive && !child.gameObject.activeInHierarchy))
                {
                    continue;
                }

                Gizmos.color = boneColor;
                Gizmos.DrawLine(joint.position, child.position);
                DrawJointHierarchy(child);
            }
        }
    }
}
