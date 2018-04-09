using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UICanvas))]
public class UIRadarChart3D : UIChart<RadarChartVO> {

	public float LabelGap = 15;

	public override void Start()
	{
		base.Start();
		canvas.OnPopulatePolygon += OnPopulatePolygon;
	}

    private List<UIVertex[]> OnPopulatePolygon(UIVertex[] vertices, string name)
    {
		// Vector2 center = GetCenter();
		List<UIVertex[]> list = new List<UIVertex[]>();
		// for (int i = 0; i < vertices.Length; i++)
		// {
			// vertices[i].color = Data.Items[i].color;
		// }
		Vector3 center = vertices[1].position;
		Color color = vertices[1].color;
		vertices[1].position = new Vector3(center.x, center.y, canvas.Depth);
		vertices[1].color = Color.Lerp(color, Color.white, 0.8f);
		list.Add(vertices);
		return list;
    }

	protected override void BeforeDrawItems(float lerp)
	{
		// if (!DrawBackground) return;
		Vector2 center = GetCenter();
		float radius = GetRadius();
		// float radiusInner = radius * 0.8f;
		// canvas.Arc(center, radius, true, CircleColor, OuterCircleThickness, false, Color.white, 0, 100, 60);
		// canvas.Arc(center, radiusInner, true, CircleColor, InnerCircleThickness, false, Color.white, 0, 100, 60);
		float radStep = (360 / Data.Items.Length) * Mathf.Deg2Rad;
		for (int i = 0; i < Data.Items.Length; i++)
		{
            float rad = radStep * i;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            Vector2 p0 = new Vector2(center.x + radius*s, center.y + radius*c);
			canvas.MoveTo(center);
			canvas.LineTo(p0);
		}
		canvas.Stroke();
	}

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
		return new Vector2[]{};
    }

	protected override void AfterDrawItems(float lerp)
	{
		if (Data == null || Data.Items == null || Data.Items.Length < 1) return;
		float radStep = (360 / Data.Items.Length) * Mathf.Deg2Rad;
		float radius = GetRadius();
		Vector2 center = GetCenter();
		canvas.Stroke();
		canvas.strokeStyle.stroke = false;
		canvas.strokeStyle.strokeColor = Color.white;
		canvas.strokeStyle.thickness = 1;
		canvas.strokeStyle.fill = true;
		canvas.strokeStyle.fillColor = Data.Items[0].color;
		Vector2 prevPoint = Vector2.zero;
		Vector2 firstPoint = Vector2.zero;
		for (int i = 0; i < Data.Items.Length; i++)
		{
			RadarItemVO item = Data.Items[i] as RadarItemVO;

			float percentage = item.value / item.Range;
			float actualRadius = percentage * radius * lerp;
            float rad = radStep * i;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            Vector2 p0 = new Vector2(center.x + actualRadius*s, center.y + actualRadius*c);     // 外边框 顶点0
			if (i == 0)
			{
				// canvas.MoveTo(p0);
				prevPoint = p0;
				firstPoint = p0;
				continue;
			}
			// else canvas.LineTo(p0);
			canvas.MoveTo(p0);
			canvas.LineTo(center);
			canvas.LineTo(prevPoint);
			canvas.LineTo(p0);
			prevPoint = p0;
		}
		canvas.MoveTo(prevPoint);
		canvas.LineTo(center);
		canvas.LineTo(firstPoint);
		canvas.LineTo(prevPoint);
		canvas.Stroke();
		// canvas.LineTo(firstPoint);
	}

	protected override void DrawScale()
	{
		base.DrawScale();
		if (!ShowScale) return;
		// RemoveButtons();	// Recycle buttons		
		Vector2 center = GetCenter();
		float radius = GetRadius();
		float radiusOutter = radius + LabelGap;
		float radiusLookat = radius * 2;
		float radStep = (360 / Data.Items.Length) * Mathf.Deg2Rad;
		for (int i = 0; i < Data.Items.Length; i++)
		{
            float rad = radStep * i;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            Vector2 p0 = new Vector2(center.x + radiusOutter*s, center.y + radiusOutter*c);     // 外边框 顶点0
            Vector2 p1 = new Vector2(center.x + radiusLookat*s, center.y + radiusLookat*c);
			RadarItemVO vo = Data.Items[i] as RadarItemVO;
			Text label = CreateLabel(vo.label);
			RectTransform button = label.rectTransform;
			// Text label1 = button.GetComponentsInChildren<Text>()[0];
			// Text label = button.GetComponentsInChildren<Text>()[0];
			// label1.text = "评分：" + vo.Score.ToString();
			label.text = vo.label;
			button.localPosition = p0;
			// float degree = Mathf.Rad2Deg * rad - 90;
			// Quaternion rotation = Quaternion.Euler(-90, 0, 0);
			button.pivot = Vector2.one * 0.5f;
			// button.localRotation = rotation;
			button.LookAt(p1);
			// if (rad == 0)
			// {
			// 	// label.alignment = TextAnchor.LowerCenter;
			// 	button.pivot = new Vector2(0.5f, 0);
			// }
			// else if (rad < Mathf.PI/2 && rad > 0)
			// {
			// 	// 1st dimention
			// 	// label.alignment = TextAnchor.MiddleLeft;
			// 	button.pivot = new Vector2(0, 0);
			// }
			// else if (rad >= Mathf.PI/2 && rad <= Mathf.PI)
			// {
			// 	// 2nd dimention
			// 	// label.alignment = TextAnchor.MiddleLeft;
			// 	button.pivot = new Vector2(0, 1);
			// }
			// else if (rad > Mathf.PI && rad <= Mathf.PI*1.5)
			// {
			// 	// 3rd dimention
			// 	// label.alignment = TextAnchor.MiddleRight;
			// 	button.pivot = new Vector2(1, 1);
			// }
			// else
			// {
			// 	// 4th dimention
			// 	// label.alignment = TextAnchor.MiddleRight;
			// 	button.pivot = new Vector2(1, 0);
			// }
		}
	}

    private float GetRadius()
	{
		RectTransform trans = transform as RectTransform;
		return Mathf.Min(trans.sizeDelta.x, trans.sizeDelta.y) / 2;
	}

	private Vector2 GetCenter()
	{
		RectTransform trans = transform as RectTransform;
		return trans.sizeDelta / 2;
	}
}
