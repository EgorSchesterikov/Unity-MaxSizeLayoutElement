using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MaxSizeLayoutElement : UIBehaviour, ILayoutElement
{
    [Header("Max Sizes")]
    [SerializeField] private float maxWidth = -1f;
    [SerializeField] private float maxHeight = -1f;

    [Header("Priority")]
    [SerializeField] private int priority = 1;
    
    public float MaxWidth 
    { 
        get => maxWidth; 
        set { if (Mathf.Approximately(maxWidth, value)) return; maxWidth = value; SetDirty(); } 
    }
    
    public float MaxHeight 
    { 
        get => maxHeight; 
        set { if (Mathf.Approximately(maxHeight, value)) return; maxHeight = value; SetDirty(); } 
    }

    public int Priority 
    { 
        get => priority; 
        set { if (priority == value) return; priority = value; SetDirty(); } 
    }

    private readonly List<ILayoutElement> _components = new List<ILayoutElement>();
    
    private float _calcMinWidth, _calcMinHeight;
    private float _calcPrefWidth, _calcPrefHeight;
    private float _calcFlexWidth, _calcFlexHeight;
    
    public int layoutPriority => priority;

    public float minWidth => _calcMinWidth;
    public float minHeight => _calcMinHeight;

    public float preferredWidth => (maxWidth >= 0 && _calcPrefWidth > maxWidth) ? maxWidth : _calcPrefWidth;
    public float preferredHeight => (maxHeight >= 0 && _calcPrefHeight > maxHeight) ? maxHeight : _calcPrefHeight;

    public float flexibleWidth => maxWidth >= 0 ? 0f : _calcFlexWidth;
    public float flexibleHeight => maxHeight >= 0 ? 0f : _calcFlexHeight;

    public void CalculateLayoutInputHorizontal()
    {
        GetComponents(_components);

        float min = -1f, pref = -1f, flex = -1f;
        int pMin = -1, pPref = -1, pFlex = -1;
        
        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];

            if (c == null || ReferenceEquals(c, this)) continue;

            int p = c.layoutPriority;

            float cMin = c.minWidth;
            if (p > pMin) { pMin = p; min = cMin; }
            else if (p == pMin && cMin > min) min = cMin;

            float cPref = c.preferredWidth;
            if (p > pPref) { pPref = p; pref = cPref; }
            else if (p == pPref && cPref > pref) pref = cPref;

            float cFlex = c.flexibleWidth;
            if (p > pFlex) { pFlex = p; flex = cFlex; }
            else if (p == pFlex && cFlex > flex) flex = cFlex;
        }

        _calcMinWidth = min >= 0 ? min : 0;
        _calcPrefWidth = pref >= 0 ? pref : 0;
        _calcFlexWidth = flex >= 0 ? flex : 0;
    }

    public void CalculateLayoutInputVertical()
    {
        float min = -1f, pref = -1f, flex = -1f;
        int pMin = -1, pPref = -1, pFlex = -1;

        for (int i = 0; i < _components.Count; i++)
        {
            var c = _components[i];
            if (c == null || ReferenceEquals(c, this)) continue;

            int p = c.layoutPriority;

            float cMin = c.minHeight;
            if (p > pMin) { pMin = p; min = cMin; }
            else if (p == pMin && cMin > min) min = cMin;

            float cPref = c.preferredHeight;
            if (p > pPref) { pPref = p; pref = cPref; }
            else if (p == pPref && cPref > pref) pref = cPref;

            float cFlex = c.flexibleHeight;
            if (p > pFlex) { pFlex = p; flex = cFlex; }
            else if (p == pFlex && cFlex > flex) flex = cFlex;
        }

        _calcMinHeight = min >= 0 ? min : 0;
        _calcPrefHeight = pref >= 0 ? pref : 0;
        _calcFlexHeight = flex >= 0 ? flex : 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        SetDirty();
        base.OnDisable();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetDirty();
    }
#endif

    private void SetDirty()
    {
        if (!IsActive()) return;
        LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
    }
}
