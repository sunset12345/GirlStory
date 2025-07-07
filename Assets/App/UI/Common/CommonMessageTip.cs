using GSDev.AssetBundles;
using GSDev.UI.Layer;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.UI.Common
{
    public class CommonMessageTip : MonoBehaviour
    {
        [SerializeField] private Image backgroundIcon;
        [SerializeField] private TextMeshProUGUI messageTxt;

        private void Initialize(string message)
        {
            messageTxt.text = message;
            messageTxt.DOFade(0, 1, 1).SetLoops(2, LoopType.Yoyo);
            backgroundIcon.DOFade(0, 1, 1).SetLoops(2, LoopType.Yoyo).OnComplete(() => Destroy(gameObject));
        }

        public static void Create(string message)
        {
            var tipPrefab = AssetBundleManager.Instance.LoadAsset<GameObject>("ui/common", "CommonMessageTip");
            var tip = Instantiate(tipPrefab, LayerManager.Instance.RootCanvas.transform, false);
            if(tip)
                tip.GetComponent<CommonMessageTip>().Initialize(message);
        }
    }
}
