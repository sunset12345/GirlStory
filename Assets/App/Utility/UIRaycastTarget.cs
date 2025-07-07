using UnityEngine;
using UnityEngine.UI;

namespace App.UI.Utility
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIRaycastTarget : Graphic
    {
        public override void SetMaterialDirty() { return; }
        // public override void SetVerticesDirty() { return; }
    }
}
