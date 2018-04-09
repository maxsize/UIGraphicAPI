using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(UICanvas))]
public abstract class UIChart<T> : MonoBehaviour where T : IChartVO
{
  public ChartInteractiveEvent OnMouseOverPolygon = new ChartInteractiveEvent();
  public ChartInteractiveEvent OnMouseOutPolygon = new ChartInteractiveEvent();
  public ChartClickEvent OnChartItemClicked = new ChartClickEvent();

  public Font LabelFont;
  public Text ScaleLabelPrefab;

  public bool ShowScale = true;
  public bool ShowLabel = true;
  public bool ShowAxis = true;
  public float RulerLabelGap = 20;
  public float NameLabelGap = 20;

  public bool Interactable = false;

  public T Data;

  public float ChartItemSizeWeightage = 0.4f;

  public bool DrawAtStart = false;
  public bool AnimateEffect = false;
  public float AnimateDuration = 1f;

  public string Unit = null;
  public Color32 AxiesColor = Color.white;
  public Color32 LabelColor = Color.gray;
  public int LabelFontSize = 18;
  public bool BestFitLabel = false;
  public bool LeaveEmptySpaceOnSides = true;

  protected UICanvas canvas;
  protected Rect boundary;

  private List<Text> m_LabelPool;
  private List<Text> m_UsingLabel;
  private bool m_PendingDraw;

  private Dictionary<ChartItemVO, Vector2[]> m_Polygons = new Dictionary<ChartItemVO, Vector2[]>();
  private Vector2[] m_CurrentPolygon;

  // Use this for initialization
  public virtual void Start()
  {
    m_LabelPool = new List<Text>();
    m_UsingLabel = new List<Text>();
    RectTransform rt = GetComponent<RectTransform>();
    // boundary = new Rect(rt.localPosition.x, rt.localPosition.y, rt.sizeDelta.x, rt.sizeDelta.y);
    boundary = rt.rect;
    canvas = GetComponent<UICanvas>();
    if (m_PendingDraw || DrawAtStart)
    {
      Draw();
      m_PendingDraw = false;
    }
  }

