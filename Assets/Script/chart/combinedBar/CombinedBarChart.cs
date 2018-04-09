using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombinedBarChart : UIChart<BarVO> {

	public Color DivideLineColor = Color.black;
	public Color LabelBorderColor = Color.white;
	public Color ValueLabelColor = Color.white;
	public float DivideLineThickness = 2f;
	public float LabelGap = 20f;
	public int DescLabelFontSize = 24;
	public Color SpecialLabelColor = Color.green;
	public string LabelTemplate = "{0}({1}) 是本县的{2}倍。";
	public string LabelTemplateMine = "本县表现({0})。";
	public bool AnimateSpecialLabelOnly = true;
	public Image SpecialLabelBg;
	public Image LabelBg;

	private Dictionary<ChartItemVO, Vector2[]> m_ItemVertexDic = new Dictionary<ChartItemVO, Vector2[]>();
	private Dictionary<ChartItemVO, Text> m_ItemLabelDic = new Dictionary<ChartItemVO, Text>();
	private Dictionary<ChartItemVO, Text> m_ItemDescDic = new Dictionary<ChartItemVO, Text>();

	// private ChartItemVO m_StandardItem;
	private bool m_LabelDrawn;
	// private float m_LabelPosition;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    public override void Start()
    {
        base.Start();
        canvas.OnPopulatePolygon += OnPopulatePolygon;
        // m_ItemVertexDic.Clear();

		// Find item with largest value
    }

	private ChartItemVO FindLargestItem()
	{
		ChartItemVO largest = null;
		for (int i = 0; i < Data.Items.Length; i++)
		{
			if (largest == null)
			{
				largest = Data.Items[i];
				continue;
			}
			var item = Data.Items[i];
			if (item.value > largest.value) largest = item;
		}
		return largest;
	}

	protected override void BeforeDrawItems(float lerp)
	{
		// if (m_StandardItem == null)
		// 	m_StandardItem = FindLargestItem();
		// float width = GetWidth();
		// Vector2 sizLerp = GetItemSize(m_StandardItem);
		// sizLerp.x = width;
		// canvas.strokeStyle.stroke = true;
		// canvas.strokeStyle.strokeColor = DivideLineColor;
		// canvas.strokeStyle.fill = false;
		// canvas.strokeStyle.fillColor = Color.white;
		// canvas.strokeStyle.thickness = DivideLineThickness;
		// canvas.Rect(0, 0, sizLerp.x, sizLerp.y);
	}

	protected override void ResetBeforeRedraw()
	{
		// m_StandardItem = null;
		m_LabelDrawn = false;
	}

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
		float lerpCache = lerp;
		// if (AnimateSpecialLabelOnly) lerp = 1;
		// if (m_StandardItem == null)
		// 	m_StandardItem = FindLargestItem();
		float width = GetWidth();
		Vector2 sizLerp = GetLerpedSize(size, lerp);
		sizLerp.x = width;
		if ((item as BarItemVO).SpecialItem)
		{
			canvas.strokeStyle.stroke = false;
			canvas.strokeStyle.fill = true;
			canvas.strokeStyle.fillColor = item.color;
			canvas.Rect(0, 0, sizLerp.x, sizLerp.y);

			Vector2[] arr1 = new Vector2[4]
			{
				new Vector2(0, 0),
				new Vector2(sizLerp.x, 0),
				new Vector2(sizLerp.x, sizLerp.y),
				new Vector2(0, sizLerp.y)
			};
        	m_ItemVertexDic[item] = arr1;
		}

		// canvas.strokeStyle.fill = false;
		// canvas.strokeStyle.stroke = true;
		// canvas.strokeStyle.strokeColor = DivideLineColor;
		// canvas.strokeStyle.thickness = DivideLineThickness;
		// canvas.MoveTo(0, sizLerp.y);
		// canvas.LineTo(sizLerp.x, sizLerp.y);
		// canvas.Stroke();

		Text label;
		if (m_ItemLabelDic.TryGetValue(item, out label))
		{
			float value = Mathf.Round(item.value * lerp * 100) / 100;
			label.text = value.ToString();
			label.transform.localPosition = new Vector2(label.transform.localPosition.x, sizLerp.y + 5);

			if (AnimateSpecialLabelOnly && (item as BarItemVO).SpecialItem)
			{
				value = Mathf.Round(item.value * lerpCache * 100) / 100;
				label.text = value.ToString();
				Vector2 sizLerp1 = GetLerpedSize(size, lerpCache);
				label.transform.localPosition = new Vector2(label.transform.localPosition.x, sizLerp1.y + 5);
			}
		}

        Vector2[] arr = new Vector2[4]
        {
            new Vector2(pos.x, pos.y),
            new Vector2(pos.x + sizLerp.x, pos.y),
            new Vector2(pos.x + sizLerp.x, pos.y + sizLerp.y),
            new Vector2(pos.x, pos.y + sizLerp.y)
        };
        return arr;
    }

	protected override void AfterDrawItems(float lerp)
	{
		float lerpCache = lerp;
		if (AnimateSpecialLabelOnly) lerp = 1;
		ChartItemVO[] items = Data.Items.Clone() as ChartItemVO[];
		Array.Sort(items, delegate(ChartItemVO c1, ChartItemVO c2){
			if (c1.value == c2.value) return 0;
			else return c1.value.CompareTo(c2.value);
		});
		float totalHeight = 0;
		BarItemVO specialItem = null;
		Text specialLabel = null;
		if (!m_LabelDrawn)
		{
			Image[] imgs = GetComponentsInChildren<Image>();
			foreach (var img in imgs)
			{
				if (img.gameObject.name != "3DImage")
					Destroy(img.gameObject);
			}
		}
		for (int i = 0; i < items.Length; i++)
		{
			var item = items[i];
			Text label;
			if (m_ItemDescDic.TryGetValue(item, out label))
			{
				Vector2 labelSize = (label.transform as RectTransform).sizeDelta;
				var finalSize = GetItemSize(item);
				var sizLerp = GetLerpedSize(finalSize, lerp);
				sizLerp.x = GetWidth();
				float leftAnchor = sizLerp.y;
				if (sizLerp.y - totalHeight < 43)
				{
					sizLerp.y = totalHeight + 43;
				}
				totalHeight = sizLerp.y + 9;
				label.transform.localPosition = new Vector2(label.transform.localPosition.x, sizLerp.y);
				canvas.strokeStyle.fill = false;
				if (!AnimateSpecialLabelOnly || (AnimateSpecialLabelOnly && !(item as BarItemVO).SpecialItem))
				{
					canvas.strokeStyle.strokeColor = DivideLineColor;
					canvas.strokeStyle.stroke = true;
					canvas.MoveTo(0, leftAnchor);
					canvas.LineTo(sizLerp.x, leftAnchor);
					canvas.LineTo(label.transform.localPosition.x - 4, label.transform.localPosition.y - labelSize.y/2);
					canvas.Stroke();
				}
				// canvas.strokeStyle.strokeColor = LabelBorderColor;
				// canvas.Rect(label.transform.localPosition.x - 4, label.transform.localPosition.y - labelSize.y, labelSize.x + 4, labelSize.y + 8);
				if (!m_LabelDrawn)
				{
					Image prefab = (item as BarItemVO).SpecialItem ? SpecialLabelBg : LabelBg;
					var image = Instantiate(prefab);
					image.transform.SetParent(transform);
					image.gameObject.SetActive(true);
					image.transform.SetAsFirstSibling();
					image.transform.localScale = Vector3.one;
					var pos = label.transform.localPosition;
					image.rectTransform.localPosition = new Vector3(pos.x - 20, pos.y - 16, 0);
				}
			}
			if ((item as BarItemVO).SpecialItem)
			{
				specialItem = item as BarItemVO;
				specialLabel = label;
			}
		}
		if (AnimateSpecialLabelOnly && specialItem != null && specialLabel != null)
		{
			var finalSize = GetItemSize(specialItem);
			var sizLerp = GetLerpedSize(finalSize, lerpCache);
			sizLerp.x = GetWidth();
			Vector2 labelSize = (specialLabel.transform as RectTransform).sizeDelta;
			float leftAnchor = sizLerp.y;
			canvas.strokeStyle.strokeColor = DivideLineColor;
			canvas.MoveTo(0, leftAnchor);
			canvas.LineTo(sizLerp.x, leftAnchor);
			canvas.LineTo(specialLabel.transform.localPosition.x - 4, specialLabel.transform.localPosition.y - labelSize.y/2);
			canvas.Stroke();
			// specialLabel.rectTransform.localPosition = new Vector3(specialLabel.rectTransform.localPosition.x, leftAnchor, specialLabel.rectTransform.localPosition.z);
		}
		m_LabelDrawn = true;
	}

    protected override void DrawScale()
    {
        // base.DrawScale();
        if (!ShowScale) return;
		RecycleLabel();
		// Draw info text
		int Len = Data.Items.Length;
		float gap = 5;
		var mine = Array.Find(Data.Items, a => (a as BarItemVO).SpecialItem == true);
		if (mine == null) mine = Data.Items[0];
		for (int i = 0; i < Len; i++)
		{
			Text valueLabel = null;
			float percentage = Mathf.Round((Data.Items[i].value / mine.value) * 100) / 100;
			string desc = string.Format(LabelTemplate, Data.Items[i].label, Data.Items[i].value, percentage);
			if (Data.Items[i] == mine)
			{
				valueLabel = CreateLabel(Data.Items[i].value.ToString());
				valueLabel.color = ValueLabelColor;
				desc = string.Format(LabelTemplateMine, mine.value);
			}
			Text descLabel = CreateLabel(desc, 400);
			descLabel.fontSize = DescLabelFontSize;
			descLabel.resizeTextForBestFit = false;
			if (Data.Items[i] == mine)
				descLabel.color = SpecialLabelColor;
			if (valueLabel)
				m_ItemLabelDic.Add(Data.Items[i], valueLabel);
			m_ItemDescDic.Add(Data.Items[i], descLabel);
			Vector2 pos = GetItemPos(i);
            Vector2 siz = GetItemSize(Data.Items[i]);
			siz.x = GetWidth();
			if (Data.dir == ChartVO.Directions.H)
			{
				if (valueLabel)
				{
					valueLabel.alignment = TextAnchor.MiddleLeft;
					valueLabel.rectTransform.pivot = new Vector2(0, 0);
					valueLabel.rectTransform.localPosition = new Vector2(siz.x + gap, pos.y);
				}

				descLabel.alignment = TextAnchor.MiddleLeft;
				descLabel.rectTransform.pivot = new Vector2(0, 0);
				descLabel.rectTransform.localPosition = new Vector2(siz.x + gap, pos.y);
			}
			else
			{
				if (valueLabel)
				{
					valueLabel.alignment = TextAnchor.MiddleCenter;
					valueLabel.rectTransform.pivot = new Vector2(0.5f, 0f);
					valueLabel.rectTransform.localPosition = new Vector2(siz.x/2, siz.y);
				}

				descLabel.alignment = TextAnchor.UpperLeft;
				descLabel.rectTransform.pivot = new Vector2(0f, 1f);
				descLabel.rectTransform.localPosition = new Vector2(siz.x + LabelGap, siz.y);
			}
		}
    }

    protected List<UIVertex[]> OnPopulatePolygon(UIVertex[] vertices, string name)
    {
        if (vertices.Length != 4) return null;
        ChartItemVO data = FindItem(vertices);
        if (data == null) return null;
        Sprite sprite = canvas.MainSprite;
        if (sprite)
        {
            var uv = (sprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(sprite) : Vector4.zero;
            vertices[0].uv0 = new Vector2(uv.x, uv.y);
            vertices[1].uv0 = new Vector2(uv.z, uv.y);
            vertices[2].uv0 = new Vector2(uv.z, uv.w);
            vertices[3].uv0 = new Vector2(uv.x, uv.w);
            vertices[0].color = Color.white;
            vertices[1].color = Color.white;
            vertices[2].color = Color.white;
            vertices[3].color = Color.white;
        }
        else
        {
            vertices[0].color = data.ColorBL;
            vertices[1].color = data.ColorBR;
            vertices[2].color = data.ColorTR;
            vertices[3].color = data.ColorTL;
            vertices[0].uv0 = Vector2.zero;
            vertices[1].uv0 = Vector2.zero;
            vertices[2].uv0 = Vector2.zero;
            vertices[3].uv0 = Vector2.zero;
        }
        var list = new List<UIVertex[]>();
        list.Add(vertices);
        return list;
    }

    private ChartItemVO FindItem(UIVertex[] vertices)
    {
        foreach (KeyValuePair<ChartItemVO, Vector2[]> kv in m_ItemVertexDic)
        {
            bool equal = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 pos = vertices[i].position;
                Vector2 val = kv.Value[i];
                equal = pos.Equals(val);
                if (!equal) break;
            }
            if (equal) return kv.Key;
        }
        return null;
    }

	private float GetWidth()
	{
		float width = (transform as RectTransform).sizeDelta.x;
		return width;
	}

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (canvas) canvas.OnPopulatePolygon -= OnPopulatePolygon;
    }
}
