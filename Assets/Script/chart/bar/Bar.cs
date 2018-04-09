using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour {

	public BarVO data;
	public GameObject itemPrefab;
	public Scale scalePrefab;

	private List<GameObject> bars;
	private List<Binding> bindings;

	// Use this for initialization
	void Start () {
		CreateItems();
	}

    private void CreateItems()
    {
		if (data == null || itemPrefab == null) return;
		bars = new List<GameObject>();
		bindings = new List<Binding>();
		RectTransform rt = GetComponent<RectTransform>();
		float maxValue = data.maxScale;
		float spacing = rt.sizeDelta.x / (data.Items.Length + 1);
		for (int i = 0; i < data.Items.Length; i++)
		{
			var item = data.Items[i];
			var go = Instantiate(itemPrefab);
			go.transform.SetParent(transform);
			var img = go.GetComponent<Image>();
			img.rectTransform.SetSiblingIndex(i);
			img.color = item.color;
			img.rectTransform.sizeDelta = new Vector2(20, rt.sizeDelta.y * (item.value / maxValue));
			img.rectTransform.localPosition = new Vector2(spacing * (i+1), 0);

			var text = go.GetComponentInChildren<Text>();
			text.text = item.label;

			bars.Add(go);

			bindings.Add(Utils.Bind(item, "value", this, "OnValueChanged", true));
			bindings.Add(Utils.Bind(item, "color", this, "OnValueChanged", true));
		}

		var numScale = data.numScale;
		var unitScale = data.unitScale;
		for (int i = 0; i <= numScale; i++)
		{
			Scale scale = Instantiate(scalePrefab);
			scale.transform.SetParent(transform);
			scale.SetValue(i * unitScale, numScale * unitScale, rt.sizeDelta.y);
		}
    }

	private void UpdateBar()
	{
		if (data == null || itemPrefab == null) return;
		RectTransform rt = GetComponent<RectTransform>();
		float maxValue = data.maxScale;
		float spacing = rt.sizeDelta.x / (data.Items.Length + 1);
		for (int i = 0; i < data.Items.Length; i++)
		{
			var item = data.Items[i];
			var go = bars[i];
			var img = go.GetComponent<Image>();
			img.color = item.color;
			img.rectTransform.sizeDelta = new Vector2(20, rt.sizeDelta.y * (item.value / maxValue));
			img.rectTransform.localPosition = new Vector2(spacing * (i+1), 0);
			var text = go.GetComponentInChildren<Text>();
			text.text = item.label;
		}

		var numScale = data.numScale;
		var unitScale = data.unitScale;
		List<Scale> scales = GetComponentsInChildren<Scale>().ToList<Scale>();
		while(scales.Count > 0)
		{
			Scale s = scales[0];
			Destroy(s.gameObject);
			scales.RemoveAt(0);
		}
		for (int i = 0; i <= numScale; i++)
		{
			Scale scale = Instantiate(scalePrefab);
			scale.transform.SetParent(transform);
			scale.SetValue(i * unitScale, numScale * unitScale, rt.sizeDelta.y);
		}
	}

	public void OnValueChanged(object value)
	{
		UpdateBar();
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		if (bindings != null && Bindings.Instance)
		{
			bindings.ForEach(b => Bindings.Instance.RemoveBinding(b));
			bindings.Clear();
		}
	}
}
