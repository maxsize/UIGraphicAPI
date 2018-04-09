using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UICanvas))]
public class RangeBarChart : UIChart<LineChartVO>
{
	public enum Placement
	{
		L, R, B, T
	}

	public float BarSize = 4;
	public float Radius = 10;
	public float RangeLabelGap = 25;
	public string RangeLabelFormat = "共{0}名";
	public string ValueLabelFormat = "第{0}名";
	public Color LineColor = Color.white;

	public bool DrawLinkage = true;
	public bool ShowRangeLabel = true;
	public bool ShowValueLabel = true;
	public Placement ValueLabelPlacement = Placement.L;

	public Sprite SliderBackground;
	public Sprite SliderForeground;
	public Sprite Slider;

	private const string SLIDER_BACKGROUND = "SliderBackground";
	private const string SLIDER_FOREGROUND = "SliderForeground";
	private const string SLIDER = "Slider";

	/// <summary>
	/// Start is called on the frame when a script is enabled just before
	/// any of the Update methods is called the first time.
	/// </summary>
	public override void Start()
	{
		base.Start();
		canvas.OnPopulatePolygon += OnPopulatePolygon;
	}

	protected List<UIVertex[]> OnPopulatePolygon(UIVertex[] vertices, string name)
	{
		if (canvas.MainSprite == null) return new List<UIVertex[]>(){vertices};
		switch (name)
		{
			case SLIDER_BACKGROUND:
				return FillBackground(vertices, SliderBackground);
			case SLIDER_FOREGROUND:
				return FillBackground(vertices, SliderForeground);
			default:
				return FillSlider(vertices, Slider);
		}
	}

	protected List<UIVertex[]> FillBackground(UIVertex[] vertices, Sprite sprite)
	{
		Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
		Vector4 inner = UnityEngine.Sprites.DataUtility.GetInnerUV(sprite);
		float x0 = vertices[0].position.x;
		float x1 = x0 + sprite.border.x;
		float x2 = vertices[1].position.x - sprite.border.z;
		float x3 = vertices[1].position.x;
		float y0 = vertices[0].position.y;
		float y1 = y0 + sprite.border.y;
		float y2 = vertices[3].position.y - sprite.border.w;
		float y3 = vertices[3].position.y;
		float[] xScratch = new float[]{x0, x1, x2, x3};
		float[] yScratch = new float[]{y0, y1, y2, y3};
		float[] uScratch = new float[]{outer.x, inner.x, inner.z, outer.z};
		float[] vScratch = new float[]{outer.y, inner.y, inner.w, outer.w};
		List<UIVertex[]> list = new List<UIVertex[]>();
		for (int x = 0; x < 3; x++)
		{
			for (int y = 0; y < 3; y++)
			{
				UIVertex[] rect = new UIVertex[4];
				rect[0] = new UIVertex()
				{
					position = new Vector2(xScratch[x], yScratch[y]),
					color = Color.white,
					uv0 = new Vector2(uScratch[x], vScratch[y])
				};
				rect[1] = new UIVertex()
				{
					position = new Vector2(xScratch[x+1], yScratch[y]),
					color = Color.white,
					uv0 = new Vector2(uScratch[x+1], vScratch[y])
				};
				rect[2] = new UIVertex()
				{
					position = new Vector2(xScratch[x+1], yScratch[y+1]),
					color = Color.white,
					uv0 = new Vector2(uScratch[x+1], vScratch[y+1])
				};
				rect[3] = new UIVertex()
				{
					position = new Vector2(xScratch[x], yScratch[y+1]),
					color = Color.white,
					uv0 = new Vector2(uScratch[x], vScratch[y+1])
				};
				list.Add(rect);
			}
		}
		return list;
	}

