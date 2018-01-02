using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UIGraphicAPI
{
	public static class UIDrawing {
		public static UILineVO MoveTo(this UICanvas canvas, float x, float y)
		{
			return MoveTo(canvas, new Vector2(x, y));
		}

		public static UILineVO MoveTo(this UICanvas canvas, Vector2 point)
		{
			UILineVO line = new UILineVO();
			line.points.Add(point);
			line.fill = canvas.strokeStyle.fill;
			line.stroke = canvas.strokeStyle.stroke;
			line.fillColor = canvas.strokeStyle.fillColor;
			line.color = canvas.strokeStyle.strokeColor;
			line.thickness = canvas.strokeStyle.thickness;
			canvas.Strokes.Add(line);
			return line;
		}

		public static UILineVO LineTo(this UICanvas canvas, float x, float y)
		{
			return LineTo(canvas, new Vector2(x, y));
		}

		public static UILineVO LineTo(this UICanvas canvas, Vector2 point)
		{
			UILineVO line = canvas.Strokes.Last();
			if (line == null)
			{
				return canvas.MoveTo(point);
			}
			line.points.Add(point);
			return line;
		}

		public static void Stroke(this UICanvas canvas)
		{
			canvas.DrawingGraphics.AddRange(canvas.Strokes.ToArray());
			canvas.Strokes.Clear();
		}

		public static UICircleVO Arc(this UICanvas canvas, Vector2 center, float radius, bool stroke, Color32 strokeColor, float thickness, bool fill, Color32 fillColor, float fillStart = 0, float fillAmount = 100, int segments = 360)
		{
			var vo = new UICircleVO();
			vo.center = center;
			vo.radius = radius;
			vo.fillAmount = fillAmount;
			vo.fill = fill;
			vo.fillColor = fillColor;
			vo.stroke = stroke;
			vo.color = strokeColor;
			vo.thickness = thickness;
			vo.segments = segments;
			vo.fillStart = fillStart;
			canvas.DrawingGraphics.Add(vo);
			return vo;
		}

		public static UIRectVO Rect(this UICanvas canvas, float x, float y, float width, float height)
		{
			var vo = new UIRectVO();
			vo.rect = new Rect(x, y, width, height);
			vo.fill = canvas.strokeStyle.fill;
			vo.fillColor = canvas.strokeStyle.fillColor;
			vo.thickness = canvas.strokeStyle.thickness;
			vo.strokeColor = canvas.strokeStyle.strokeColor;
			vo.stroke = canvas.strokeStyle.stroke;
			canvas.DrawingGraphics.Add(vo);
			return vo;
		}
	}

}