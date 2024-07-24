using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexAds : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ShowAdsExtern(int makeAuth);
    [DllImport("__Internal")]
    private static extern void ShowAdvExtern();

    private bool canShowAds = true;

    private ProductCoin currentProductCoin;

    public void ShowAds(int makeAuth = 0)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (canShowAds)
            {
                ShowAdsExtern(makeAuth);
                canShowAds = false;
                StartCoroutine(RefreshAds());
            }
        }
    }

    public void ShowAdv(ProductCoin productCoin)
    {
        if (productCoin != null)
        {
            if (productCoin.CanPickup)
            {
                currentProductCoin = productCoin;
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    ShowAdvExtern();
                }
                else
                    AddCoins(1);
            }
            else
            {
                GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.daily"), null, true);
            }
        }
    }

    public void AddCoins(int add)
    {
        if (add == 1)
        {
            currentProductCoin.Buy();
            GameManager.Instance.PlayerManager.SaveData();
        }
        else
        {
            Debug.Log("Error, coins not added!");
            GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.error"), null, true);
            // Если произошла ошибка при воспроизведении рекламы
        }

        currentProductCoin = null;
    }

    private IEnumerator RefreshAds()
    {
        yield return new WaitForSeconds(60);
        canShowAds = true;
    }
}