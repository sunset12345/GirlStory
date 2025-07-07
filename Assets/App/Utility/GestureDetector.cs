using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GestureEvent : UnityEvent<GestureDetector.GestureData> { }

public class GestureDetector : MonoBehaviour
{
    // 手势事件配置
    public enum GestureType { SwipeHorizontal, SwipeVertical, Tap, DoubleTap, LongPress }
    
    [System.Serializable]
    public struct GestureSettings
    {
        [Header("通用设置")]
        [Tooltip("单位：像素")] 
        public float minSwipeDistance;
        [Tooltip("单位：秒")] 
        public float maxGestureTime;
        
        [Header("方向锁定")]
        [Range(0,1)] 
        public float verticalLockThreshold;
        [Range(0,1)] 
        public float horizontalLockThreshold;

        [Header("高级设置")]
        public bool allowMultiTouch;
        public bool enableMouseSimulation;
    }

    // 公开事件
    public GestureEvent OnSwipeLeft;
    public GestureEvent OnSwipeRight;
    public GestureEvent OnSwipeUp;
    public GestureEvent OnSwipeDown;
    public GestureEvent OnTap;
    public GestureEvent OnDoubleTap;
    public GestureEvent OnLongPress;

    // 配置参数
    [SerializeField] 
    private GestureSettings settings = new GestureSettings()
    {
        minSwipeDistance = 50f,
        maxGestureTime = 0.5f,
        verticalLockThreshold = 0.7f,
        horizontalLockThreshold = 0.7f,
        allowMultiTouch = false,
        enableMouseSimulation = true
    };

    // 运行时数据
    private GestureData currentGesture;
    private bool isTracking;
    private float touchStartTime;
    private Vector2 touchStartPosition;
    private int tapCount;

    void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (settings.enableMouseSimulation)
        {
            HandleMouseInput();
        }
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            if (!settings.allowMultiTouch && Input.touchCount > 1) return;

            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartTracking(touch.position);
                    break;
                
                case TouchPhase.Moved:
                    UpdateTracking(touch.position);
                    break;
                
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndTracking(touch.position);
                    break;
            }
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTracking(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateTracking(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndTracking(Input.mousePosition);
        }
    }

    private void StartTracking(Vector2 position)
    {
        isTracking = true;
        touchStartTime = Time.time;
        touchStartPosition = position;
        currentGesture = new GestureData
        {
            startPosition = position,
            startTime = Time.time
        };
    }

    private void UpdateTracking(Vector2 position)
    {
        if (!isTracking) return;

        currentGesture.currentPosition = position;
        currentGesture.delta = position - touchStartPosition;
        currentGesture.duration = Time.time - touchStartTime;

        // 提前触发持续跟踪事件
        if (currentGesture.duration > settings.maxGestureTime)
        {
            EndTracking(position);
        }
    }

    private void EndTracking(Vector2 endPosition)
    {
        if (!isTracking) return;

        isTracking = false;
        currentGesture.endPosition = endPosition;
        currentGesture.duration = Time.time - touchStartTime;

        AnalyzeGesture();
        ResetTracking();
    }

    private void AnalyzeGesture()
    {
        // 计算基础参数
        float distance = Vector2.Distance(currentGesture.startPosition, currentGesture.endPosition);
        Vector2 direction = (currentGesture.endPosition - currentGesture.startPosition).normalized;
        
        // 构建手势数据
        currentGesture.direction = direction;
        currentGesture.velocity = distance / currentGesture.duration;
        currentGesture.gestureType = DetermineGestureType(distance, direction);

        // 触发对应事件
        switch (currentGesture.gestureType)
        {
            case GestureType.SwipeHorizontal:
                if (direction.x > 0) OnSwipeRight?.Invoke(currentGesture);
                else OnSwipeLeft?.Invoke(currentGesture);
                break;
            
            case GestureType.SwipeVertical:
                if (direction.y > 0) OnSwipeUp?.Invoke(currentGesture);
                else OnSwipeDown?.Invoke(currentGesture);
                break;
            
            case GestureType.Tap:
                OnTap?.Invoke(currentGesture);
                break;
            
            case GestureType.DoubleTap:
                OnDoubleTap?.Invoke(currentGesture);
                break;
            
            case GestureType.LongPress:
                OnLongPress?.Invoke(currentGesture);
                break;
        }
    }

    private GestureType DetermineGestureType(float distance, Vector2 direction)
    {
        // 长按检测
        if (currentGesture.duration > 1f)
        {
            return GestureType.LongPress;
        }

        // 滑动检测
        if (distance >= settings.minSwipeDistance)
        {
            bool isHorizontal = Mathf.Abs(direction.x) > settings.horizontalLockThreshold;
            bool isVertical = Mathf.Abs(direction.y) > settings.verticalLockThreshold;

            if (isHorizontal && !isVertical) return GestureType.SwipeHorizontal;
            if (isVertical && !isHorizontal) return GestureType.SwipeVertical;
        }

        // 点击检测
        return (tapCount == 2) ? GestureType.DoubleTap : GestureType.Tap;
    }

    private void ResetTracking()
    {
        StartCoroutine(ResetTapCounter());
    }

    private System.Collections.IEnumerator ResetTapCounter()
    {
        yield return new WaitForSeconds(0.3f);
        tapCount = 0;
    }

    // 手势数据结构
    public struct GestureData
    {
        public Vector2 startPosition;
        public float startTime;
        public Vector2 endPosition;
        public Vector2 currentPosition;
        public Vector2 direction;
        public Vector2 delta;
        public float duration;
        public float velocity;
        public GestureType gestureType;

        public override string ToString()
        {
            return $"{gestureType} | Velocity: {velocity:F1} px/s | Duration: {duration:F2}s";
        }
    }

    // 调试工具
    [Header("Debug")]
    [SerializeField] bool showDebugLogs;

    private void OnValidate()
    {
        if (showDebugLogs && currentGesture.gestureType != null)
        {
            Debug.Log($"Gesture Detected: {currentGesture}");
        }
    }
}