  /// <summary>
  /// Update is called every frame, if the MonoBehaviour is enabled.
  /// </summary>
  void Update()
  {
    if (Interactable)
    {
      Vector2 local;
      Canvas cvs = transform.root.GetComponent<Canvas>();
      RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, Input.mousePosition, cvs.worldCamera, out local);
      var poly = HitTestUtil.HitTest(m_Polygons.Values.ToArray(), local);
      if (poly != null && m_CurrentPolygon == null)
      {
        OnMouseOverPolygon.Invoke(Array.Find(m_Polygons.ToArray(), p => p.Value == poly).Key, poly);
        m_CurrentPolygon = poly;
      }
      else if (poly == null && m_CurrentPolygon != null)
      {

        OnMouseOutPolygon.Invoke(Array.Find(m_Polygons.ToArray(), p => p.Value == poly).Key, m_CurrentPolygon);
        m_CurrentPolygon = null;
      }

      if (m_CurrentPolygon != null && Input.GetMouseButtonDown(0))
      {
        // Mouse left down
        ChartItemVO data = Array.Find(m_Polygons.ToArray(), p => p.Value == poly).Key;
        OnChartItemClicked.Invoke(data.data as JSONNode);
      }
    }
  }

  public void Draw()
  {
    if (canvas == null)
    {
      m_PendingDraw = true;
      return;
    }
    ResetBeforeRedraw();
    DrawScale();

    canvas.drawingGraphics.Clear();
    canvas.Clear();
    if (Data == null) return;
    if (AnimateEffect)
    {
      StopCoroutine("AnimateChart");
      StartCoroutine("AnimateChart", AnimateDuration);
    }
    else
    {
      BeforeDrawItems(1);
      DrawItems(1);
      AfterDrawItems(1);
      DrawAxies();
      canvas.SetVerticesDirty();
    }
  }

  protected virtual void ResetBeforeRedraw() { }

  protected virtual void BeforeDrawItems(float lerp)
  {
  }

  /// <summary>
  /// Called after all items are drawn, and before drawing axies
  /// </summary>
  protected virtual void AfterDrawItems(float lerp)
  {
  }

  private IEnumerator AnimateChart(float totalTime)
  {
    float cur = 0;
    while (cur <= totalTime)
    {
      float lerp = Mathf.Sin((cur / totalTime) * Mathf.PI / 2);
      canvas.drawingGraphics.Clear();
      canvas.Clear();
      BeforeDrawItems(lerp);
      DrawItems(lerp);
      AfterDrawItems(lerp);
      DrawAxies();
      if (cur == totalTime) break;
      cur = Mathf.Clamp(cur + Time.deltaTime, 0, totalTime);
      canvas.SetVerticesDirty();
      yield return null;
    }
    canvas.SetVerticesDirty();
  }

  protected virtual void DrawAxies()
  {
    if (!ShowAxis) return;
    canvas.strokeStyle.stroke = true;
    canvas.strokeStyle.fill = false;
    canvas.strokeStyle.strokeColor = AxiesColor;
    canvas.strokeStyle.thickness = 1;
    canvas.MoveTo(0, boundary.height);
    canvas.LineTo(0, 0);
    canvas.LineTo(boundary.width, 0);
    canvas.Stroke();
  }

  protected virtual void DrawScale()
  {
    if (!ShowScale) return;
    RecycleLabel();

    var numScale = Data.numScale;
    var unitScale = Data.unitScale;
    float maxScale = numScale * unitScale;
    float size = Data.dir == BarVO.Directions.V ? boundary.height : boundary.width;
    const float len = 4;

    float labelWidth = 200;
    if (Data.dir == ChartVO.Directions.V)
      labelWidth = Mathf.Min(boundary.width / (Data.Items.Length + 1), 200);
    // Draw scale on ruler
    for (int i = 0; i <= numScale; i++)
    {
      if (i == numScale && string.IsNullOrEmpty(Unit)) continue;
      float pos = ((i * unitScale) / maxScale) * size;
      Text txt = CreateLabel((unitScale * i).ToString(), labelWidth);
      if (i == numScale) txt.text = Unit;
      if (Data.dir == BarVO.Directions.V)
      {
        canvas.MoveTo(0, pos);
        canvas.LineTo(-len, pos);
        txt.alignment = TextAnchor.LowerRight;
        txt.rectTransform.localPosition = new Vector2(-RulerLabelGap, pos);
        txt.rectTransform.pivot = new Vector2(1, 0);
      }
      else
      {
        canvas.MoveTo(pos, 0);
        canvas.LineTo(pos, -len);
        txt.alignment = TextAnchor.LowerCenter;
        txt.rectTransform.localPosition = new Vector2(pos, -txt.rectTransform.sizeDelta.y - RulerLabelGap);
        txt.rectTransform.pivot = new Vector2(0.5f, 0);
      }
      canvas.Stroke();
    }
  }

  protected void DrawLabel()
  {
    if (!ShowLabel) return;
    float labelWidth = 200;
    if (Data.dir == ChartVO.Directions.V)
      labelWidth = Mathf.Min(boundary.width / (Data.Items.Length + 1), 200);
    // Draw info text
    int Len = Data.Items.Length;
    for (int i = 0; i < Len; i++)
    {
      Text txt = CreateLabel(Data.Items[i].label, labelWidth);
      Vector2 pos = GetItemPos(i);
      if (Data.dir == ChartVO.Directions.H)
      {
        txt.alignment = TextAnchor.LowerRight;
        txt.rectTransform.localPosition = new Vector2(-NameLabelGap, pos.y);
        txt.rectTransform.pivot = new Vector2(1, 0);
      }
      else
      {
        txt.alignment = TextAnchor.LowerCenter;
        float halfWidth = (boundary.width / Data.Items.Length) * ChartItemSizeWeightage / 2;
        pos.x += halfWidth;
        pos.y -= NameLabelGap;
        txt.rectTransform.localPosition = pos;
        txt.rectTransform.pivot = new Vector2(0.5f, 1);
      }
    }
  }

  protected void RecycleLabel()
  {
    while (m_UsingLabel.Count > 0)
    {
      Text t = m_UsingLabel[0];
      m_LabelPool.Add(t);
      m_UsingLabel.Remove(t);
      t.gameObject.SetActive(false);
    }
  }

  protected Text CreateLabel(string value, float lableWidth = 200, float lableHeight = 30)
  {
    Text text;
    if (m_LabelPool.Count > 0)
    {
      text = m_LabelPool[0];
      text.gameObject.SetActive(true);
      m_LabelPool.RemoveAt(0);
    }
    else
    {
      if (ScaleLabelPrefab == null)
      {
        var go = new GameObject();
        text = go.AddComponent<Text>();
      }
      else
        text = Instantiate(ScaleLabelPrefab);
    }

    text.transform.SetParent(transform);
    text.transform.localScale = Vector3.one;
    text.transform.localRotation = Quaternion.identity;
    text.transform.localPosition = new Vector3(text.transform.localPosition.x, text.transform.localPosition.y, 0);
    text.fontStyle = FontStyle.Normal;
    text.color = LabelColor;
    text.font = LabelFont == null ? Font.CreateDynamicFontFromOSFont("FZLanTingHei-R-GBK", LabelFontSize) : LabelFont;
    text.fontSize = LabelFontSize;
    text.rectTransform.anchorMin = Vector2.zero;
    text.rectTransform.anchorMax = Vector2.zero;
    text.rectTransform.pivot = Vector2.one;
    text.rectTransform.sizeDelta = new Vector2(lableWidth, lableHeight);
    text.text = value;
    text.resizeTextForBestFit = BestFitLabel;
    text.resizeTextMaxSize = 80;
    text.resizeTextMinSize = 14;
    m_UsingLabel.Add(text);
    return text;
  }

  void DrawItems(float lerp)
  {
    m_Polygons.Clear();
    for (int i = 0; i < Data.Items.Length; i++)
    {
      DrawItem(Data.Items[i], i, lerp);
    }
  }

  protected void DrawItem(ChartItemVO item, int index, float lerp)
  {
    Vector2 siz = GetItemSize(item);
    // Vector2 sizLerp = Vector2.Lerp(Vector2.zero, siz, lerp);
    Vector2 pos = GetItemPos(index);
    // if (Data.dir == ChartVO.Directions.H)
    // {
    // 	siz.x = sizLerp.x;
    // }
    // else
    // {
    // 	siz.y = sizLerp.y;
    // }

    var v = FillItem(item, pos, siz, lerp);
    m_Polygons.Add(item, v);
  }

  protected abstract Vector2[] FillItem(ChartItemVO item, Vector2 pos, Vector2 size, float lerp);

  protected Vector2 GetLerpedSize(Vector2 siz, float lerp)
  {
    Vector2 sizLerp = Vector2.Lerp(Vector2.zero, siz, lerp);
    if (Data.dir == ChartVO.Directions.H)
    {
      sizLerp.y = siz.y;
    }
    else
    {
      sizLerp.x = siz.x;
    }
    return sizLerp;
  }

  protected Vector2 GetItemPos(int index)
  {
    float space = 0;
    float itemSize = 0;
    if (Data.dir == BarVO.Directions.V)
    {
      itemSize = (boundary.width / Data.Items.Length) * ChartItemSizeWeightage;
      if (LeaveEmptySpaceOnSides)
      {
        space = boundary.width / (Data.Items.Length + 1);
        space *= (index + 1);
      }
      else
      {
        space = boundary.width / (Data.Items.Length - 1);
        space *= index;
      }
      return new Vector2(space - itemSize / 2 + boundary.x, 0 + boundary.y);
    }
    else
    {
      itemSize = (boundary.height / Data.Items.Length) * ChartItemSizeWeightage;
      if (LeaveEmptySpaceOnSides)
      {
        space = boundary.height / (Data.Items.Length + 1);
        space *= (index + 1);
      }
      else
      {
        space = boundary.height / (Data.Items.Length - 1);
        space *= index;
      }
      return new Vector2(boundary.x, space - itemSize / 2 + boundary.y);
    }
  }

  protected Vector2 GetItemSize(ChartItemVO item)
  {
    if (Data.dir == BarVO.Directions.V) return new Vector2((boundary.width / Data.Items.Length) * ChartItemSizeWeightage, boundary.height * (item.value / Data.maxScale));
    else return new Vector2(boundary.width * (item.value / Data.maxScale), (boundary.height / Data.Items.Length) * ChartItemSizeWeightage);
  }
}

[Serializable]
public class ChartInteractiveEvent : UnityEvent<ChartItemVO, Vector2[]> { }
[Serializable]
public class ChartClickEvent : UnityEvent<JSONNode> { }