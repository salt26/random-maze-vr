using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceSlider : MonoBehaviour
{
    public RectTransform fillRectX;
    public RectTransform fillRectY;
    public RectTransform tickY;

    private float _xValue;
    private float _yValue;

    // Start is called before the first frame update
    void Start()
    {
        _xValue = 0f;
        _yValue = 0f;
    }

    public void SetValues(Vector2 v)
    {
        _xValue = Mathf.Clamp(v.x, 0f, 1f);
        _yValue = Mathf.Clamp(v.y, 0f, 1f);
        UpdateSlider();
    }

    void UpdateSlider()
    {
        fillRectX.anchorMax = new Vector2(_xValue / 2f, 1f);
        fillRectY.anchorMax = new Vector2((_xValue + _yValue) / 2f, 1f);
        tickY.anchorMin = new Vector2(0.49f + _xValue / 2f, 0f);
        tickY.anchorMax = new Vector2(0.51f + _xValue / 2f, 1f);
    }
}
