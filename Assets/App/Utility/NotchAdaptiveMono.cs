using System.Globalization;
using UnityEngine;

//刘海屏幕适配
public class NotchAdaptiveMono : MonoBehaviour {
    public bool bUpOffset = true, bDownOffset = false;

    private void Awake() {
        var rectTrans = transform as RectTransform;
        float offUp = 0, offDown = 0;
#if UNITY_EDITOR
        if (UnityEngine.Screen.height / UnityEngine.Screen.width >= 2) {
            offUp = 75;
            offDown = 60;
        }
#else
        offUp = GetOffSetY();
// #if UNITY_IPHONE && !UNITY_EDITOR
//         if (IsIPad()) offDown = 30;
//         else if(offUp != 0) offDown = 60;
// #endif
#endif
        if (bUpOffset && offUp != 0) {
            rectTrans.offsetMax = new Vector2(rectTrans.offsetMax.x, rectTrans.offsetMax.y - offUp);
        }
        if (bDownOffset && offDown != 0) {
            rectTrans.offsetMin = new Vector2(rectTrans.offsetMin.x, rectTrans.offsetMin.y + offDown);
        }
        // MDebug.Log($"=====screen:{Screen.width},{Screen.height}   SafeArea:{Screen.safeArea}");
        // MDebug.Log($"=====safePoint:{safePoint}   yMin:{Screen.safeArea.yMin}  yMax:{Screen.safeArea.yMax}");


        // Vector2 screenPoint1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        // Vector2 safePoint1 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.safeArea.width, Screen.safeArea.height, 0));

        // MDebug.Log($"=====screenPoint1:{screenPoint1}   safePoint1:{safePoint1}");

    }
    public static bool IsIPad() {
#if UNITY_EDITOR
        return ((double)UnityEngine.Screen.height / UnityEngine.Screen.width).ToString(CultureInfo.InvariantCulture).StartsWith("1.33");
#else
		var modelStr = SystemInfo.deviceModel.ToLower ();
		return modelStr.StartsWith ("ipad");
#endif
    }

    public static float GetOffSetY() {

#if UNITY_EDITOR
        if (UnityEngine.Screen.height / UnityEngine.Screen.width >= 2) {
            return 75f;
        }
#endif
        var offsetY = WindowInfo.Instance.NotchHeight;
        
#if UNITY_IPHONE && !UNITY_EDITOR
    		offsetY = offsetY / 2;
#endif
        
        return offsetY;
    }

}