using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : UIChart<BarVO>
{
  public bool ShowSideScale = true;

  private Dictionary<ChartItemVO, Vector2[]> m_ItemVertexDic = new Dictionary<ChartItemVO, Vector2[]>();

  /// <summary>
  /// Start is called on the frame when a script is enabled just before
  /// any of the Update methods is called the first time.
  /// </summary>
  public override void Start()
  {
    base.Start();
    // canvas.OnPopulatePolygon += OnPopulatePolygon;
    // m_ItemVertexDic.Clear();
  }

  protected override Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp)
  {
    Vector2 sizLerp = GetLerpedSize(size, lerp);
    canvas.strokeStyle.stroke = false;
    canvas.strokeStyle.fill = true;
    canvas.strokeStyle.fillColor = item.color;
    canvas.Rect(pos.x, pos.y, sizLerp.x, sizLerp.y);

    Vector2[] arr = new Vector2[4]
    {
            new Vector2(pos.x, pos.y),
            new Vector2(pos.x + sizLerp.x, pos.y),
            new Vector2(pos.x + sizLerp.x, pos.y + sizLerp.y),
            new Vector2(pos.x, pos.y + sizLerp.y)
    };
    // m_ItemVertexDic.Add(item, arr);
    return arr;
  }

  protected override void DrawScale()
  {
    RecycleLabel();
    if (!ShowScale) return;
    if (ShowSideScale) base.DrawScale();
    if (ShowLabel) base.DrawLabel();
    // Draw info text
    int Len = Data.Items.Length;
    float gap = 5;
    for (int i = 0; i < Len; i++)
    {
      Text valueLabel = CreateLabel(Data.Items[i].value.ToString());
      var filter = valueLabel.gameObject.AddComponent<ContentSizeFitter>();
      filter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
        float halfWidth = (boundary.width / Data.Items.Length) * ChartItemSizeWeightage / 2;
        valueLabel.rectTransform.localPosition = new Vector2(pos.x + halfWidth, siz.y + boundary.y);
      }
    }
  }

  protected List<UIVertex[]> OnPopulatePolygon(UIVertex[] vertices, string name)
  {
    if (vertices.Length != 4) return null;
    ChartItemVO data = FindItem(vertices);
    if (data == null) return null;
    Sprite sprite = canvas.MainSprite;
    if (sprite)
    {
      var uv = (sprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(sprite) : Vector4.zero;
      vertices[0].uv0 = new Vector2(uv.x, uv.y);
      vertices[1].uv0 = new Vector2(uv.z, uv.y);
      vertices[2].uv0 = new Vector2(uv.z, uv.w);
      vertices[3].uv0 = new Vector2(uv.x, uv.w);
      vertices[0].color = Color.white;
      vertices[1].color = Color.white;
      vertices[2].color = Color.white;
      vertices[3].color = Color.white;
    }
    else
    {
      vertices[0].color = data.ColorBL;
      vertices[1].color = data.ColorBR;
      vertices[2].color = data.ColorTR;
      vertices[3].color = data.ColorTL;
      vertices[0].uv0 = Vector2.zero;
      vertices[1].uv0 = Vector2.zero;
      vertices[2].uv0 = Vector2.zero;
      vertices[3].uv0 = Vector2.zero;
    }
    var list = new List<UIVertex[]>();
    list.Add(vertices);
    return list;
  }

  private ChartItemVO FindItem(UIVertex[] vertices)
  {
    foreach (KeyValuePair<ChartItemVO, Vector2[]> kv in m_ItemVertexDic)
    {
      bool equal = true;
      for (int i = 0; i < vertices.Length; i++)
      {
        Vector2 pos = vertices[i].position;
        Vector2 val = kv.Value[i];
        equal = pos.Equals(val);
        if (!equal) break;
      }
      if (equal) return kv.Key;
    }
    return null;
  }

  /// <summary>
  /// This function is called when the MonoBehaviour will be destroyed.
  /// </summary>
  void OnDestroy()
  {
    if (canvas) canvas.OnPopulatePolygon -= OnPopulatePolygon;
  }
}
