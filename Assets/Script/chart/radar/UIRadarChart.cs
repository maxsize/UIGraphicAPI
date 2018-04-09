using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UICanvas))]
public class UIRadarChart : UIChart<RadarChartVO> {

	public enum OutlineStyle
	{
		CIRCLE, POLYGON
	}

	public OutlineStyle Style = OutlineStyle.CIRCLE;

	public bool DrawBackground = true;
	public bool FillBackground = true;
	public Color FillBackgroundColor = Color.gray;
	public Color CircleColor = Color.gray;
	// public Color GraphicColor = Color.blue;
	public float OuterCircleThickness = 2;
	// public float InnerCircleThickness = 2;
	public float LabelGap = 15;
	public bool FillRadar = true;
	public Color RadarColor = Color.white;
	public Color RadarBorderColor = Color.white;
	public bool DrawGraphicBorder = false;
	// public Color BorderColor = Color.black;
	public float BorderThickness = 1;
	public Sprite ButtonImageSprite;
	public RectTransform LabelPrefab;
	public float LabelWidth = 160;
	public float LabelHeight = 50;

	public LabelClickEvent OnLabelClicked = new LabelClickEvent();
	// public LabelClickEvent OnScoreClicked = new LabelClickEvent();

	private List<RectTransform> m_ButtonsCache = new List<RectTransform>();
	private List<RectTransform> m_Buttons = new List<RectTransform>();

	public override void Start()
	{
		base.Start();
		// canvas.OnPopulatePolygon += OnPopulatePolygon;
	}