	protected List<UIVertex[]> FillSlider(UIVertex[] vertices, Sprite sprite)
	{
		var uv = (sprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(sprite) : Vector4.zero;
		vertices[0].uv0 = new Vector2(uv.x, uv.y);
		vertices[1].uv0 = new Vector2(uv.z, uv.y);
		vertices[2].uv0 = new Vector2(uv.z, uv.w);
		vertices[3].uv0 = new Vector2(uv.x, uv.w);
		return new List<UIVertex[]>(){vertices};
	}

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
		LineChartItemVO vo = item as LineChartItemVO;
		canvas.strokeStyle.fill = true;
		canvas.strokeStyle.fillColor = Color.white;
		canvas.strokeStyle.stroke = false;
        List<Vector2> allV = new List<Vector2>();
		float siz = GetItemCenter(vo) * lerp;
		if (Data.dir == ChartVO.Directions.V)
		{
			var rect = canvas.Rect(pos.x, 0, BarSize, boundary.height);
			rect.name = SLIDER_BACKGROUND;
			rect = canvas.Rect(pos.x, 0, BarSize, siz);
			rect.name = SLIDER_FOREGROUND;
			rect = canvas.Rect(pos.x - Radius + BarSize/2, siz - Radius, Radius*2, Radius*2);
			rect.name = SLIDER;
			allV.Add(new Vector2(pos.x, pos.y));
			allV.Add(new Vector2(pos.x + BarSize, pos.y));
			allV.Add(new Vector2(pos.x + BarSize, pos.y + boundary.height));
			allV.Add(new Vector2(pos.x, pos.y + boundary.height));
		}
		else
		{
			// float siz = GetItemCenter(vo) * lerp;
			var rect = canvas.Rect(0, pos.y, boundary.width, BarSize);
			rect.name = SLIDER_BACKGROUND;
			rect = canvas.Rect(0, pos.y, siz, BarSize);
			rect.name = SLIDER_FOREGROUND;
			rect = canvas.Rect(siz - Radius, pos.y - Radius + BarSize/2, Radius*2, Radius*2);
			rect.name = SLIDER;
			allV.Add(new Vector2(pos.x, pos.y));
			allV.Add(new Vector2(pos.x + boundary.width, pos.y));
			allV.Add(new Vector2(pos.x + boundary.width, pos.y + BarSize));
			allV.Add(new Vector2(pos.x, pos.y + BarSize));
		}

		return allV.ToArray();
    }

	protected override void BeforeDrawItems(float lerp)
	{
		if (DrawLinkage)
		{
			canvas.strokeStyle.stroke = true;
			canvas.strokeStyle.fill = false;
			canvas.strokeStyle.strokeColor = LineColor;
			canvas.strokeStyle.thickness = 5;
			for (int i = 0; i < Data.Items.Length; i++)
			{
				var item = Data.Items[i];
				LineChartItemVO vo = item as LineChartItemVO;
				// Vector2 siz = GetItemSize(item);
				// Vector2 sizLerp = Vector2.Lerp(Vector2.zero, siz, lerp);
				Vector2 pos = GetItemPos(i);
				// if (Data.dir == ChartVO.Directions.H)
				// 	siz.x = sizLerp.x;
				// else
				// 	siz.y = sizLerp.y;

				float range = GetItemCenter(vo) * lerp;
				if (Data.dir == ChartVO.Directions.V)
				{
					if (i == 0) canvas.MoveTo(pos.x + BarSize/2, range);
					else canvas.LineTo(pos.x + BarSize/2, range);
				}
				else
				{
					if (i == 0) canvas.MoveTo(range, pos.y + BarSize/2);
					else canvas.LineTo(range, pos.y + BarSize/2);
				}
			}
			canvas.Stroke();
			canvas.strokeStyle.fill = true;
			canvas.strokeStyle.fillColor = Color.white;
			canvas.strokeStyle.stroke = false;
		}
	}

	// No axies required for range bar
	protected override void DrawAxies() {}

