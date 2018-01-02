using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
 

namespace UIGraphicAPI
{
    public class UICircle : UICanvas
    {
        public UICircleVO Data;

        #if UNITY_EDITOR
        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            if (Data != null) Redraw();
        }
        #endif

        private void Redraw()
        {
            if (!DrawingGraphics.Contains(Data)) DrawingGraphics.Add(Data);
        }
    }


    public static class UICircleExt
    {
        public static void DrawCircle(this UICanvas canvas, List<UIVertex> vertices, List<int> indices, UICircleVO circle)
        {
            float f = (circle.fillAmount / 100f);
            float fs = (circle.fillStart / 100f);
            float degrees = 360f / circle.segments;
            float fa = (circle.segments) * f;
            float start = (circle.segments) * fs;
    
            List<Vector2> allV = new List<Vector2>();
            float i = start * degrees;
            float end = (fa + start) * degrees;
            while (i <= end)
            {
                float rad = Mathf.Deg2Rad * i;
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);

                Vector2 p0 = new Vector2(circle.center.x + circle.radius*c, circle.center.y + circle.radius*s);     // 外边框 顶点0
                allV.Add(p0);
                if (i == end) break;
                
                i = Mathf.Min(i + degrees, end);
            }

            List<Vector2> polygonVertices = allV.ToArray().ToList();
            if (circle.fillAmount < 100)
                polygonVertices.Add(circle.center);
            else
                polygonVertices = polygonVertices.GetRange(0, polygonVertices.Count - 1);

            if (circle.fill) canvas.FillPolygon(vertices, indices, polygonVertices, circle.fillColor);
            if (circle.stroke) canvas.StrokePolygon(vertices, indices, allV, circle.thickness, circle.color);
        }
    }
}