using EZCameraShake;
using UnityEngine;

public class Bomb : MovingObject
{
    protected override Transform MyPoolObject => GameManager.Instance.BombsPool;
    public override Sprite Skin { get => GameManager.Instance.PlayerManager.SelectedBomb.Sprite; }
    public override Color Color { get => GameManager.Instance.PlayerManager.SelectedBomb.Color; }

    [SerializeField] private Color particlesColor;

    public override int DealDamage => 0;

    public override void SetPoints(int points, int damagePerClick)
    {
        this.points = -Mathf.Clamp(points, 1, int.MaxValue);
        maxPoints = this.points;
        this.damagePerClick = maxPoints;
    }

    public override void OnClicked()
    {
        if (points - damagePerClick <= 0)
        {
            transform.parent.GetComponent<BallonsSpawner>().DecreaseBombChance(5f);
        }

        base.OnClicked();
    }

    public override void InitSound()
    {
        GameManager.Instance.SoundsManager.SetupAudioBomb(audioSource);
    }

    public override void PlayPopSound()
    {
        base.PlayPopSound();
        CameraShaker.Instance.ShakeOnce(3.9f, 3.9f, 0.1f, 0.1f);
    }

    public override void UpdateColor()
    {
        FirstParticlesColor = particlesColor;
        SecondParticlesColor = Helpers.AddColorHue(FirstParticlesColor, 0.6f);
    }
}
