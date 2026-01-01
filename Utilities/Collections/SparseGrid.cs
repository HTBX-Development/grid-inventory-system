using System.Collections.Generic;
using System.Linq;
using Hitbox.Collections.Data;
using UnityEngine;

namespace Hitbox.Collections
{
    public class SparseGrid<TData>
    {
        #region Fields

        private Dictionary<Vector2Int, GridElement> _elements = new();

        #region Indexers

        public TData this[Vector2Int pos]
        {
            get => GetDataAtPosition(pos);
            set => InsertElement(value, pos, Vector2Int.one);
        }

        public TData this[int x, int y]
        {
            get => GetDataAtPosition(x, y);
            set => InsertElement(value, new Vector2Int(x, y), Vector2Int.one);
        }

        public TData this[Vector2Int pos, Vector2Int size]
        {
            set => InsertElement(value, pos, size);
        }

        #endregion

        #endregion

        #region Methods

        #region Retrieval

        #region Data

        public TData GetDataAtPosition(Vector2Int pos)
        {
            return _elements.TryGetValue(pos, out GridElement element) ? element.Data : default;
        }

        public TData GetDataAtPosition(int x, int y)
        {
            return GetDataAtPosition(new Vector2Int(x, y));
        }
        
        public bool TryGetDataAtPosition(Vector2Int pos, out TData data)
        {
            if (_elements.TryGetValue(pos, out GridElement element))
            {
                data = element.Data;
                return true;
            }

            data = default;
            return false;
        }
        
        public bool TryGetDataAtPosition(int x, int y, out TData data)
        {
            return TryGetDataAtPosition(new Vector2Int(x, y), out data);
        }

        public TData[] GetAllData()
        {
            HashSet<TData> data = new HashSet<TData>();
            foreach (GridElement element in _elements.Values)
            {
                data.Add(element.Data);
            }

            return data.ToArray();
        }

        public bool ContainsData(TData data)
        {
            return GetAllData().Contains(data);
        }

        #endregion

        #region Elements

        public (TData, GridRect) GetElementAtPosition(Vector2Int pos)
        {
            return _elements.TryGetValue(pos, out GridElement element) ? element.ElementData : default;
        }

        public (TData, GridRect) GetElementAtPosition(int x, int y)
        {
            return GetElementAtPosition(new Vector2Int(x, y));
        }
        
        public (TData, GridRect)[] GetAllElementData()
        {
            HashSet<(TData, GridRect)> data = new HashSet<(TData, GridRect)>();
            foreach (GridElement element in _elements.Values)
            {
                data.Add(element.ElementData);
            }

            return data.ToArray();
        }

        // TODO: optimize!
        public (TData, GridRect) GetElementFromData(TData data)
        {
           GridElement[] elements = GetAllElements();

           foreach (GridElement element in elements)
           {
               if (element.Data.Equals(data)) return element.ElementData;
           }

           return default;
        }
        
        private GridElement[] GetAllElements()
        {
            HashSet<GridElement> data = new HashSet<GridElement>();
            foreach (GridElement element in _elements.Values)
            {
                data.Add(element);
            }

            return data.ToArray();
        }

        public bool ElementAt(Vector2Int position)
        {
            return _elements.ContainsKey(position);
        }

        public bool ElementIn(GridRect rect)
        {
            for (int y = rect.min.y; y < rect.max.y; y++)
            {
                for (int x = rect.min.x; x < rect.max.x; x++)
                {
                    if (ElementAt(new Vector2Int(x, y))) return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Insertion

        public bool InsertElement(TData data, Vector2Int position, Vector2Int dimensions)
        {
            GridRect rect = new GridRect(position, dimensions);
            // Checking if given rect overlaps anywhere
            if (ElementIn(rect)) return false;

            GridElement element = new GridElement(data, rect);

            // Updating all taken slots with new element.
            for (int y = rect.min.y; y < rect.max.y; y++)
            {
                for (int x = rect.min.x; x < rect.max.x; x++)
                {
                    _elements.Add(new Vector2Int(x, y), element);
                }
            }

            return true;
        }

        public bool InsertElement(TData data, GridRect rect)
        {
            return InsertElement(data, rect.position, rect.size);
        }

        #endregion

        #region Deletion

        public TData Remove(Vector2Int pos)
        {
            if (!ElementAt(pos)) return default;

            GridElement element = _elements[pos];

            // Clearing all taken slots
            for (int y = element.Dimensions.min.y; y < element.Dimensions.max.y; y++)
            {
                for (int x = element.Dimensions.min.x; x < element.Dimensions.max.x; x++)
                {
                    _elements.Remove(new Vector2Int(x, y));
                }
            }

            return element.Data;
        }
        
        public TData Remove(TData target)
        {
            GridElement[] elements = GetAllElements();
            
            foreach (GridElement element in elements)
            {
                if (element.Data.Equals(target))
                {
                    // Clearing all taken slots
                    for (int y = element.Dimensions.min.y; y < element.Dimensions.max.y; y++)
                    {
                        for (int x = element.Dimensions.min.x; x < element.Dimensions.max.x; x++)
                        {
                            _elements.Remove(new Vector2Int(x, y));
                        }
                    }

                    return element.Data;
                }
            }

            return default;
        }

        public (TData, GridRect) RemoveElement(Vector2Int pos)
        {
            if (!ElementAt(pos)) return default;

            GridElement element = _elements[pos];

            // Updating all taken slots with new element.
            for (int y = element.Dimensions.min.y; y < element.Dimensions.max.y; y++)
            {
                for (int x = element.Dimensions.min.x; x < element.Dimensions.max.x; x++)
                {
                    _elements.Remove(new Vector2Int(x, y));
                }
            }

            return element.ElementData;
        }

        /// <summary>
        /// Clear the entire grid
        /// </summary>
        public void Clear()
        {
            _elements.Clear();
        }

        #endregion

        #endregion

        private readonly struct GridElement
        {
            #region Fields

            /// <summary>
            /// Data contained within the grid element
            /// </summary>
            public TData Data { get; }

            /// <summary>
            /// Dimensions of the grid element
            /// </summary>
            public GridRect Dimensions { get; }

            /// <summary>
            /// Converts GridElement into a retrievable tuple.
            /// </summary>
            public (TData, GridRect) ElementData => (Data, Dimensions);

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a grid element with the given dimensions
            /// </summary>
            /// <param name="data">data for element to contain</param>
            /// <param name="dimensions">dimensions of the element</param>
            public GridElement(TData data, GridRect dimensions)
            {
                Data = data;
                Dimensions = dimensions;
            }

            /// <summary>
            /// Creates a grid element with the given position and size
            /// </summary>
            /// <param name="data">data for element to contain</param>
            /// <param name="position">minimum position of the element</param>
            /// <param name="size">size of the element</param>
            public GridElement(TData data, Vector2Int position, Vector2Int size)
            {
                Data = data;
                Dimensions = new GridRect(position, size);
            }

            #endregion
        }
    }

}