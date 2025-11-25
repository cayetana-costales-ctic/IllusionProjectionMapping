using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIElementColorChanger : MonoBehaviour
{
    private Image image;
    private RawImage rawImage;

    void Awake()
    {                                                                                                                                                                                    
        image = GetComponent<Image>();
        rawImage = GetComponent<RawImage>();
    }


    public void SetColor(Color newColor)
    {
        if (image)
            image.color = newColor;

        else if (rawImage)
            rawImage.color = newColor;
    }
}