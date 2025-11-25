using UnityEngine.UI;
using RuntimeGizmos;
using UnityEngine;
using TMPro;

public class SelectedObjectUI : MonoBehaviour
{
    public TMP_Text objectNameText;

    public Slider sliderTilingX;
    public TMP_Text textTilingX;
    public Slider sliderTilingY;
    public TMP_Text textTilingY;

    public Slider sliderOffsetX;
    public TMP_Text textOffsetX;
    public Slider sliderOffsetY;
    public TMP_Text textOffsetY;

    private QuadBilinear currentQuadBilinear;

    public void SetSelectedObject(Transform cam)
    {
        Transform obj = cam.GetComponent<TransformGizmo>().currentTarget;

        if (obj)
        {
            objectNameText.text = obj.name;

            currentQuadBilinear = obj.GetComponent<QuadBilinear>();

            currentQuadBilinear.textureTiling.x = sliderTilingX.value;
            textTilingX.text = "Tiling X: " + currentQuadBilinear.textureTiling.x.ToString("F2");
            currentQuadBilinear.textureTiling.y = sliderTilingY.value;
            textTilingY.text = "Tiling Y: " + currentQuadBilinear.textureTiling.y.ToString("F2");

            currentQuadBilinear.textureTiling.x = sliderOffsetX.value;
            textOffsetX.text = "Offset X: " + currentQuadBilinear.textureOffset.x.ToString("F2");
            currentQuadBilinear.textureTiling.y = sliderOffsetY.value;
            textOffsetY.text = "Offset Y: " + currentQuadBilinear.textureOffset.y.ToString("F2");
        }
    }

    public void UpdateTilingX(float value)
    {
        if (currentQuadBilinear != null)
        {
            currentQuadBilinear.textureTiling.x = value;
            textTilingX.text = "Tiling X: " + value.ToString("F2");
        }
    }

    public void UpdateTilingY(float value)
    {
        if (currentQuadBilinear != null)
        {
            currentQuadBilinear.textureTiling.y = value;
            textTilingY.text = "Tiling Y: " + value.ToString("F2");
        }
    }

    public void UpdateOffsetX(float value)
    {
        if (currentQuadBilinear != null)
        {
            currentQuadBilinear.textureOffset.x = value;
            textOffsetX.text = "Offset X: " + value.ToString("F2");
        }
    }

    public void UpdateOffsetY(float value)
    {
        if (currentQuadBilinear != null)
        {
            currentQuadBilinear.textureOffset.y = value;
            textOffsetX.text = "Offset Y: " + value.ToString("F2");
        }
    }

    private void OnEnable()
    {
        sliderTilingX.onValueChanged.AddListener(UpdateTilingX);
        sliderTilingY.onValueChanged.AddListener(UpdateTilingY);
        sliderOffsetX.onValueChanged.AddListener(UpdateOffsetX);
        sliderOffsetY.onValueChanged.AddListener(UpdateOffsetY);
    }

    private void OnDisable()
    {
        sliderTilingX.onValueChanged.RemoveListener(UpdateTilingX);
        sliderTilingY.onValueChanged.RemoveListener(UpdateTilingY);
        sliderOffsetX.onValueChanged.RemoveListener(UpdateOffsetX);
        sliderOffsetY.onValueChanged.RemoveListener(UpdateOffsetY);
    }
}