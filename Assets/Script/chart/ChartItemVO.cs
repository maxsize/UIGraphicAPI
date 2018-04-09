using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChartItemVO {
	// [Range(min:0, max:float.MaxValue)]
	public float value;
	public Color32 color = Color.white;
	public string label;
	public object data;

	public Color32 ColorTL = Color.white;
	public Color32 ColorTR = Color.white;
	public Color32 ColorBL = Color.white;
	public Color32 ColorBR = Color.white;
}
