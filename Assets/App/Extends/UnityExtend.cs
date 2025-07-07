using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public static class UnityExtend
{
    public static T GetOrCreateComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }
    
    public static RectTransform AsRT(this Component self) => self.transform as RectTransform;
    public static Rect Rect(this Component self) => ((RectTransform) self.transform).rect;
    
    public static void SetPivot(this RectTransform self, float x, float y)
    {
        var pivot = self.pivot;
        pivot.x = x;
        pivot.y = y;
        self.pivot = pivot;
    }
    public static void SetPivot(this Transform self, float x, float y)
    {
        SetPivot(self as RectTransform, x, y);
    }

    #region 设置大小

    public static void SetWidth(this RectTransform self, float width)
    {
        var size = self.sizeDelta;
        size.x = width;
        self.sizeDelta = size;
    }
    public static void SetWidth(this Transform self, float width)
    {
        SetWidth(self as RectTransform, width);
    }

    public static void SetHeight(this RectTransform self, float height)
    {
        var size = self.sizeDelta;
        size.y = height;
        self.sizeDelta = size;
    }
    public static void SetHeight(this Transform self, float height)
    {
        SetHeight(self as RectTransform, height);
    }

    public static void SetSize(this RectTransform self, float width, float height)
    {
        var sizeDelta = self.sizeDelta;
        sizeDelta.x = width;
        sizeDelta.y = height;
        self.sizeDelta = sizeDelta;
    }
    public static void SetSize(this Transform self, float width, float height)
    {
        SetSize(self as RectTransform, width, height);
    }

    public static void SetSize(this RectTransform self, float size)
    {
        var sizeDelta = self.sizeDelta;
        sizeDelta.x = size;
        sizeDelta.y = size;
        self.sizeDelta = sizeDelta;
    }
    public static void SetSize(this Transform self, float size)
    {
        SetSize(self as RectTransform, size);
    }

    #endregion

    #region 设置位置

    public static void SetLocPos(this Transform self, float x, float y, float z)
    {
        var pos = self.localPosition;
        pos.x = x;
        pos.y = y;
        pos.z = z;
        self.localPosition = pos;
    }

    public static void SetLocPos(this Transform self, float x, float y)
    {
        var pos = self.localPosition;
        pos.x = x;
        pos.y = y;
        self.localPosition = pos;
    }

    public static void SetLocPos(this Transform self, Vector2 vec2)
    {
        var pos = self.localPosition;
        pos.x = vec2.x;
        pos.y = vec2.y;
        self.localPosition = pos;
    }

    public static void SetLocPosX(this Transform self, float x)
    {
        var pos = self.localPosition;
        pos.x = x;
        self.localPosition = pos;
    }

    public static void Set3DLocPosX(this Transform self, float x)
    {
        var pos = self.localPosition;
        pos.x = x / 100;
        self.localPosition = pos;
    }

    public static void SetLocPosY(this Transform self, float y)
    {
        var pos = self.localPosition;
        pos.y = y;
        self.localPosition = pos;
    }
    
    public static void SetLocPosZ(this Transform self, float z)
    {
        var pos = self.localPosition;
        pos.z = z;
        self.localPosition = pos;
    }

    public static void SetPos(this Transform self, float x, float y)
    {
        var pos = self.position;
        pos.x = x;
        pos.y = y;
        self.position = pos;
    }
    public static void SetPosX(this Transform self, float x)
    {
        var pos = self.position;
        pos.x = x;
        self.position = pos;
    }
    public static void SetPosY(this Transform self, float y)
    {
        var pos = self.position;
        pos.y = y;
        self.position = pos;
    }
    public static void SetPosZ(this Transform self, float z)
    {
        var pos = self.position;
        pos.z = z;
        self.position = pos;
    }

    public static void SetAnchorPos(this RectTransform self, float x, float y)
    {
        Vector3 pos = self.anchoredPosition;
        pos.x = x;
        pos.y = y;
        self.anchoredPosition = pos;
    }
    public static void SetAnchorPos(this Transform self, float x, float y)
    {
        SetAnchorPos(self as RectTransform, x, y);
    }

    public static void SetAnchorPos(this RectTransform self, Vector2 vec2)
    {
        Vector3 pos = self.anchoredPosition;
        pos.x = vec2.x;
        pos.y = vec2.y;
        self.anchoredPosition = pos;
    }
    public static void SetAnchorPos(this Transform self, Vector2 vec2)
    {
        SetAnchorPos(self as RectTransform, vec2);
    }

    public static void SetAnchorPosX(this RectTransform self, float x)
    {
        Vector3 pos = self.anchoredPosition;
        pos.x = x;
        self.anchoredPosition = pos;
    }
    public static void SetAnchorPosX(this Transform self, float x)
    {
        SetAnchorPosX(self as RectTransform, x);
    }

    public static void SetAnchorPosY(this RectTransform self, float y)
    {
        Vector3 pos = self.anchoredPosition;
        pos.y = y;
        self.anchoredPosition = pos;
    }
    public static void SetAnchorPosY(this Transform self, float y)
    {
        SetAnchorPosY(self as RectTransform, y);
    }

    #endregion

    #region 设置旋转 放大

    public static void SetRotationAroundZ(this Transform self, float z)
    {
        self.rotation = Quaternion.Euler(0, 0, z);
    }

    public static void SetLocalRotationAroundZ(this Transform self, float z)
    {
        self.localRotation = Quaternion.Euler(0, 0, z);
    }

    public static void SetScale(this Transform self, float v)
    {
        var scale = self.localScale;
        scale.x = v;
        scale.y = v;
        self.localScale = scale;
    }

    public static void SetScaleX(this Transform self, float x)
    {
        var scale = self.localScale;
        scale.x = x;
        self.localScale = scale;
    }
    public static void SetScaleY(this Transform self, float y)
    {
        var scale = self.localScale;
        scale.y = y;
        self.localScale = scale;
    }

    public static void SetScale3(this Transform self, float v)
    {
        var scale = self.localScale;
        scale.x = v;
        scale.y = v;
        scale.z = v;
        self.localScale = scale;
    }
    
    public static void SetScale3(this Transform self, float x, float y, float z)
    {
        var scale = self.localScale;
        scale.x = x;
        scale.y = y;
        scale.z = z;
        self.localScale = scale;
    }

    #endregion

    #region 设置 颜色

    public static void SetAlpha(this Graphic self, float alpha)
    {
        var color = self.color;
        color.a = alpha;
        self.color = color;
    }
    public static void SetAlpha(this Graphic self, int alpha)
    {
        var color = self.color;
        color.a = alpha / 255f;
        self.color = color;
    }
    public static void SetAlpha(this SpriteRenderer self, float alpha)
    {
        var color = self.color;
        color.a = alpha;
        self.color = color;
    }
    public static void SetAlpha(this SpriteRenderer self, int alpha)
    {
        var color = self.color;
        color.a = alpha / 255f;
        self.color = color;
    }

    public static void SetColors(this Transform self, Color color)
    {
        var graphics = self.GetComponentsInChildren<Graphic>();
        foreach (var g in graphics)
        {
            g.color = color;
        }
    }

    public static void SetShadowColors(this Text self, Color color)
    {
        var shadows = self.GetComponentsInChildren<Shadow>();
        foreach (var g in shadows)
        {
            g.effectColor = color;
        }
    }

    #endregion

    #region 查找

    public static T FindC<T>(this Component self, string name)
    {
        return self.transform.Find(name).GetComponent<T>();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public static T FindC<T>(this Transform self, string name)
    {
        return self.Find(name).GetComponent<T>();
    }

    public static T FindC<T>(string path)
    {
        return GameObject.Find(path).GetComponent<T>();
    }

    public static Transform Find(this Component self, string name)
    {
        return self.transform.Find(name);
    }
    
    /// <summary>
    /// 得到组建
    /// </summary>
    /// <param name="self"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetC<T>(this Component self) where T : Component 
    {
        var t = self.GetComponent<T>();
        if (!t) Debug.LogError($"NAME:{self.name} no has component {t.GetType()}");
        return t;
    }

    #endregion

    #region 销毁
    
    public static void DestroySelf(this Component self)
    {
        self.DOKill();
        Object.Destroy(self.gameObject);
    }

    public static void DestroysInChildren(this Transform self)
    {
        foreach (Transform tf in self)
        {
            tf.DestroySelf();
        }
    }

    #endregion

    #region 显示 激活

    public static bool IsActive(this Component self) {
        return self.gameObject.activeSelf;
    }

    public static void ShowSelf(this Component self)
    {
        self.gameObject.SetActive(true);
    }

    public static void HideSelf(this Component self)
    {
        self.gameObject.SetActive(false);
    }

    public static void SetActive(this Transform self, bool active)
    {
        self.gameObject.SetActive(active);
    }

    #endregion

    #region 设置 Layer

    public static void SetLayers(this GameObject self, int layer)
    {
        foreach (var obj in self.GetComponentsInChildren<Transform>()) obj.gameObject.layer = layer;
    }
    
    public static void SetLayers(this Transform self, int layer)
    {
        foreach (var obj in self.GetComponentsInChildren<Transform>()) obj.gameObject.layer = layer;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public static void SetLayers(this GameObject self, string layerName)
    {
        var layer = LayerMask.NameToLayer(layerName);
        self.SetLayers(layer);
    }
    #endregion

    public static Transform[] GetChildArray(this Transform self)
    {
        var count = self.childCount;
        var children = new Transform[count];
        var index = 0;
        foreach (Transform item in self)
        {
            children[index] = item;
            index += 1;
        }

        return children;
    }

    /// ///////////////////////////////////////////////////////////////
    public static bool IsNull(this Object self) => !self;
    public static bool IsNoNull(this Object self) => self;

    // public static bool IsNull<T>(this T self) where T: class => self == null;
    // public static bool IsNoNull<T>(this T self) where T: class => self != null;

    /// ///////////////////////////////////////////////////////////////
    public static Vector3 ConvertToWorld(this Component self)
    {
        var position = self.transform.position;
        Vector3 ptScreen = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
        ptScreen.z = Mathf.Abs(Camera.main.transform.position.z - position.z);
        return Camera.main.ScreenToWorldPoint(ptScreen);
    }

    public static Vector3 ConvertToWorld(this Vector3 pos)
    {
        Vector3 ptScreen = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
        ptScreen.z = Mathf.Abs(Camera.main.transform.position.z - pos.z);
        return Camera.main.ScreenToWorldPoint(ptScreen);
    }
    
    public static Vector3 CameraToUIPos(this Component self, Camera _camera) {
        var pos = _camera.WorldToViewportPoint(self.transform.position);
        pos = Camera.main.ViewportToWorldPoint(pos);
        return pos;
    }
    
    public static Vector3 CameraToUIPos(this Vector3 self, Camera _camera) {
        var pos = _camera.WorldToViewportPoint(self);
        pos = Camera.main.ViewportToWorldPoint(pos);
        return pos;
    }
    /// ///////////////////////////////////////////////////////////////
    /// <summary>
    /// A、B两个点，求B相对A的角度
    /// B.ToAngle(A.position);
    /// </summary>
    /// <param name="self"></param>
    /// <param name="pos">世界坐标</param>
    /// <returns>正上方为0，逆时针</returns>
    public static float ToAngle(this Transform self, Vector2 pos)
    {
        pos = self.InverseTransformPoint(pos);
        var angle = Mathf.Atan(pos.y / pos.x) * Mathf.Rad2Deg;
        angle += (pos.x >= 0.0f ? 90.0f : 270.0f);
        return angle;
    }

    public static float GetDistance(this Transform one, Transform two)
    {
        return Vector3.Distance(one.position, two.position);
    }

    public static string GetFullName(this Transform self)
    {
        var names = new List<string>();
        var tf = self;
        while (tf)
        {
            names.Add(tf.name);
            tf = tf.parent;
        }

        names.Reverse();
        var fullname = names.Aggregate("", (current, s) => current + (s + "."));
        return fullname.Substring(0, fullname.Length - 1).Replace("Canvas (Environment).", "");
    }
}