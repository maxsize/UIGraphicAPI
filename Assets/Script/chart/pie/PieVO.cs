using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PieVO : IChartVO
{
	[SerializeField]
	private PiePieceVO[] m_Items;

	public float totalValue
	{
		get
		{
			if (m_Items == null || m_Items.Length == 0) return 0;
			float total = 0;
			m_Items.ToList<PiePieceVO>().ForEach(p => total += p.value);
			return total;
		}
	}

	/// <summary>
	/// Get pie piece fill info, x is starting position, y is fill amount
	/// </summary>
	public Vector2 GetItemFillInfo(PiePieceVO piece)
	{
		float totalValue = this.totalValue;
		float weight = (piece.value / totalValue);
		int index = Array.IndexOf(m_Items, piece);
		float startFill = 0;
		for (int i = 0; i < index; i++)
		{
			startFill += m_Items[i].value;
		}
		startFill = (startFill / totalValue);
		// float dicimalStart = startFill - (int)startFill;
		// startFill = (int)startFill;
		// weight = (int) weight + dicimalStart;
		return new Vector2(startFill * 100, weight * 100);
	}

    public float maxValue { get { return 0; } }

    public float unitScale { get { return 0; } }

    public float numScale { get { return 0; } }

    public float maxScale { get { return 0; } }

    public ChartVO.Directions dir { get;set; }
    public ChartItemVO[] Items { get { return m_Items; } set { m_Items = value as PiePieceVO[]; } }
}
