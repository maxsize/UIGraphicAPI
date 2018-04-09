using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPieChart : UIChart<PieVO>
{
	public enum LegendPlacement
	{
		L,
		R
	}

	public LegendPlacement Placement = LegendPlacement.R;
	public float HorizontalSpace = 10;
	public float VerticalSpace = 5;
	public Color32 LegendFontColor = Color.white;

	protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
	{
		Vector2 sizeDelta = (transform as RectTransform).sizeDelta;
		float radius = Mathf.Min(sizeDelta.x, sizeDelta.y) / 2;
		Vector2 center = Vector2.one * radius;
		PiePieceVO piece = item as PiePieceVO;
		Vector2 fillInfo = Data.GetItemFillInfo(piece);
		canvas.Arc(center, radius, false, Color.white, 1, true, item.color, fillInfo.x, fillInfo.y, 180);
		return new Vector2[]{};
	}

	// Disable drawing axies coordinate
	protected override void DrawAxies(){}

	protected override void DrawScale()
	{
		if (!ShowScale) return;
		canvas.strokeStyle.fill = true;
		canvas.strokeStyle.stroke = false;
		canvas.strokeStyle.strokeColor = Color.white;
		canvas.strokeStyle.thickness = 1;
		float blockSize = 30f;
		for (int i = 0; i < Data.Items.Length; i++)
		{
			// Draw legend color
			var item = Data.Items[i];
			canvas.strokeStyle.fillColor = item.color;
			float x = boundary.width + HorizontalSpace;
			float y = boundary.height - i * (blockSize+VerticalSpace);
			canvas.Rect(x, y, blockSize, blockSize);

			// Create label
			Text txt = CreateLabel(item.label, 20);
			txt.alignment = TextAnchor.MiddleLeft;
			txt.rectTransform.pivot = new Vector2(0, 0.5f);
			txt.rectTransform.sizeDelta = new Vector2(txt.rectTransform.sizeDelta.x, blockSize);
			txt.rectTransform.localPosition = new Vector2(x + blockSize + HorizontalSpace, y + blockSize/2);
		}
	}
}
