using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopHud : MonoBehaviour, IAnimatedHud
{
    [SerializeField] private RectTransform verticalPanel;
    [SerializeField] private RectTransform productsPanel;

    [Header("Buttons")]
    [SerializeField] private AnimatedButton buttonBalls;
    [SerializeField] private AnimatedButton buttonBombs;
    [SerializeField] private AnimatedButton buttonCoins;
    [SerializeField] private AnimatedButton buttonPurchased;

    [Space]
    [SerializeField] private TextMeshProUGUI coinsText;

    private Color defaultBackgroundColor;
    private Vector2 defaultVerticalPanelPos;
    private Vector2 defaultVerticalPanelSize;

    private ProductsCategory selectedCategoty = ProductsCategory.Balls;

    private void Awake()
    {
        defaultBackgroundColor = transform.GetChild(0).GetComponent<Image>().color;

        defaultVerticalPanelPos = verticalPanel.anchoredPosition;
        defaultVerticalPanelSize = verticalPanel.sizeDelta;

        buttonBalls.onClick.AddListener(() => ChangeCategory(ProductsCategory.Balls));
        buttonBombs.onClick.AddListener(() => ChangeCategory(ProductsCategory.Bombs));
        buttonCoins.onClick.AddListener(() => ChangeCategory(ProductsCategory.Coins));
        buttonPurchased.onClick.AddListener(() => ChangeCategory(ProductsCategory.Purchased));
    }

    private void Start()
    {
        RefreshCategory();
        UpdateCoins();

        HudManager.ToggleAllChilds(transform, false);
        ToggleAnim(false, true);
    }

    public void ToggleAnim(bool enable, bool force = false)
    {
        ToggleAnim(enable, null, force);
    }

    public void ToggleAnim(bool enable, Action actionAfterAnim, bool force = false)
    {
        if (!GameManager.Instance.HudManager.IsAnimated || force)
        {
            List<HudManager.RectTransformAnimated> anims;

            var background = transform.GetChild(0).GetComponent<RectTransform>();
            var defCol = defaultBackgroundColor;
            var animSpeed = GameManager.Instance.HudManager.HudAnimSpeed;

            LeanTween.cancel(background);

            if (enable)
            {
                UpdateCoins();

                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = verticalPanel, position = defaultVerticalPanelPos, size = defaultVerticalPanelSize },
                };

                LeanTween.color(background, defCol, animSpeed).setIgnoreTimeScale(true);
            }
            else
            {
                anims = new()
                {
                    new HudManager.RectTransformAnimated() { rectTransform = verticalPanel, position = new(defaultVerticalPanelPos.x, 170), size = new(defaultVerticalPanelSize.x, -620) },
                };

                LeanTween.color(background, new Color(defCol.r, defCol.g, defCol.b, 0), animSpeed).setIgnoreTimeScale(true);
            }

            GameManager.Instance.HudManager.AnimateRectTransforms(transform, anims, enable, actionAfterAnim, force, 0.7f);
        }
    }

    public void RefreshCategory()
    {
        DestroyProductUIs();

        var productSample = productsPanel.GetChild(0).GetChild(0);
        switch (selectedCategoty)
        {
            case ProductsCategory.Coins:
                {
                    var coins = GameManager.Instance.ProductManager.GetProductCoins();
                    foreach (var coin in coins)
                    {
                        var prod = Instantiate(productSample, productSample.parent).GetComponent<ProductUI>();
                        prod.SetProduct(coin);
                        prod.gameObject.SetActive(true);
                    }
                }
                break;

            case ProductsCategory.Purchased:
                {
                    List<ProductBall> balls = GameManager.Instance.ProductManager.GetProductBalls(true).ToList();
                    balls.AddRange(GameManager.Instance.ProductManager.GetProductBombs(true));
                    foreach (var ball in balls)
                    {
                        if (ball.Purchased)
                        {
                            var prod = Instantiate(productSample, productSample.parent).GetComponent<ProductUI>();
                            prod.SetProduct(ball);
                            prod.gameObject.SetActive(true);
                        }
                    }
                }
                break;

            case ProductsCategory.Bombs:
                {
                    foreach (var ball in GameManager.Instance.ProductManager.GetProductBombs(false))
                    {
                        var prod = Instantiate(productSample, productSample.parent).GetComponent<ProductUI>();
                        prod.SetProduct(ball);
                        prod.gameObject.SetActive(true);
                    }
                }
                break;

            case ProductsCategory.Balls:
            default:
                {
                    foreach (var ball in GameManager.Instance.ProductManager.GetProductBalls(false))
                    {
                        var prod = Instantiate(productSample, productSample.parent).GetComponent<ProductUI>();
                        prod.SetProduct(ball);
                        prod.gameObject.SetActive(true);
                    }
                }
                break;
        }
    }

    public void ChangeCategory(ProductsCategory category)
    {
        if (selectedCategoty == category) return;

        selectedCategoty = category;
        RefreshCategory();
    }

    public void DestroyProductUIs()
    {
        var content = productsPanel.GetChild(0);
        for (var i = content.childCount - 1; i >= 1; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }

    public void UpdateCoins()
    {
        coinsText.text = GameManager.Instance.PlayerManager.Coins.ToString();
    }
}
