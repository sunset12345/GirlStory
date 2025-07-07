using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class AutoDisableRaycastTarget
{
    static AutoDisableRaycastTarget()
    {
        // 注册组件添加事件
        ObjectFactory.componentWasAdded += OnComponentAdded;
    }

    private static void OnComponentAdded(Component component)
    {
        if (component is Image image && image.raycastTarget)
        {
            image.raycastTarget = false;
            EditorUtility.SetDirty(image);
        }
        else if (component is TextMeshProUGUI tmp && tmp.raycastTarget)
        {
            tmp.raycastTarget = false;
            EditorUtility.SetDirty(tmp);
        }
    }
}