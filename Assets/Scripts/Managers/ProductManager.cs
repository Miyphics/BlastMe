using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public enum BallType
{
    Ball,
    Bomb
}

[Serializable]
public class Product
{
    [SerializeField] protected string name;
    public string Name => name;
    [SerializeField] protected Sprite sprite;
    [SerializeField] protected Sprite secondSprite;
    public Sprite SecondSprite => secondSprite;
    public Sprite Sprite => sprite;
    [SerializeField] protected uint price;
    public uint Price => price;
}

[Serializable]
public class ProductBall : Product
{
    [SerializeField] protected Color color = Color.white;
    public Color Color => color;
    [SerializeField] protected bool purchased;
    public bool Purchased => purchased;
    [SerializeField] protected BallType ballType = BallType.Ball;
    public BallType BallType => ballType;

    public void Buy() { purchased = true; }
}

[Serializable]
public class ProductCoin : Product
{
    private bool canPickup;
    public bool CanPickup
    {
        get
        {
            if (lastPickupTime == null || lastPickupTime.Length <= 0)
                canPickup = true;
            else
            {
                try
                {
                    DateTime lastTime = DateTime.ParseExact(lastPickupTime, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    DateTime curTime = DateTime.Now;

                    if (curTime.Day > lastTime.Day)
                        canPickup = true;
                }
                catch { canPickup = true; }
            }

            return canPickup;
        }
    }

    private string lastPickupTime;
    public string LastPickupTime => lastPickupTime;

    public void Buy()
    {
        GameManager.Instance.PlayerManager.AddGlobalCoins(price);
        canPickup = false;
        lastPickupTime = DateTime.Now.ToString("dd.MM.yyyy");
    }

    public void SetLastPickupTime(string lastPickupTime)
    {
        this.lastPickupTime = lastPickupTime;
    }
}

public class ProductManager : MonoBehaviour
{
    [SerializeField] private List<ProductBall> productBalls;
    public IReadOnlyList<ProductBall> ProductBalls => productBalls;

    [SerializeField] private List<ProductBall> productBombs;
    public IReadOnlyList<ProductBall> ProductBombs => productBombs;

    [SerializeField] private List<ProductCoin> productCoins;
    public IReadOnlyList<ProductCoin> ProductCoins => productCoins;

    public IReadOnlyList<ProductBall> GetProductBalls(bool purchased)
    {
        return productBalls.Where(x => purchased ? x.Purchased : !x.Purchased).ToList();
    }

    public IReadOnlyList<ProductBall> GetProductBombs(bool purchased)
    {
        return productBombs.Where(x => purchased ? x.Purchased : !x.Purchased).ToList();
    }

    public IReadOnlyList<ProductCoin> GetProductCoins()
    {
        return productCoins;
    }

    public void LoadData(List<SaveProduct> savedProducts)
    {
        foreach (var product in savedProducts)
        {
            if (product is SaveProductBall prodBall)
            {
                switch (prodBall.ballType)
                {
                    case BallType.Bomb:
                        if (productBombs.Count > product.index)
                        {
                            productBombs[product.index].Buy();

                        }
                        break;

                    case BallType.Ball:
                    default:
                        if (productBalls.Count > product.index)
                        {
                            productBalls[product.index].Buy();

                        }
                        break;
                }
            }
            else if (product is SaveProductCoin prodCoin)
            {
                productCoins[product.index].SetLastPickupTime(prodCoin.lastPickupTime);
            }
        }
    }
}
