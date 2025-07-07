using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using AOT;
using App.UI.Common;
using GSDev.EventSystem;
using GSDev.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace App.IosFunction
{
    public class IOSPhotoManager : MonoSingleton<IOSPhotoManager>, IEventSender
    {
        public EventDispatcher Dispatcher => EventDispatcher.Global;

        // 定义回调委托
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SaveCallback(IntPtr context, string result);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PickCallback(IntPtr context, string base64Image);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SelectAvatarCallback(IntPtr context, string base64Image);

        // 静态委托实例
        private static SaveCallback _staticSaveCallback;
        private static SelectAvatarCallback _staticSelectAvatarCallback;
        private static PickCallback _staticPickCallback;

        // GCHandle 用于保持对象引用
        private static GCHandle _gcHandle;

        protected override void Init()
        {
            base.Init();
            // 初始化静态委托和GCHandle
            _staticSaveCallback = new SaveCallback(OnSaveResult);
            _staticPickCallback = new PickCallback(OnPickResult);
            _staticSelectAvatarCallback = new SelectAvatarCallback(OnSelectAvatarResult);
            _gcHandle = GCHandle.Alloc(this);
        }

        // 注册原生方法
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _NativeGallery_SaveImage(string filePath, SaveCallback callback, IntPtr context);

    [DllImport("__Internal")]
    private static extern void _NativeGallery_PickImage(bool allowEditing, PickCallback callback, IntPtr context);

    [DllImport("__Internal")]
    private static extern void _NativeGallery_PickImage(bool allowEditing, SelectAvatarCallback callback, IntPtr context);
#endif

        //---------------- 静态回调方法（带上下文）----------------
        [MonoPInvokeCallback(typeof(SaveCallback))]
        private static void OnSaveResult(IntPtr context, string result)
        {
            var instance = (IOSPhotoManager)GCHandle.FromIntPtr(context).Target;
            instance.HandleSaveResult(result);
        }

        [MonoPInvokeCallback(typeof(PickCallback))]
        private static void OnPickResult(IntPtr context, string base64Image)
        {
            var instance = (IOSPhotoManager)GCHandle.FromIntPtr(context).Target;
            instance.HandlePickResult(base64Image);
        }

        [MonoPInvokeCallback(typeof(SelectAvatarCallback))]
        private static void OnSelectAvatarResult(IntPtr context, string base64Image)
        {
            var instance = (IOSPhotoManager)GCHandle.FromIntPtr(context).Target;
            instance.HandleSelectAvatarResult(base64Image);
        }

        private void HandleSaveResult(string result)
        {
            Debug.Log($"保存结果: {result}");
            if (result == "success")
            {
                CommonMessageTip.Create("Save successfully");
            }
        }

        private void HandlePickResult(string base64Image)
        {
            if (!string.IsNullOrEmpty(base64Image))
            {
                OnImagePicked(base64Image, false);
            }
        }

        private void HandleSelectAvatarResult(string base64Image)
        {
            if (!string.IsNullOrEmpty(base64Image))
            {
                OnImagePicked(base64Image);
            }
        }


        //---------------- 从相册选择图片 -----------------

        // 按钮点击触发选择
        public void OnSelectAvatarButtonClick()
        {
#if UNITY_IOS && !UNITY_EDITOR
        IntPtr context = GCHandle.ToIntPtr(_gcHandle);
        _NativeGallery_PickImage(true, _staticSelectAvatarCallback, context);
#else
            Debug.LogWarning("PickImage is only supported on iOS.");
#endif
        }

        public void PickImage()
        {
#if UNITY_IOS && !UNITY_EDITOR
        IntPtr context = GCHandle.ToIntPtr(_gcHandle);
        _NativeGallery_PickImage(true, _staticPickCallback, context);
#else
            Debug.LogWarning("PickImage is only supported on iOS.");
#endif
        }

        #region 从手机读取头像
        public static event Action<Texture2D> OnAvatarUpdated;

        // 接收图片数据的回调方法
        private void OnImagePicked(string base64Image, bool isAvatar = true)
        {
            byte[] imageData = Convert.FromBase64String(base64Image);
            StartCoroutine(ProcessImage(imageData, isAvatar));
        }

        private IEnumerator ProcessImage(byte[] data, bool isAvatar)
        {
            // 创建Texture
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(data);

            // 等比缩放至最大512px
            int maxSize = 512;
            if (texture.width > maxSize || texture.height > maxSize)
            {
                float scale = Mathf.Min(
                    (float)maxSize / texture.width,
                    (float)maxSize / texture.height
                );

                ScaleWithRenderTexture(texture,
                    Mathf.FloorToInt(texture.width * scale),
                    Mathf.FloorToInt(texture.height * scale)
                );
            }

            var pngName = isAvatar ? "avatar.png" : DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
            // 保存到本地
            string savePath = Path.Combine(Application.persistentDataPath, pngName);
            File.WriteAllBytes(savePath, texture.EncodeToPNG());

            // 更新显示
            if (isAvatar)
                OnAvatarUpdated?.Invoke(texture);
            else
            {
                // 这里可以添加其他处理逻辑，比如更新UI等
                // 例如：SetTextureToImage(texture, targetImage);
                this.DispatchEvent(Witness<OnPhotoSelectEvent>._, texture, savePath);
            }

            yield return null;
        }

        Texture2D ScaleWithRenderTexture(Texture2D source, int width, int height)
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);

            Texture2D result = new Texture2D(width, height);
            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        public void SetTextureToImage(Texture2D texture, Image targetImage)
        {
            // Step 1: 创建Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), // 纹理范围
                new Vector2(0.5f, 0.5f), // 轴心点（居中）
                pixelsPerUnit: 100 // 每单位像素数
            );

            // Step 2: 设置给Image组件
            targetImage.sprite = sprite;

            // Step 3: 调整Image显示（可选）
            targetImage.preserveAspect = true; // 保持宽高比
        }

        // 加载本地保存的头像
        public static Texture2D LoadSavedAvatar()
        {
            string path = Path.Combine(Application.persistentDataPath, "avatar.png");
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                return tex;
            }
            return null;
        }

        public static Texture2D LoadSavedPhoto(string path)
        {
            if (File.Exists(path))
            {
                byte[] data = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                return tex;
            }
            return null;
        }

        #endregion


        //---------------- 保存图片到相册 -----------------
        public void SaveImage(string filePath)
        {
#if UNITY_IOS && !UNITY_EDITOR
        IntPtr context = GCHandle.ToIntPtr(_gcHandle);
        _NativeGallery_SaveImage(filePath, _staticSaveCallback, context);
#else
            Debug.LogWarning("SaveImage is only supported on iOS.");
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_gcHandle.IsAllocated)
            {
                _gcHandle.Free();
            }
        }
    }

    public class OnPhotoSelectEvent : EventBase<Texture2D, string>
    {
        public Texture2D Texture => Field1;
        public string FilePath => Field2;
    }
}
