using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIGraphicAPI
{
	public class HitTestUtil
	{

		public static Vector2[] HitTest(Vector2[][] polys, Vector2 point)
		{
			for (int i = 0; i < polys.Length; i++)
				if (HitTest(polys[i], point)) return polys[i];
			return null;
		}

		public static bool HitTest(Vector2[] poly, Vector2 point)
		{
			int nvert = poly.Length;
			int i, j = 0;
			bool c = false;
			for (i = 0, j = nvert-1; i < nvert; j = i++)
			{
				if ( ((poly[i].y>point.y) != (poly[j].y>point.y)) &&
				(point.x < (poly[j].x-poly[i].x) * (point.y-poly[i].y) / (poly[j].y-poly[i].y) + poly[i].x) )
					c = !c;
			}
			return c;
		}

		public static bool HitTest(UIVertex[] poly, Vector2 point)
		{
			int nvert = poly.Length;
			int i, j = 0;
			bool c = false;
			for (i = 0, j = nvert-1; i < nvert; j = i++)
			{
				if ( ((poly[i].position.y>point.y) != (poly[j].position.y>point.y)) &&
				(point.x < (poly[j].position.x-poly[i].position.x) * (point.y-poly[i].position.y) / (poly[j].position.y-poly[i].position.y) + poly[i].position.x) )
					c = !c;
			}
			return c;
		}

		public static bool IsEdge(UIVertex[] vertices, int index)
		{
			UIVertex v = vertices[index];
			return IsEdge(vertices, v.position);
		}

		public static bool IsEdge(UIVertex[] vertices, Vector2 point)
		{
			Vector2[] around = new Vector2[4]{
				new Vector2(point.x - 1f, point.y - 1f),
				new Vector2(point.x + 1f, point.y - 1f),
				new Vector2(point.x - 1f, point.y + 1f),
				new Vector2(point.x + 1f, point.y + 1f),
			};
			for (int i = 0; i < 4; i++)
			{
				// If any points of around hittest failed means it's an edge point
				if (!HitTest(vertices, around[i])) return true;
			}
			return false;
		}
	}
}
