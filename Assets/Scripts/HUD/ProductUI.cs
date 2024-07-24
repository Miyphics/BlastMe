using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductUI : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI priceText;
    [SerializeField] protected Image productImage;
    [SerializeField] protected GameObject coinObject;
    private Button button;

    private Product product;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() =>
        {
            if (!GameManager.Instance.HudManager.IsAnimated && GameManager.Instance.HudManager.ConfirmationWindow.Hidden)
            {
                // Balls and bombs
                if (product is ProductBall prodBall)
                {
                    var playerMng = GameManager.Instance.PlayerManager;

                    if (!prodBall.Purchased)
                    {
                        if (GameManager.Instance.PlayerManager.Coins >= product.Price)
                        {
                            GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.buySkin"), () =>
                            {
                                GameManager.Instance.PlayerManager.BuyBall(prodBall);
                                GameManager.Instance.HudManager.MenuHud.ShopPanel.RefreshCategory();
                            });
                        }
                    }
                    else
                    {
                        bool canUse = true;

                        switch (prodBall.BallType)
                        {
                            case BallType.Bomb:
                                if (playerMng.SelectedBomb == product)
                                {
                                    canUse = false;
                                }
                                break;
                            
                            case BallType.Ball:
                            default:
                                if (playerMng.SelectedBall == product)
                                {
                                    canUse = false;
                                }
                                break;
                        }

                        if (canUse)
                        {
                            GameManager.Instance.HudManager.ConfirmationWindow.SetInfo(LocalizationManager.Instance.GetLocalizedText("info.doChangeSkin"), () =>
                            {
                                GameManager.Instance.PlayerManager.ChangeBallSkin(prodBall);
                                GameManager.Instance.HudManager.MenuHud.ShopPanel.RefreshCategory();
                            });
                        }
                    }
                }

                // Coins
                else if (product is ProductCoin prodCoin)
                {
                    Yandex.Instance.YandexAds.ShowAdv(prodCoin);
                    button.interactable = false;
                    StartCoroutine(ToggleButtonInteractIE(true, 1f));
                }
            }
        });
    }

    public void SetProduct(Product product)
    {
        this.product = product;

        SetImage(product.Sprite);
        productImage.color = Color.white;

        UpdatePriceText();
    }

    public void SetProduct(ProductBall productBall)
    {
        SetProduct((Product)productBall);

        productImage.color = productBall.Color;
    }

    protected void UpdatePriceText()
    {
        if ((product is ProductBall prodBall && !prodBall.Purchased) || (product is ProductCoin))
        {
            priceText.text = Helpers.GetShortPrice(product.Price);

            if (product.SecondSprite != null)
                coinObject.GetComponent<Image>().sprite = product.SecondSprite;
            
            if (!coinObject.activeSelf)
                coinObject.SetActive(true);
        }
        else
        {
            string prText = LocalizationManager.Instance.GetLocalizedText("Purchased");

            if (product is ProductBall pBall)
            {
                switch (pBall.BallType)
                {
                    case BallType.Bomb:
                        if (GameManager.Instance.PlayerManager.SelectedBomb == product)
                        {
                            prText = LocalizationManager.Instance.GetLocalizedText("Selected");
                        }
                        break;

                    case BallType.Ball:
                    default:
                        if (GameManager.Instance.PlayerManager.SelectedBall == product)
                        {
                            prText = LocalizationManager.Instance.GetLocalizedText("Selected");
                        }
                        break;
                }
            }

            priceText.text = prText;

            if (coinObject.activeSelf)
                coinObject.SetActive(false);
        }
    }

    protected IEnumerator ToggleButtonInteractIE(bool enable, float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        button.interactable = enable;
    }

    protected void SetImage(Sprite sprite)
    {
        var ratComp = productImage.GetComponent<AspectRatioFitter>();
        ratComp.aspectMode = AspectRatioFitter.AspectMode.None;

        productImage.sprite = sprite;

        var spWidth = sprite.rect.width;
        var spHeight = sprite.rect.height;

        if (spWidth != spHeight)
        {
            if (spWidth > spHeight)
            {
                ratComp.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                
            }
            else if (spHeight > spWidth)
            {
                ratComp.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
            }

            ratComp.aspectRatio = spWidth / spHeight;
        }
        

        productImage.rectTransform.anchorMin = new(0, 0);
        productImage.rectTransform.anchorMax = new(1, 1);

        productImage.rectTransform.offsetMax = new(0, 0);
        productImage.rectTransform.offsetMin = new(0, 0);
    }
}
