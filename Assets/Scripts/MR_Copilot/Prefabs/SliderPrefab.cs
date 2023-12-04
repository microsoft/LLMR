using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderPrefab : MonoBehaviour
{
    public TextMeshProUGUI slider_name;
    public Slider slider;
    public RectTransform rect_transform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetName(string name)
    {
        gameObject.name = name;
        slider_name.text = name;
    }

    public void SetRange(float min_val, float max_val, float curr_val)
    {
        slider.minValue = min_val;
        slider.maxValue = max_val;
        slider.value = curr_val;
    }

    public void SetPosition(Vector2 pos)
    {
        rect_transform.anchoredPosition = pos;
        //// Set the anchor preset of the Slider to the top right corner
        //sliderRect.anchorMin = new Vector2(1, 1);
        //sliderRect.anchorMax = new Vector2(1, 1);

        //// Set the pivot of the Slider to the top right corner
        //sliderRect.pivot = new Vector2(1, 1);

        //// Set the position of the Slider to the offset from the top right corner
        //sliderRect.anchoredPosition = new Vector2(-sliderOffsetX, -sliderOffsetY);
    }

    public void MoveToTopRight(Vector2 offset)
    {
        // Set the anchor preset of the Slider to the top right corner
        rect_transform.anchorMin = new Vector2(1, 1);
        rect_transform.anchorMax = new Vector2(1, 1);

        // Set the pivot of the Slider to the top right corner
        rect_transform.pivot = new Vector2(1, 1);

        // Set the position of the Slider to the offset from the top right corner
        rect_transform.anchoredPosition = new Vector2(-offset.x, -offset.y);
    }

    public void MoveToTopLeft(Vector2 offset)
    {
        // Set the anchor preset of the Slider to the top left corner
        rect_transform.anchorMin = new Vector2(0, 1);
        rect_transform.anchorMax = new Vector2(0, 1);

        // Set the pivot of the Slider to the top left corner
        rect_transform.pivot = new Vector2(0, 1);

        // Set the position of the Slider to the offset from the top left corner
        rect_transform.anchoredPosition = new Vector2(offset.x, -offset.y);
    }

    public void SetSize(Vector2 size)
    {
        rect_transform.sizeDelta = size;
    }
}
