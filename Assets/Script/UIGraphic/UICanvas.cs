using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace UIGraphicAPI
{
    [ExecuteInEditMode]
    public class UICanvas : MaskableGraphic {

        public delegate List<UIVertex[]> PopulatePolygonDelegate(UIVertex[] vertices, string name);
        public static readonly Vector2[] uvVertices = new[]{Vector2.zero, Vector2.right, new Vector2(1, 1), Vector2.up};

        [HideInInspector]
        public List<UIGraphicVO> DrawingGraphics = new List<UIGraphicVO>();
        [HideInInspector]
        public List<UILineVO> Strokes;

        public float Depth = 30;
        public float OffsetX = -15;
        public float OffsetY = 10;
        public float ColorLerpEnd = 0.7f;
        public float ColorLerpStart = 0.3f;
        public readonly StrokeStyleVO strokeStyle = new StrokeStyleVO();

        public Sprite MainSprite;
        public Sprite LineSprite;

        public PopulatePolygonDelegate OnPopulatePolygon;

        protected List<UIVertex> vertices = new List<UIVertex>();
        protected List<int> indices = new List<int>();

        public override Texture mainTexture
        {
            get
            {
                return MainSprite != null ? MainSprite.texture : base.mainTexture;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Clear();
            for (int i = 0; i < DrawingGraphics.Count; i++)
            {
                var vo = DrawingGraphics[i];
                if (vo is UILineVO) this.DrawLine(vertices, indices, (UILineVO)vo);
                else if (vo is UICircleVO) this.DrawCircle(vertices, indices, (UICircleVO)vo);
                else if (vo is UIRectVO) this.DrawRect(vertices, indices, (UIRectVO)vo);
            }
            ThreeDimentionAll();
            // ThreeDimentionEdge();
            vh.AddUIVertexStream(vertices, indices);
        }

        private void ThreeDimentionAll()
        {
            // Draw 3D part
            if (Depth <= 0) return;
            if (vertices.Count < 1) return;
            // Draw front side
            Color typicalColor = vertices[0].color;
            int front = vertices.Count;
            Color32 backColor = Color32.Lerp(typicalColor, Color.black, ColorLerpEnd);
            // add back side vertices
            for (int i = 0; i < front; i++)
            {
                var p = vertices[i];
                UIVertex v = new UIVertex()
                {
                    position = new Vector3(p.position.x + OffsetX, p.position.y + OffsetY, Depth),
                    color = backColor,
                    uv0 = UICanvas.uvVertices[0]
                };
                vertices.Add(v);
            }
            // add side triangles
            for (int i = 0; i < front - 1; i++)
            {
                if (Vector2.Distance(vertices[i].position, vertices[i+1].position) > 50) continue;
                int[] quad = new []{i, i+1, front+i+1, front+i+1, front+i, i};
                // indices.AddRange(quad);
                indices.InsertRange(0, quad);
            }
            int len = front - 1;
            int[] last = new []{0, len, front+len, 0, front, front+len};
            indices.InsertRange(0, last);
        }

        private int[] FindEdgeVertices()
        {
            List<int> list = new List<int>();
            UIVertex[] arrVert = vertices.ToArray();
            for (int i = 0; i < vertices.Count; i++)
            {
                if (HitTestUtil.IsEdge(arrVert, i)) list.Add(i);
            }
            return list.ToArray();
        }

        private void ThreeDimentionEdge()
        {
            // Draw 3D part
            if (Depth <= 0) return;
            // Draw front side
            // find vertices on the edge
            int[] edgeIndices = FindEdgeVertices();
            Color typicalColor = vertices[0].color;
            int numFront = vertices.Count;
            // add back side vertices
            Color32 backColor = Color32.Lerp(typicalColor, Color.black, ColorLerpEnd);
            for (int i = 0; i < edgeIndices.Length; i++)
            {
                int index = edgeIndices[i];
                var p = vertices[index];
                UIVertex v = new UIVertex()
                {
                    position = new Vector3(p.position.x + OffsetX, p.position.y + OffsetY, Depth),
                    color = backColor,
                    uv0 = UICanvas.uvVertices[0]
                };
                vertices.Add(v);
            }
            // add side triangles
            int numNewVerts = edgeIndices.Length;
            int front0 = 0;
            int back0 = 0;
            for (int i = 0; i < numNewVerts - 1; i++)
            {
                front0 = edgeIndices[i];
                back0 = numFront + i;
                int[] quad = new []{front0, front0+1, back0+1, back0+1, back0, front0};
                indices.InsertRange(0, quad);
            }
            // add last quad
            int len = numNewVerts - 1;
            front0 = edgeIndices[0];
            back0 = numFront;
            int frontLast = edgeIndices[len];
            int backLast = numFront + len;
            int[] last = new []{frontLast, front0, back0, back0, backLast, frontLast};
            indices.InsertRange(0, last);
        }

        public UIVertex[] SetVbo(Vector3[] vertices, Vector2[] uvs, Color32 color)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                // vert.uv0 = uvs[i];
                vert.GetType().GetField("uv"+i).SetValue(vert, uvs[i]);
                vert.uv0 = uvs[i];
                // vert.uv1 = uvs[1];
                // vert.uv2 = uvs[2];
                // vert.uv3 = uvs[3];
                vbo[i] = vert;
            }
            return vbo;
        }

        public void Clear()
        {
            vertices.Clear();
            indices.Clear();
        }
    }


    [Serializable]
    public class UILineVO : UIGraphicVO
    {
        public Color32 fillColor = Color.white;
        public bool fill = false;
        public bool stroke = true;
        public List<Vector2> points = new List<Vector2>();
    }

    [Serializable]
    public class UICircleVO : UIGraphicVO
    {
        [Range(3, 360)]
        public int segments = 360;
        public Vector2 center = Vector2.zero;
        public float radius = 10;
        public bool fill = false;
        public bool stroke = true;
        public Color32 fillColor = Color.white;
        [Range(0, 100)]
        public float fillAmount = 100;
        [Range(0, 100)]
        public float fillStart = 0;
    }

    [Serializable]
    public class UIRectVO : UIGraphicVO
    {
        public Rect rect;
        public bool fill;
        public Color32 fillColor;
        public Color32 strokeColor;
        public bool stroke;
    }

    [Serializable]
    public class UIGraphicVO
    {
        public Color32 color = Color.white;
        [Range(1, 100)]
        public float thickness = 1;
        public string name;
    }

    [Serializable]
    public class StrokeStyleVO
    {
        public Color32 fillColor = Color.white;
        public bool fill = false;
        public bool stroke = true;
        public float thickness = 1;
        public Color32 strokeColor = Color.white;
    }
}