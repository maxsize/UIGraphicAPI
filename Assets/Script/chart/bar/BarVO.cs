using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class BarVO : ChartVO, IChartVO
{
	[SerializeField]
	private Directions m_Dir;

	[SerializeField]
	private BarItemVO[] m_Items;

	public Color32 Color;

	private float m_MaxValue = float.MinValue;
	private float m_UnitScale = float.MinValue;
	private float m_NumScale = float.MinValue;
	private float m_MaxScale = float.MinValue;

	public float maxValue { get { 
		if (m_MaxValue == float.MinValue) m_MaxValue = getMaxValue(m_Items);
		return m_MaxValue;
	 } }
	public float unitScale { get { 
		if (m_UnitScale == float.MinValue) m_UnitScale = getUnitScale(m_Items);
		return m_UnitScale;
	 } }
	public float numScale { get { 
		if (m_NumScale == float.MinValue) m_NumScale = getNumScale(m_Items);
		return m_NumScale;
	 } }
	public float maxScale { get { 
		if (m_MaxScale == float.MinValue) m_MaxScale = getMaxScale(m_Items);
		return m_MaxScale;
	 } }

    public Directions dir { get { return m_Dir;} set { m_Dir = value; } }

    public ChartItemVO[] Items {
		get { return m_Items;}
		set
		{
			m_Items = value as BarItemVO[];
			// Array.ForEach(m_Items, i => i.color = Color);
		}
	}
}

public interface IChartVO
{
	float maxValue {get;}
	float unitScale {get;}
	float numScale {get;}
	float maxScale {get;}
	ChartVO.Directions dir {get;set;}
	ChartItemVO[] Items {get;set;}
}

public class ChartVO
{

	public enum Directions
	{
		H = 0,
		V = 1
	}

	static readonly int[] SCALES = {1, 2, 5};
	static readonly int MAX_SCALES = 10;
	static readonly int MIN_SCALES = 3;

	protected float getMaxValue(ChartItemVO[] items)
	{
		if (items == null) return 0;
		float max = 0;
		items.ToList<ChartItemVO>().ForEach(d => {
			max = d.value > max ? d.value:max;
		});
		return max;
	}

	public int getUnitScale(ChartItemVO[] items)
	{
		float max = getMaxValue(items);
		if (max == 0) return 0;
		// int unit = SCALES[0];
		int multiplier = 1;
		// for (int k = 0; k < 10; k++)
		int value;
		bool notFound = true;
		while(notFound)
		{
			for (int i = 0; i < SCALES.Length; i++)
			{
				value = (int) max / (SCALES[i] * multiplier);
				notFound = !(value >= MIN_SCALES && value <= MAX_SCALES);
				// Debug.Log(string.Format("notFound:{0}, value:{1}", notFound, value));
				if (!notFound)
				{
					return SCALES[i] * multiplier;
				}
			}
			multiplier *= 10;
		}
		return SCALES[SCALES.Length - 1];
	}

	public int getNumScale(ChartItemVO[] items)
	{
		int us = getUnitScale(items);
		if (us == 0) return 0;
		float max = getMaxValue(items);
		int num = (int) Math.Ceiling(max / us);
		return num;
	}

	public int getMaxScale(ChartItemVO[] items)
	{
		return getNumScale(items) * getUnitScale(items);
	}
}
