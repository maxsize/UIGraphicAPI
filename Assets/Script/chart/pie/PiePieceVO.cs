using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PiePieceVO : ChartItemVO {
	[HideInInspector]
	/// <summary>weightage within the pie, x is start, y is end</summary>
	public Vector2 weight = Vector2.zero;
}
