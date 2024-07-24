using UnityEngine.EventSystems;

public class Checkbox : AnimatedButton
{
    private bool isChecked;
    public bool IsChecked
    {
        get { return isChecked; }
        set
        {
            isChecked = value;
            if (value)
                ShowObjectOnButton();
            else
                HideObjectOnButton();
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.HudManager.IsAnimated && (GameManager.Instance.HudManager.ConfirmationWindow.Hidden || canClickInConfrimWindow))
        {
            isChecked = !isChecked;
            ToggleObjectOnButton();

            if (!onClickEventAfterAnim)
                onClick.Invoke();

            PlayAnim();
        }
    }
}