    private List<UIVertex[]> OnPopulatePolygon(UIVertex[] vertices, string name)
    {
		List<UIVertex[]> list = new List<UIVertex[]>();
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i].color = Data.Items[i].color;
		}
		list.Add(vertices);
		return list;
    }

    protected override void BeforeDrawItems(float lerp)
	{
		if (!DrawBackground) return;
		Vector2 center = GetCenter();
		float radius = GetRadius();
		// float radiusInner = radius * 0.8f;
		float radStep = (360 / Data.Items.Length) * Mathf.Deg2Rad;

		if (Style == OutlineStyle.POLYGON)
		{
			canvas.strokeStyle.stroke = true;
			canvas.strokeStyle.strokeColor = CircleColor;
			canvas.strokeStyle.fill = false;
			for (int i = 0; i < Data.Items.Length; i++)
			{
				float rad = radStep * i;
				float c = Mathf.Cos(rad);
				float s = Mathf.Sin(rad);
				Vector2 p0 = new Vector2(center.x + radius*s, center.y + radius*c);     // 外边框 顶点0
				canvas.MoveTo(center);
				canvas.LineTo(p0);
			}
			canvas.Stroke();
		}

		if (Style == OutlineStyle.CIRCLE)
		{
			canvas.strokeStyle.fill = FillBackground;
			canvas.strokeStyle.fillColor = FillBackgroundColor;
			canvas.Arc(center, radius / 2, true, CircleColor, OuterCircleThickness, FillBackground, FillBackgroundColor, 0, 100, 60);
			canvas.Arc(center, radius, true, CircleColor, OuterCircleThickness, FillBackground, FillBackgroundColor, 0, 100, 60);
		}
		else
		{
			if (FillBackground)
			{
				// Draw fill background before draw lines
				canvas.strokeStyle.fill = FillBackground;
				canvas.strokeStyle.fillColor = FillBackgroundColor;
				canvas.strokeStyle.stroke = false;
				for (int i = 0; i <= Data.Items.Length; i++)
				{
					float rad = radStep * i;
					float c = Mathf.Cos(rad);
					float s = Mathf.Sin(rad);
					Vector2 p0 = new Vector2(center.x + radius*s, center.y + radius*c);     // 外边框 顶点0
					if (i == 0) canvas.MoveTo(p0);
					else if (i == Data.Items.Length) canvas.LineTo(center.x, center.y + radius);
					else canvas.LineTo(p0);
				}
				canvas.Stroke();
			}

			canvas.strokeStyle.fill = false;
			canvas.strokeStyle.stroke = true;
			for (int j = 1; j <= 5; j++)
			{
				float innerRadius = radius * (j/5f);
				for (int i = 0; i <= Data.Items.Length; i++)
				{
					float rad = radStep * i;
					float c = Mathf.Cos(rad);
					float s = Mathf.Sin(rad);
					Vector2 p0 = new Vector2(center.x + innerRadius*s, center.y + innerRadius*c);     // 外边框 顶点0
					if (i == 0) canvas.MoveTo(p0);
					else if (i == Data.Items.Length) canvas.LineTo(center.x, center.y + innerRadius);
					else canvas.LineTo(p0);
				}
			}
			canvas.Stroke();
		}
		// canvas.Arc(center, radiusInner, true, CircleColor, InnerCircleThickness, false, Color.white, 0, 100, 60);
	}

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
		return new Vector2[]{};
    }

	protected override void AfterDrawItems(float lerp)
	{
		if (Data == null || Data.Items == null || Data.Items.Length < 1) return;
		float radStep = -(360 / Data.Items.Length) * Mathf.Deg2Rad;
		float radius = GetRadius();
		Vector2 center = GetCenter();
		canvas.Stroke();
		canvas.strokeStyle.stroke = DrawGraphicBorder;
		canvas.strokeStyle.strokeColor = RadarBorderColor;
		canvas.strokeStyle.thickness = BorderThickness;
		canvas.strokeStyle.fill = FillRadar;
		canvas.strokeStyle.fillColor = RadarColor;
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
				canvas.MoveTo(p0);
				firstPoint = p0;
			}
			else canvas.LineTo(p0);
		}
		canvas.LineTo(firstPoint);
		canvas.Stroke();
	}

	protected override void DrawScale()
	{
		if (!ShowScale) return;
		RemoveButtons();	// Recycle buttons		
		Vector2 center = GetCenter();
		float radius = GetRadius();
		float radiusOutter = radius + LabelGap;
		float radStep = (360 / Data.Items.Length) * Mathf.Deg2Rad;
		for (int i = 0; i < Data.Items.Length; i++)
		{
            float rad = radStep * i;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            Vector2 p0 = new Vector2(center.x + radiusOutter*s, center.y + radiusOutter*c);     // 外边框 顶点0
			RadarItemVO vo = Data.Items[i] as RadarItemVO;
			RectTransform button = CreateButton(vo, LabelWidth, LabelHeight);
			// Text label1 = button.GetComponentsInChildren<Text>()[0];
			TextMeshProUGUI label = button.GetComponentsInChildren<TextMeshProUGUI>()[0];
			// label1.text = "评分：" + vo.Score.ToString();
			label.text = vo.label;
			button.localPosition = p0;
			if (rad == 0)
			{
				// label.alignment = TextAnchor.LowerCenter;
				button.pivot = new Vector2(0.5f, 0);
			}
			else if (rad < Mathf.PI/2 && rad > 0)
			{
				// 1st dimention
				// label.alignment = TextAnchor.MiddleLeft;
				button.pivot = new Vector2(0, 0);
			}
			else if (rad >= Mathf.PI/2 && rad <= Mathf.PI)
			{
				// 2nd dimention
				// label.alignment = TextAnchor.MiddleLeft;
				button.pivot = new Vector2(0, 1);
			}
			else if (rad > Mathf.PI && rad <= Mathf.PI*1.5)
			{
				// 3rd dimention
				// label.alignment = TextAnchor.MiddleRight;
				button.pivot = new Vector2(1, 1);
			}
			else
			{
				// 4th dimention
				// label.alignment = TextAnchor.MiddleRight;
				button.pivot = new Vector2(1, 0);
			}
		}
	}

	private RectTransform CreateButton(RadarItemVO data, float width, float height)
	{
		RectTransform trans;
		if (m_ButtonsCache.Count > 0)
		{
			trans = m_ButtonsCache[0];
			m_ButtonsCache.RemoveAt(0);
		}
		else
		{
			trans = Instantiate(LabelPrefab);
		}
		TextMeshProUGUI label = trans.GetComponentInChildren<TextMeshProUGUI>();
		label.fontSize = LabelFontSize;
		label.color = LabelColor;
		label.text = data.label;
		trans.gameObject.SetActive(true);
		trans.transform.SetParent(transform);
		RectTransform rect = trans.transform as RectTransform;
		rect.localScale = Vector3.one;
		rect.localPosition = Vector3.zero;
		rect.localRotation = Quaternion.identity;
		rect.sizeDelta = new Vector2(width, height);
		// Button b1 = trans.GetComponentsInChildren<Button>()[0];
		// Button b2 = trans.GetComponentsInChildren<Button>()[1];
		// b1.onClick.AddListener(() => {
		// 	OnScoreClicked.Invoke(data);
		// });
		// b1.onClick.AddListener(() => {
		// 	OnLabelClicked.Invoke(data);
		// });
		m_Buttons.Add(trans);
		return trans;
	}

    private void RemoveButtons()
    {
		// m_Buttons.ForEach(trans => {
		// 	Button b1 = trans.GetComponentsInChildren<Button>()[0];
		// 	b1.onClick.RemoveAllListeners();
		// 	trans.gameObject.SetActive(false);
		// });
        m_ButtonsCache.AddRange(m_Buttons);
		m_Buttons.Clear();
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

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		// OnScoreClicked.RemoveAllListeners();
		OnLabelClicked.RemoveAllListeners();
	}
}

[Serializable]
public class RadarChartVO : ChartVO, IChartVO
{
	// public RadarItemVO[] Items;
	[SerializeField]
	private RadarItemVO[] m_Items;

	public JSONNode Data;

    public float maxValue {get;}

    public float unitScale {get;}

    public float numScale {get;}

    public float maxScale {get;}

    public Directions dir { get;set; }
	
    public ChartItemVO[] Items { get { return m_Items; } set { m_Items = value as RadarItemVO[]; } }
	// public Color RadarColor;
}

[Serializable]
public class RadarItemVO : ChartItemVO
{
	public float Range;
	public float Score;
}

[Serializable]
public class LabelClickEvent : UnityEvent<RadarItemVO> {}