	protected override void DrawScale()
	{
		if (!ShowScale) return;
		RecycleLabel();
		// Draw info text
		int Len = Data.Items.Length;
		RectTransform rect = transform as RectTransform;
		float gap = RangeLabelGap;
		for (int i = 0; i < Len; i++)
		{
			Text nameLabel = CreateLabel(Data.Items[i].label);
			Text rangeLabel = CreateLabel(string.Format(RangeLabelFormat, (Data.Items[i] as LineChartItemVO).range.ToString()));
			Text valueLabel = CreateLabel(string.Format(ValueLabelFormat, Data.Items[i].value.ToString()));
			rangeLabel.gameObject.SetActive(ShowRangeLabel);
			valueLabel.gameObject.SetActive(ShowValueLabel);
			Vector2 pos = GetItemPos(i);
			LineChartItemVO vo = Data.Items[i] as LineChartItemVO;

			switch (ValueLabelPlacement)
			{
				case Placement.L:
					valueLabel.alignment = TextAnchor.MiddleRight;
					valueLabel.rectTransform.pivot = new Vector2(1, 0.5f);
					if (Data.dir == ChartVO.Directions.H)
						valueLabel.rectTransform.localPosition = new Vector2(GetItemCenter(vo) - Radius, pos.y);
					else
						valueLabel.rectTransform.localPosition = new Vector2(pos.x - Radius + BarSize/2, GetItemCenter(vo));
					break;
				case Placement.R:
					valueLabel.alignment = TextAnchor.MiddleLeft;
					valueLabel.rectTransform.pivot = new Vector2(0, 0.5f);
					if (Data.dir == ChartVO.Directions.H)
						valueLabel.rectTransform.localPosition = new Vector2(GetItemCenter(vo) + Radius, pos.y);
					else
						valueLabel.rectTransform.localPosition = new Vector2(pos.x + Radius + BarSize/2, GetItemCenter(vo));
					break;
				case Placement.T:
					valueLabel.alignment = TextAnchor.LowerCenter;
					valueLabel.rectTransform.pivot = new Vector2(0.5f, 0);
					if (Data.dir == ChartVO.Directions.H)
						valueLabel.rectTransform.localPosition = new Vector2(GetItemCenter(vo), pos.y + Radius);
					else
						valueLabel.rectTransform.localPosition = new Vector2(pos.x + BarSize/2, GetItemCenter(vo) + Radius);
					break;
				case Placement.B:
					valueLabel.alignment = TextAnchor.UpperCenter;
					valueLabel.rectTransform.pivot = new Vector2(0.5f, 0);
					if (Data.dir == ChartVO.Directions.H)
						valueLabel.rectTransform.localPosition = new Vector2(GetItemCenter(vo), pos.y - Radius);
					else
						valueLabel.rectTransform.localPosition = new Vector2(pos.x + BarSize/2, GetItemCenter(vo) - Radius*2);
					break;
			}

			if (Data.dir == ChartVO.Directions.H)
			{
				nameLabel.alignment = TextAnchor.MiddleRight;
				nameLabel.rectTransform.pivot = new Vector2(1, 0.5f);
				nameLabel.rectTransform.localPosition = new Vector2(-Radius, pos.y + BarSize/2);

				rangeLabel.alignment = TextAnchor.MiddleLeft;
				rangeLabel.rectTransform.pivot = new Vector2(0, 0.5f);
				rangeLabel.rectTransform.localPosition = new Vector2(rect.sizeDelta.x + gap, pos.y + BarSize/2);

				// valueLabel.alignment = TextAnchor.MiddleCenter;
				// valueLabel.rectTransform.pivot = new Vector2(0.5f, 0);
				// valueLabel.rectTransform.localPosition = new Vector2(GetItemCenter(vo), pos.y + Radius * 2);
			}
			else
			{
				nameLabel.alignment = TextAnchor.LowerCenter;
				nameLabel.rectTransform.pivot = new Vector2(0.5f, 1);
				nameLabel.rectTransform.localPosition = new Vector2(pos.x + BarSize/2, pos.y);

				rangeLabel.alignment = TextAnchor.LowerCenter;
				rangeLabel.rectTransform.pivot = new Vector2(0.5f, 0);
				rangeLabel.rectTransform.localPosition = new Vector2(pos.x + BarSize/2, pos.y + rect.sizeDelta.y + gap);

				// valueLabel.alignment = TextAnchor.MiddleLeft;
				// valueLabel.rectTransform.pivot = new Vector2(0, 0.5f);
				// valueLabel.rectTransform.localPosition = new Vector2(pos.x + Radius * 2, GetItemCenter(vo));
			}
		}
	}

	private float GetItemCenter(LineChartItemVO itemVO)
	{
		if (Data.dir == ChartVO.Directions.H)
		{
			return (1 - (itemVO.value/itemVO.range)) * boundary.width;
		}
		else
		{
			return (1 - (itemVO.value/itemVO.range)) * boundary.height;
		}
	}
}
