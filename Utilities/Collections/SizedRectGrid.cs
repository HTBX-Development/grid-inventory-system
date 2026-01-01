using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizedRectGrid<TData>
{
    public List<ContainedElement> Elements = new List<ContainedElement>();
    public Rect Bounds { get; protected set; }

    #region Methods

    public bool Insert(TData data, Rect rect)
    {
        if (!Bounds.ContainsRect(rect)) return false;

        Elements.Add(new ContainedElement(rect, data));
        
        return true;
    }

    public bool TryGetDataAtPosition(Vector2 pos, out List<TData> data)
    {
        data = new List<TData>();
        foreach (var element in Elements)
        {
            if(!element.Rect.Contains(pos)) continue;
            
            data.Add(element.Data);
        }

        return data.Count != 0;
    }

    #endregion
    
    #region Constructors

    public SizedRectGrid(Rect bounds)
    {
        Bounds = bounds;
    }
    
    #endregion
    
    public struct ContainedElement
    {
        public readonly Rect Rect;
        public readonly TData Data;

        public ContainedElement(Rect rect, TData data)
        {
            Rect = rect;
            Data = data;
        }
    }
}
