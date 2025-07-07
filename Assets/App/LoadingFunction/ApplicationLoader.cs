using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App.Config;
using App.Extends;
using Cysharp.Threading.Tasks;
using GSDev.AssetBundles;
using GSDev.UI.Layer;
using Unity.Services.Core;
using UnityEngine;
using App.UI.HtmlFunction;
using App.IAP;

namespace App.LoadingFunction
{
    public static class ApplicationLoader
    {
        public static void StartLoading(Action callback)
        {
            var watch = new LoadStepWatch(null);
            var processor = LoadingManager.Instance.CreateLoadingProcess(GetLoaders(watch));
            processor.OnFinish = () =>
            {
                watch.Stop();
                callback?.Invoke();
            };
        }

        private static IEnumerable<IEnumerator<float>> GetLoaders(LoadStepWatch watch)
        {
            Network.HttpManager.Instance.Setup(TimeSpan.FromSeconds(20f));
            yield return InitSDKs(watch);
            yield return LoadPreloadAssetBundles(watch);
            yield return LoadConfig(watch);
            yield return LoadChatRecord(watch);
            yield return InitializeGamingServices(watch); 
            yield return InitIAP(watch);
            yield return Login(watch);
        }

        private static bool _sdkInitialized = false;
        private static IEnumerator<float> InitSDKs(LoadStepWatch watch)
        {
            if (_sdkInitialized)
                yield break;

            using (watch.NewStep(nameof(InitSDKs)))
            {
                _sdkInitialized = true;
            }
        }

        private static void InitLayers()
        {
            LayerManager.Instance.CreateUILayer(LayerTag.Main, new StackLayerController());
            LayerManager.Instance.CreateUILayer(LayerTag.Dialog, new StackLayerController());
            LayerManager.Instance.CreateUILayer(LayerTag.Popup, new StackLayerController());

            LayerManager.Instance.CreateUILayer(LayerTag.Tip, new DefaultLayerController());
            LayerManager.Instance.CreateUILayer(LayerTag.Waiting, new DefaultLayerController());
        }

        private static IEnumerator<float> LoadPreloadAssetBundles(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(LoadPreloadAssetBundles)))
            {
                InitLayers();
                var waiter = LoadPreloadAssetBundlesAsync().ToUniTask().GetAwaiter();
                while (!waiter.IsCompleted)
                    yield return 0.3f;

                yield return 1f;
            }
        }

        private static List<string> GetPreloadBundles()
        {
            var bundles = new List<string>()
            {
                "res/videos",
                // "ui/tmp_shaders",
            };

            bundles.AddRange(AssetBundleManager.Instance.GetAllBundleNames()
                .Where(name => name.Contains("config")));

            return bundles;
        }

        private static IEnumerator LoadPreloadAssetBundlesAsync()
        {
            var bundles = GetPreloadBundles();
            foreach (var bundle in bundles)
            {
                yield return AssetBundleManager.Instance.LoadAssetBundleAsync(bundle, true);
            }
        }

        private static IEnumerator<float> LoadConfig(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(LoadConfig)))
            {
                LoadingManager.Instance.SetInfo("Load config...");
                var task = ConfigManager.Instance.LoadAll();
                while (!task.GetAwaiter().IsCompleted)
                    yield return 0.3f;
                yield return 1f;
            }
        }


        private static IEnumerator<float> LoadChatRecord(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(LoadConfig)))
            {
                // var task = MessageController.Instance.RestoreChatHistory();
                // while (!task.GetAwaiter().IsCompleted)
                //     yield return 0.3f;
                yield return 1f;
            }
        }

        private static IEnumerator<float> InitializeGamingServices(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(InitializeGamingServices)))
            {
                var task = UnityServices.InitializeAsync();
                while (!task.IsCompleted)
                    yield return 0.3f;
                Debug.Log("UnityServices.InitializeAsync() is completed.");
                yield return 1f;
            }
        }

        private static IEnumerator<float> InitIAP(LoadStepWatch watch)
        {
            using (watch.NewStep(nameof(InitIAP)))
            {
                var finished = false;
                IAPManager.Instance.InitCallback = (result, products, msg) => { finished = true; };
                IAPManager.Instance.PurchaseCallback = StoreManager.Instance.PurchaseCallBack;

                //初始化iap 配置
                var configs = ConfigManager.Instance.GetConfig<InappConfigTable>();
                IAPManager.Instance.Init(configs.Rows);

                const float timeOut = 2f;
                var timer = 0f;
                while (timer < timeOut && !finished)
                {
                    timer += UnityEngine.Time.deltaTime;
                    yield return timer / timeOut;
                }

                yield return 1;
            }
        }


        private static IEnumerator<float> Login(LoadStepWatch watch)
        {
            var finfished = false;
            HtmlViewManager.Instance.OnLoginFailed = (_) =>
            {
                finfished = true;
            };
            HtmlViewManager.Instance.OnLoginSuccess = (_) =>
            {
                finfished = true;
            };
            using (watch.NewStep(nameof(Login)))
            {
                var task = HtmlViewManager.Instance.LoginCoroutine();
                while (!finfished)
                    yield return 0.3f;
                yield return 1;
            }
        }
    }
}
