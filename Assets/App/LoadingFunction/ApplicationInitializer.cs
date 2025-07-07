using Cysharp.Threading.Tasks;
using GSDev.AssetBundles;
using GSDev.UI.Layer;
using UnityEngine;
using UnityEngine.SceneManagement;
using App.UI.HtmlFunction;
using PushFunction;
using App.UI.LogininFunction;
using App.LocalData;
using App.UI.RegistrationFunction;

namespace App.LoadingFunction
{
    public class ApplicationInitializer : MonoBehaviour
    {
        [SerializeField] private string _assetBundleCode;
        [SerializeField] private string _enterSceneName;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = false;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

#if ENABLE_GM
            DG.Tweening.DOTween.Init(false, false, DG.Tweening.LogBehaviour.Default);
            Debug.unityLogger.logEnabled = true;
            GSDev.CSVConfig.ConfigBase.ErrorLogger = error => Debug.unityLogger.Log(LogType.Error, error);
#endif
            AssetBundleManager.Instance.Init(
                AssetBundleUpdateProcessor.VersionFileRelativePath,
                _assetBundleCode);

            LayerManager.Instance.CreateLayer(
                (int)LayerTag.Loading,
                new DefaultLayerController());
            PushManager.Instance.TriggerTokenRequest();
            LayerManager.Instance.AddLayerLoader((path, layer) =>
            {
                path.ResolveAssetPath(out var bundle, out var asset);
                var prefab = AssetBundleManager.Instance.LoadAsset<GameObject>(bundle, asset);
                Debug.Assert(prefab, $"add layer error:{path}");
                var go = GameObject.Instantiate(
                    prefab,
                    layer.Root,
                    false);
                go.name = prefab.name;
                return go;
            });

            LayerManager.Instance.LoadContent(
                LayerTag.Loading,
                "ui/loading/LoadingLayer");
            ApplicationLoader.StartLoading(FinishLoading);
        }

        private async void FinishLoading()
        {
            await SceneManager.LoadSceneAsync(_enterSceneName);

            HtmlViewManager.Instance.OnLoginFailed += (_) =>
            {
                var playerData = LocalDataManager.Instance.GetPlayerData();
                if (string.IsNullOrEmpty(playerData.Email))
                    RegistrationLayer.Create();
                else
                    LogininLayer.Create();
                LayerManager.Instance.GetLayerController((int)LayerTag.Loading)?.CloseAll();
            };
            if (HtmlViewManager.Instance.Logining)
            {
                var _ = HtmlViewManager.Instance.UploadCoroutine();
            }
            else
                HtmlViewManager.Instance.OnLoginFailed?.Invoke("Login failed, please try again.");
        }
    }

}
