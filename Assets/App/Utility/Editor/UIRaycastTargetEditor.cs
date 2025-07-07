using System.Collections;
using System.Collections.Generic;
using App.UI.Utility;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace App.UI.Utility.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(UIRaycastTarget), false)]
    public class UIRaycastTargetEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(base.m_Script, true);
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}