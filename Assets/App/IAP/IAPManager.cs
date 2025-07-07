using System;
using System.Collections.Generic;
using System.Linq;
using App.Config;
using App.UI.Common;
using GSDev.Singleton;
using GSDev.UI.Layer;
using UnityEngine;
using UnityEngine.Purchasing;

namespace App.IAP
{
    public class IAPManager : Singleton<IAPManager>, IStoreListener
    {
        public Action<bool, Product[], string> InitCallback;
        public Action<bool, Product, int> PurchaseCallback;
        

        private bool _isInitialized;
        private IStoreController _storeController;
        private IExtensionProvider _storeExtension;

        private LayerContent _waiting;


        #region ================================================= Init ==================================================

        public void Init(Dictionary<int, InappConfig> storeConfigs)
        {
            if (_isInitialized)
                return;

            //标准采购模块
            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            //配置模式
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
    #if UNITY_ANDROID || UNITY_EDITOR
            var name = GooglePlay.Name;
    #elif UNITY_IOS
            var name = AppleAppStore.Name;
    #else
            var name = GooglePlay.Name;
    #endif
            string id;
            foreach (var item in storeConfigs)
            {
                id = GetProductIdById(item.Value);
                if (string.IsNullOrEmpty(id))
                    continue;
                builder.AddProduct(id, (ProductType) item.Value.ProductType, new IDs() {{id, name}});
            }

            UnityPurchasing.Initialize(this, builder);
        }

        //初始化成功
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _storeExtension = extensions;
            _isInitialized = true;
            var products = _storeController.products;
            Debug.Log("IAP init success");
            IAPDebug("init success");
            InitCallback?.Invoke(true, products.all, string.Empty);
        }

        //初始化失败(没有网络的情况下并不会调起，而是一直等到有网络连接再尝试初始化)
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            var er = error.ToString("G");
            Debug.Log($"IAP init fail: {er}");
            IAPDebug($"init fail: {er}");
            InitCallback?.Invoke(false, null, er);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            
        }

        #endregion

        #region ================================================== 购买 ==================================================

        public void Purchase(InappConfig config, bool doubled = false)
        {
            var productId = GetProductIdById(config);
            Purchase(productId, doubled);
        }

        public void Purchase(InappConfig config, int giftId)
        {
            var productId = GetProductIdById(config);
            Purchase(productId, false, giftId);
        }
        
        public bool Purchase(string productId, bool doubled = false, int giftId = 0)
        {
            if (!_isInitialized)
            {
                IAPDebug($"ID:{productId}. Not init.");
                return false;
            }

            if (string.IsNullOrEmpty(productId))
            {
                IAPDebug($"product is null");
                return false;
            }

            Product product = _storeController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                IAPDebug($"ID:{productId}.Not found or is not available for purchase");
                return false;
            }
            // WaitingTip.Open(2f);
            _storeController.InitiatePurchase(productId);
            return true;
        }

        //购买失败
        public void OnPurchaseFailed(Product pro, PurchaseFailureReason p)
        {
            // WaitingTip.Close();
            var er = p.ToString("G");
            IAPDebug($"ID:{pro.definition.id}. purchase fail: {er}");
            PurchaseCallback?.Invoke(false, pro, 0);
        }

        //购买成功
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            // WaitingTip.Close();
            var product = args.purchasedProduct;

    #if !UNITY_EDITOR
            PurchaseCallback?.Invoke(true, product, 1);
            return PurchaseProcessingResult.Complete;
    #else
            //TODO 缺少本地校验
            PurchaseCallback?.Invoke(true, product, 1);
            return PurchaseProcessingResult.Complete;
    #endif
        }
        #endregion

        #region ================================================ 恢复购买 ==================================================

        /// <summary>
        /// 恢复购买
        /// </summary>
        [Obsolete]
        public void RestorePurchases(Action<bool> callBack)
        {
            if (!_isInitialized)
            {
                IAPDebug("RestorePurchases FAIL. Not init.");
                callBack?.Invoke(false);
                return;
            }

    #if UNITY_ANDROID
            IGooglePlayStoreExtensions googlePlayStoreExtensions =
                _storeExtension.GetExtension<IGooglePlayStoreExtensions>();
            googlePlayStoreExtensions.RestoreTransactions((result) =>
            {
                // 返回一个bool值，如果成功，则会多次调用支付回调，然后根据支付回调中的参数得到商品id，最后做处理(ProcessPurchase); 
                IAPDebug($"RestorePurchases result: {result}");
                callBack?.Invoke(result);
            });
    #elif UNITY_IOS
            IAPDebug("RestorePurchases started ...");
            IAppleExtensions apple = _storeExtension.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                // 返回一个bool值，如果成功，则会多次调用支付回调，然后根据支付回调中的参数得到商品id，最后做处理(ProcessPurchase); 
                IAPDebug($"RestorePurchases result: {result}");
                callBack?.Invoke(result);
            });
    #else
            IAPDebug($"RestorePurchases FAIL. Not supported on this platform. Current = {Application.platform}");
            callBack?.Invoke(false);
    #endif
        }

        #endregion


        public static string GetProductIdById(InappConfig config)
        {
    #if UNITY_ANDROID || UNITY_EDITOR
            return config.GoogleProductId;
    #elif UNITY_IOS
            return config.AppleProductId;
    #endif
            return "";
        }

        public static InappConfig GetStoreConfig(string productId)
        {
            var configs = ConfigManager.Instance.GetConfig<InappConfigTable>().Rows.Values.ToList();
    #if UNITY_IOS
            return configs.Find(a => a.AppleProductId == productId);
    #else
            return configs.Find(a => a.GoogleProductId == productId);
    #endif
        }

        public string GetLocalizedPrice(InappConfig storeConfig)
        {
            var productId = GetProductIdById(storeConfig);
            #if UNITY_EDITOR
                return $"${storeConfig.UsDollor}";
            #endif
            if (!_isInitialized)
            {
                IAPDebug($"ID:{productId}. Not init.");
                return $"${storeConfig.UsDollor}";
            }

            var product = _storeController.products.WithID(productId);
            return product.metadata.localizedPriceString;
        }

        public string GetLocalizedPrice(string productId)
        {
            if (!_isInitialized)
            {
                IAPDebug($"ID:{productId}. Not init.");
                return "";
            }

            var product = _storeController.products.WithID(productId);
            return product.metadata.localizedPriceString;
        }

        
        private void IAPDebug(string mes)
        {
            Debug.Log($"IAPManager {mes}");
        }
    }
}