using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIGraphicAPI
{
	[ExecuteInEditMode]
	public class UIRect : UICanvas
	{
		public UIRectVO Data;

		#if UNITY_EDITOR
		void Update()
		{
			if (Data != null)
			{
				Redraw();
			}
		}
		#endif

        private void Redraw()
        {
			if (!DrawingGraphics.Contains(Data))
	            DrawingGraphics.Add(Data);
        }
    }

	public static class UIRectExt {
	
		public static void DrawRect(this UICanvas canvas, List<UIVertex> vertices, List<int> indices, UIRectVO rectVO)
		{
			Rect rect = rectVO.rect;
			Vector2 TL = rect.position;
			Vector2 TR = new Vector2(rect.xMax, rect.yMin);
			Vector2 BR = new Vector2(rect.xMax, rect.yMax);
			Vector2 BL = new Vector2(rect.xMin, rect.yMax);

			if (rectVO.fill)
			{
				var polygon = new List<Vector2>(){TL, TR, BR, BL};
				canvas.FillPolygon(vertices, indices, polygon, rectVO.fillColor, rectVO.name);
			}
			if (rectVO.stroke)
			{
				var polygon = new List<Vector2>(){TL, TR, BR, BL, TL};
				canvas.StrokePolygon(vertices, indices, polygon, rectVO.thickness, rectVO.strokeColor);
			}
		}
	}

}