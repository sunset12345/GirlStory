using GSDev.UI.TweenAni;
using UnityEditor;
using UnityEngine;

namespace GSDev.UI
{
    [CustomEditor(typeof(Button), true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : Editor
    {
        // protected void OnEnable()
        // {
        // }
        //
        // public override void OnInspectorGUI()
        // {
        //     base.OnInspectorGUI();
        // }

        [MenuItem("GameObject/CustomUI/Button", false, 0)]
        private static void NewUIButton()
        {
            var obj = new GameObject("Button", typeof(RectTransform));
            obj.AddComponent<Button>();
            obj.AddComponent<ButtonClickBlendableScale>();
            obj.AddComponent<ButtonClickPlayAudio>();

            if (Selection.gameObjects.Length > 0)
            {
                var gameObject = Selection.gameObjects[0];
                obj.transform.SetParent(gameObject.transform, false);
            }

            Selection.activeObject = obj;
        }
    }
}

