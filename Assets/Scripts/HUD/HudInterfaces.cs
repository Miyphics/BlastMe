using System;

public interface IAnimatedHud
{
    void ToggleAnim(bool enable, bool force = false);
    void ToggleAnim(bool enable, Action actionAfterAnim, bool force = false);
}
