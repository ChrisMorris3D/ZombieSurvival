using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrispyCube
{
    [CreateAssetMenu(menuName = "Create Gizmo Settings")]
    public class GizmoColors : ScriptableObject
    {
        public Color chasingActiveColor;
        public Color chasingInactiveColor;

        public Color attackActiveColor;
        public Color attackInactiveColor;
    }
}
