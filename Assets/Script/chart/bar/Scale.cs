using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scale : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}

    public void SetValue(float scaleValue, float maxScale, float height)
    {
		var txt = GetComponentInChildren<Text>();
		txt.text = scaleValue.ToString();

		float y = (scaleValue / maxScale) * height;
		RectTransform rt = (RectTransform) transform;
		rt.localPosition = new Vector2(0, y);
    }
}
