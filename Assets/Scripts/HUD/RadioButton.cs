using UnityEngine.EventSystems;

public class RadioButton : Checkbox
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.Instance.HudManager.IsAnimated && (GameManager.Instance.HudManager.ConfirmationWindow.Hidden || canClickInConfrimWindow))
        {
            if (!IsChecked)
            {
                if (!onClickEventAfterAnim)
                    onClick.Invoke();

                for (var i = transform.parent.childCount - 1; i >= 0; i--)
                {
                    var child = transform.parent.GetChild(i).GetComponent<RadioButton>();
                    if (child != this)
                    {
                        child.IsChecked = false;
                    }
                }

                IsChecked = true;
                PlayAnim();
                return;
            }

            PlayAnim(true);
        }
    }
}
