using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UIGraphicAPI
{
	public static class UILine
	{
		public static void DrawLine(this UICanvas canvas, List<UIVertex> vertices, List<int> indices, UILineVO line)
		{
			if (line.fill)
			{
				FillPolygon(canvas, vertices, indices, line);
			}
			if (line.stroke)
			{
				canvas.StrokePolygon(vertices, indices, line.points, line.thickness, line.color);
			}
		}

		public static void StrokePolygon(this UICanvas canvas, List<UIVertex> vertices, List<int> indices, List<Vector2> points, float thickness, Color32 strokeColor)
		{
			if (points == null || points.Count < 2) return;
			int countStart = vertices.Count;

			for (int i = 0; i < points.Count - 1; i++)
			{
				Vector2 p0 = points[i];
				Vector2 p1 = points[i+1];
				float r = Vector2.Distance(p0, p1);
				float cosA = (p1.x - p0.x) / r;
				float radA = Mathf.Acos(cosA);
				if (p1.y < p0.y) radA += (Mathf.PI - radA)*2;
				float rad90 = Mathf.PI / 2;
				// float degA = Mathf.Rad2Deg * radA;
				Vector3 v0 = Vector3.zero;
				Vector3 v1 = Vector3.zero;
				Vector3 v2 = Vector3.zero;
				Vector3 v3 = Vector3.zero;
				float half = thickness/2;
				v0.x = p0.x + half * Mathf.Cos(radA - rad90);
				v0.y = p0.y + half * Mathf.Sin(radA - rad90);
				v1.x = p1.x + half * Mathf.Cos(radA - rad90);
				v1.y = p1.y + half * Mathf.Sin(radA - rad90);
				v2.x = p1.x + half * Mathf.Cos(radA + rad90);
				v2.y = p1.y + half * Mathf.Sin(radA + rad90);
				v3.x = p0.x + half * Mathf.Cos(radA + rad90);
				v3.y = p0.y + half * Mathf.Sin(radA + rad90);

				UIVertex[] vertex;
				if (canvas.LineSprite)
				{
					Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(canvas.LineSprite);
					Vector2[] uvs = new Vector2[4]{
						new Vector2(outer.x, outer.y),
						new Vector2(outer.z, outer.y),
						new Vector2(outer.z, outer.w),
						new Vector2(outer.x, outer.w)
					};
					vertex = canvas.SetVbo(new[]{v0, v1, v2, v3}, uvs, strokeColor);
				}
				else
					vertex = canvas.SetVbo(new[]{v0, v1, v2, v3}, UICanvas.uvVertices, strokeColor);

				int start = vertices.Count;
				int[] newIndices = new []{start, start+1, start+2, start+2, start+3, start};
				vertices.AddRange(vertex);
				indices.AddRange(newIndices);
			}

			int vCount = vertices.Count;
			// 画锚点
			for (int i = countStart; i < vCount - 4; i += 4)
			{
				indices.AddRange(new []{i + 1, i + 2, i + 4});
				indices.AddRange(new []{i + 1, i + 2, i + 7});
			}
			// 如果起始点和结束点坐标一样，则需要锚接这两个点
			Vector2 first = points.First();
			Vector2 last = points.Last();
			if (first.Equals(last))
			{
				indices.AddRange(new []{countStart, countStart + 3, vCount - 3});
			}
		}

		private static void FillPolygon(UICanvas canvas, List<UIVertex> vertices, List<int> indices, UILineVO line)
		{
			var points = line.points.GetRange(0, line.points.Count - 1);
			canvas.FillPolygon(vertices, indices, points, line.fillColor, line.name);
		}

		public static void FillPolygon(this UICanvas canvas, List<UIVertex>vertices, List<int> indices, List<Vector2> points, Color32 fillColor, string polygonName = "")
		{
			int[] frontQuad = DoFillPolygon(canvas, vertices, indices, points, fillColor, 0, polygonName);

			indices.AddRange(frontQuad);
		}

		private static int[] DoFillPolygon(UICanvas canvas, List<UIVertex>vertices, List<int> indices, List<Vector2> points, Color32 fillColor, float depth, string polygonName = "")
		{
			if (points == null) return null;
			List<UIVertex> vs = new List<UIVertex>();
			for (int i = 0; i < points.Count; i++)
			{
				UIVertex v = UIVertex.simpleVert;
				v.position = new Vector3(points[i].x, points[i].y, depth);
				v.color = fillColor;
				v.uv0 = UICanvas.uvVertices[0];
				vs.Add(v);
			}
			if (canvas.OnPopulatePolygon != null)
			{
				UIVertex[] arr = vs.ToArray();
				var polygons = canvas.OnPopulatePolygon(arr, polygonName);
				List<int> newIndices = new List<int>();
				polygons.ForEach(
					p => {
						vs = new List<UIVertex>(p);
						newIndices.AddRange(AddPolygon(vs, vertices));
					}
				);
				return newIndices.ToArray();
			}
			else
				return AddPolygon(vs, vertices);
			// var tri = new Triangulator(points.ToArray());
			// int[] newIndices = tri.Triangulate();
			// int start = vertices.Count;
			// AddOn(start, newIndices);
			// vertices.AddRange(vs);
		}

		private static int[] AddPolygon(List<UIVertex> newVertices, List<UIVertex> vertices)
		{
			var tri = new Triangulator(Vertex2Vector(newVertices.ToArray()));
			int[] newIndices = tri.Triangulate();

			// int[] newIndices = Triangulate(Vertex2Vector(newVertices.ToArray()));

			int start = vertices.Count;
			AddOn(start, newIndices);
			vertices.AddRange(newVertices);
			return newIndices;
		}

		private static Vector2[] Vertex2Vector(UIVertex[] vertices)
		{
			Vector2[] arr = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				arr[i] = vertices[i].position;
			}
			return arr;
		}

		private static void AddOn(int add, int[] indices)
		{
			for (int i = 0; i < indices.Length; i++)
			{
				indices[i] += add;
			}
		}
	}
}