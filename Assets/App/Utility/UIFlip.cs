using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI.Utility
{
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Effects/Flip")]
    public class UIFlip : BaseMeshEffect
    {
        [SerializeField]
        private bool _horizontal = false;

        [SerializeField]
        private bool _veritical = false;

        public bool horizontal
        {
            get => _horizontal;
            set
            {
                _horizontal = value;
                graphic.SetVerticesDirty();
            }
        }

        public bool vertical
        {
            get => _veritical;
            set
            {
                _veritical = value;
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            RectTransform rt = transform as RectTransform;

            UIVertex uiVertex = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; ++i)
            {
                vh.PopulateUIVertex(ref uiVertex, i);

                // Modify positions
                uiVertex.position = new Vector3(
                    (_horizontal ? rt.rect.center.x * 2 - uiVertex.position.x : uiVertex.position.x),
                    (_veritical ? rt.rect.center.y * 2 - uiVertex.position.y : uiVertex.position.y),
                    uiVertex.position.z);

                // Apply
                vh.SetUIVertex(uiVertex, i);
            }
        }
    }
}