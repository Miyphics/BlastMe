using UnityEngine;

public class Ballon : MovingObject
{
    public override Sprite Skin { get => GameManager.Instance.PlayerManager.SelectedBall.Sprite; }
    public override Color Color { get => GameManager.Instance.PlayerManager.SelectedBall.Color; }

    protected override float DistanceToDealDamage
    {
        get
        {
            if (points >= 50)
                return 0;
            else if (points >= 30)
                return 1.5f;
            else if (points >= 10)
                return 2.35f;
            else
                return base.DistanceToDealDamage;
        }
    }

    public override void OnClicked()
    {
        if (points - damagePerClick <= 0)
        {
            mySpawner.IncreaseBombChance();
        }

        base.OnClicked();
    }
}
