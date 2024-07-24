using UnityEngine;

public class Helpers
{
    public static string GetShortPrice(uint price)
    {
        return price.ToString();
    }

    public static Color AddColorValue(in Color color, float value)
    {
        Color.RGBToHSV(color, out var H, out var S, out var V);
        V *= value;
        return Color.HSVToRGB(H, S, V);
    }

    public static Color AddColorHue(in Color color, float hue)
    {
        Color.RGBToHSV(color, out var H, out var S, out var V);
        H *= hue;
        return Color.HSVToRGB(H, S, V);
    }
}
