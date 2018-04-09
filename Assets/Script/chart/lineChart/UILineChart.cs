using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILineChart : UIChart<LineChartVO>
{
    public float Radius = 10;
    public float Thickness = 3;

    protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
    {
        Vector2 lerpedSize = GetLerpedSize(size, lerp);
        canvas.strokeStyle.stroke = false;
        Vector2 center = Vector2.zero;
        if (Data.dir == BarVO.Directions.V)
        {
            center.x = pos.x;
            center.y = lerpedSize.y;
        }
        else
        {
            center.x = lerpedSize.x;
            center.y = pos.y;
        }
        canvas.Arc(center, Radius, false, Color.white, 1, true, item.color, 0, 100, 30);

        float degrees = 1f;
        int fa = 361;
 
        List<Vector2> allV = new List<Vector2>();
        for (int i = 0; i < fa; i++)
        {
            float rad = Mathf.Deg2Rad * (i * degrees);
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);

            Vector2 p0 = new Vector2(center.x + Radius*c, center.y + Radius*s);     // 外边框 顶点0
            allV.Add(p0);
        }
		return allV.ToArray();
    }

    protected override void BeforeDrawItems(float lerp)
    {
        canvas.strokeStyle.thickness = Thickness;
        canvas.strokeStyle.strokeColor = Data.color;
        Vector2 pos = GetItemPos(0);
        Vector2 siz = GetItemSize(Data.Items[0]);
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
        for (int i = 1; i < Data.Items.Length; i++)
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
        canvas.Stroke();
    }
}


[Serializable]
public class LineChartVO : ChartVO, IChartVO
{
    [SerializeField]
    private Directions m_Dir;
    [SerializeField]
    private LineChartItemVO[] m_Items;

	public Color32 color = Color.white;

	public float maxValue { get { return getMaxValue(Items); } }
	public float unitScale { get { return getUnitScale(Items); } }
	public float numScale { get { return getNumScale(Items); } }
	public float maxScale { get { return getMaxScale(Items); } }

    public Directions dir { get { return m_Dir; } set { m_Dir = value; } }
    public ChartItemVO[] Items { get { return m_Items; } set { m_Items = value as LineChartItemVO[]; } }
}

[Serializable]
public class LineChartItemVO : ChartItemVO
{
    public int range;
}