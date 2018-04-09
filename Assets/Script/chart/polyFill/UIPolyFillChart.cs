using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPolyFillChart : UIChart<BarVO> {
	public float Radius = 4;
	public Color DotColor = Color.white;
	public Color LineColor = Color.white;
	public float Thickness = 3;
    public bool DrawDivideLine = true;
    public Color DivideLineColor = Color.gray;
    public float DivideLineThickness = 2;
    public bool DisplayValue = true;
	public bool DisplayDot = true;
	public bool FillGraph = true;

	protected override void BeforeDrawItems(float lerp)
	{
        Vector2 pos;
        Vector2 siz;
		canvas.strokeStyle.stroke = true;
		canvas.strokeStyle.fill = false;
		canvas.strokeStyle.thickness = DivideLineThickness;
        canvas.strokeStyle.strokeColor = DivideLineColor;
        if (DrawDivideLine)
        {
            for (int i = 0; i < Data.Items.Length; i++)
            {
                pos = GetItemPos(i);
                if (Data.dir == ChartVO.Directions.V)
                {
                    canvas.MoveTo(pos.x, 0);
                    canvas.LineTo(pos.x, boundary.height);
                }
                else
                {
                    canvas.MoveTo(0, pos.y);
                    canvas.LineTo(boundary.width, pos.y);
                }
            }
        }
		canvas.strokeStyle.fill = FillGraph;
		canvas.strokeStyle.thickness = Thickness;
        canvas.strokeStyle.strokeColor = LineColor;
		canvas.strokeStyle.fillColor = LineColor;
        pos = GetItemPos(0);
        siz = GetItemSize(Data.Items[0]);

		int start = 0;
		if (FillGraph)
		{
			canvas.MoveTo(0, 0);
		}
		else
		{
			start = 1;
			if (Data.dir == BarVO.Directions.V)
            {
                siz.y *= lerp;
                canvas.MoveTo(pos.x, siz.y);
            }
            else
            {
                siz.x *= lerp;
                canvas.MoveTo(siz.x, pos.y);
            }
		}
		
        for (int i = start; i < Data.Items.Length; i++)
        {
            pos = GetItemPos(i);
            siz = GetItemSize(Data.Items[i]);
            if (Data.dir == BarVO.Directions.V)
            {
                siz.y *= lerp;
                canvas.LineTo(pos.x, siz.y);
            }
            else
            {
                siz.x *= lerp;
                canvas.LineTo(siz.x, pos.y);
            }
        }
		if (FillGraph)
		{
			if (Data.dir == ChartVO.Directions.V)
				canvas.LineTo(pos.x, 0);
			else
				canvas.LineTo(siz.x, pos.y);
			canvas.LineTo(0, 0);
		}
        canvas.Stroke();
	}

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
        // canvas.strokeStyle.stroke = false;
		// canvas.strokeStyle.fill = true;
		// canvas.strokeStyle.fillColor = item.color;
		// canvas.Rect(pos.x, pos.y, size.x, size.y);
        Vector2 lerped = GetLerpedSize(size, lerp);
		if (DisplayDot)
		{
			Vector2 center;
			if (Data.dir == ChartVO.Directions.H) center = new Vector2(lerped.x, pos.y);
			else center = new Vector2(pos.x, lerped.y);
			canvas.Arc(center, Radius, false, Color.black, 1, true, DotColor, 0, 100, 30);
		}

        Vector2[] arr = new Vector2[4]
        {
            new Vector2(pos.x, pos.y),
            new Vector2(pos.x + lerped.x, pos.y),
            new Vector2(pos.x + lerped.x, pos.y + lerped.y),
            new Vector2(pos.x, pos.y + lerped.y)
        };
        return arr;
    }

    protected override void DrawScale()
    {
        base.DrawScale();
        if (!DisplayValue) return;
		// Draw info text
		int Len = Data.Items.Length;
		float gap = 5;
        float labelWidth = 200;
        if (Data.dir == ChartVO.Directions.V)
            labelWidth = Mathf.Min(boundary.width/(Data.Items.Length+1), 200);
		for (int i = 0; i < Len; i++)
		{
			Text valueLabel = CreateLabel(Data.Items[i].value.ToString(), labelWidth);
			Vector2 pos = GetItemPos(i);
            Vector2 siz = GetItemSize(Data.Items[i]);
			if (Data.dir == ChartVO.Directions.H)
			{
				valueLabel.alignment = TextAnchor.MiddleLeft;
				valueLabel.rectTransform.pivot = new Vector2(0, 0);
				valueLabel.rectTransform.localPosition = new Vector2(siz.x + gap, pos.y);
			}
			else
			{
				valueLabel.alignment = TextAnchor.MiddleCenter;
				valueLabel.rectTransform.pivot = new Vector2(0.5f, 0);
                float halfWidth = (boundary.width/Data.Items.Length)*ChartItemSizeWeightage/2;
				valueLabel.rectTransform.localPosition = new Vector2(pos.x + halfWidth, siz.y);
			}
		}
    }
}
