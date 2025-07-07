using GSDev.Singleton;
using GSDev.UI.Layer;
using UnityEngine;
using UnityEngine.UI;

public class WindowInfo : Singleton<WindowInfo>
{
    public readonly float Width, WidthHalf;
    public readonly float Height, HeightHalf;
    public readonly CanvasScaler CanvasScaler;
    public readonly WindowScale Scale;

    public readonly float NotchHeight;

    public WindowInfo()
    {
        Scale = new WindowScale();
        CanvasScaler = LayerManager.Instance.GetComponent<CanvasScaler>();
        var size = CanvasScaler.Rect().size;
        Width = size.x;
        Height = size.y;
        WidthHalf = Width * 0.5f;
        HeightHalf = Height * 0.5f;
        
        Scale.X = Width / CanvasScaler.referenceResolution.x;
        Scale.Y = Height / CanvasScaler.referenceResolution.y;
        Scale.Min = Mathf.Min(Scale.X, Scale.Y);
        Scale.Max = Mathf.Max(Scale.X, Scale.Y);
        
        NotchHeight = Height * (Screen.height - Screen.safeArea.height) / Screen.height;
    }
}

public class WindowScale
{
    public float X, Y, Min, Max;
}

