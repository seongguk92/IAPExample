using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public const string ProductHeart = "heart";//Consumable
    public const string ProductCharacterSkin = "character_skin";//UnConsumable
    public const string ProductSubscription = "premium_subscription";//Subscription

    private const string _iOS_HeartId = "com.studio.app.heart";
    private const string _android_HeartId = "com.studio.app.heart";

    private const string _iOS_SkinId = "com.studio.app.skin";
    private const string _android_SkinId = "com.studio.app.skin";

    private const string _iOS_PremiumSub = "com.studio.app.sub";
    private const string _android_PremiumSub = "com.studio.app.sub";

    private static IAPManager m_instance;

    public static IAPManager Instance
    {
        get
        {
            if (m_instance != null) return m_instance;

            m_instance = FindObjectOfType<IAPManager>();

            if (m_instance == null) m_instance = new GameObject("IAP Manager").AddComponent<IAPManager>();
            return m_instance;
        }
    }

    private IStoreController storeController; // 구매 과정을 제어하는 함수 제공
    private IExtensionProvider storeExtensionProvider; // 여러 플랫폼을 위한 확장 처리를 제공

    public bool isInitialized => storeController != null && storeExtensionProvider != null;
    void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        InitUnityIAP();
    }

    private void InitUnityIAP()
    {
        if (isInitialized) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance()); // 유니티 기본 스토어 설정 가지고 생성
        builder.AddProduct(
                ProductHeart, ProductType.Consumable,
                new IDs()
                {
                    {_iOS_HeartId, AppleAppStore.Name },
                    {_android_HeartId, GooglePlay.Name }
                }
            );
        builder.AddProduct(
                ProductCharacterSkin, ProductType.NonConsumable,
                new IDs()
                {
                    {_iOS_SkinId, AppleAppStore.Name },
                    {_android_SkinId, GooglePlay.Name }
                }
            );
        builder.AddProduct(
                ProductSubscription, ProductType.Subscription,
                new IDs()
                {
                    {_iOS_PremiumSub, AppleAppStore.Name },
                    {_android_PremiumSub, GooglePlay.Name }
                }
            );
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        //UnityPurchasing.Initialize(this, builder); 끝난 뒤 실행
        Debug.Log("유니티 IAP 초기화 성공");
        storeController = controller;
        storeExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"유니티 IAP 초기화 실패 {error}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        Debug.Log($"구매 성공 - ID : {args.purchasedProduct.definition.id}");
        if(args.purchasedProduct.definition.id == ProductHeart)
        {
            Debug.Log("골드 상승 처리...");
        }
        else if (args.purchasedProduct.definition.id == ProductCharacterSkin)
        {
            Debug.Log("스킨 등록...");
        }
        else if (args.purchasedProduct.definition.id == ProductSubscription)
        {
            Debug.Log("구독 서비스 시작...");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"구매 실패 - {product.definition.id}, {reason}");
    }

    /// <summary>
    /// 상품 구매 시도
    /// </summary>
    public void Purchase(string productId)
    {
        if (!isInitialized) return;
        var product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"구매 시도 - {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log($"구매 시도 불가 - {productId}"); 
        }
    }

    /// <summary>
    /// 이전 구매 복구
    /// </summary>
    public void RestorePurchase()
    {
        if (!isInitialized) return;
        if(Application.platform == RuntimePlatform.IPhonePlayer 
            || Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("구매 복구 시도");
            
            var appleExt = storeExtensionProvider.GetExtension<IAppleExtensions>();
            appleExt.RestoreTransactions(
                    result => Debug.Log($"구매 복구 시도 결과 - {result}")
                ); ;
        }
    }

    /// <summary>
    /// 구매 하고 있는 아이템 인지 
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public bool HadPurchased(string productId)
    {
        if (!isInitialized) return false;

        var product = storeController.products.WithID(productId);
        if( product != null)
        {
            return product.hasReceipt;
        }

        return false;
    }
}
