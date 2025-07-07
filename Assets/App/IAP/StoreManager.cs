using App.LocalData;
using App.UI.Common;
using GSDev.EventSystem;
using GSDev.Singleton;
using UnityEngine.Purchasing;

namespace App.IAP
{
    public class StoreManager : Singleton<StoreManager>, IEventSender
    {
        EventDispatcher IEventSender.Dispatcher => EventDispatcher.Global;

        public void PurchaseCallBack(
            bool result,
            Product product,
            int code)
        {
            if (result)
            {
                var config = IAPManager.GetStoreConfig(product.definition.id);
                // LocalDataManager.Instance.AddGoldCount(config.DiamondCount);
                // GirlViewLoginManager.Instance.ApplyPay(product);
                CommonMessageTip.Create("Purchase Success");
            }
            else
                CommonMessageTip.Create("Purchase failed");
        }
    }
}

