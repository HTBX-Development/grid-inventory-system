using System;
using System.Collections.Generic;
using System.Linq;
using Hitbox.Collections.Data;
using UnityEngine;

namespace Hitbox.Collections
{
    public class SizedSparseGrid<TData>
    {
        #region Fields

        private readonly Dictionary<int, GridElement> _elements = new();

        private int _width;
        private int _height;

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

        public TData GetDataAtIndex(int index)
        {
            return _elements.TryGetValue(index, out GridElement element) ? element.Data : default;
        }
        
        public TData GetDataAtPosition(Vector2Int pos)
        {
            return _elements.TryGetValue(PositionToIndex(pos), out GridElement element) ? element.Data : default;
        }

        public TData GetDataAtPosition(int x, int y)
        {
            return GetDataAtPosition(new Vector2Int(x, y));
        }
        
        public bool TryGetDataAtPosition(Vector2Int pos, out TData data)
        {
            if (_elements.TryGetValue(PositionToIndex(pos), out GridElement element))
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
        
        public (TData data, GridRect rect) GetElementAtIndex(int index)
        {
            return _elements.TryGetValue(index, out GridElement element) ? element.ElementData : default;
        }

        public (TData data, GridRect rect) GetElementAtPosition(Vector2Int pos)
        {
            return _elements.TryGetValue(PositionToIndex(pos), out GridElement element) ? element.ElementData : default;
        }

        public (TData data, GridRect rect) GetElementAtPosition(int x, int y)
        {
            return GetElementAtPosition(new Vector2Int(x, y));
        }
        
        public (TData data, GridRect rect)[] GetAllElementData()
        {
            HashSet<(TData, GridRect)> data = new HashSet<(TData, GridRect)>();
            foreach (GridElement element in _elements.Values)
            {
                data.Add(element.ElementData);
            }

            return data.ToArray();
        }

        // TODO: optimize!
        public (TData data, GridRect rect) GetElementFromData(TData data)
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

        /// <summary>
        /// Check if an element is at the given position
        /// </summary>
        /// <param name="position">position to test</param>
        /// <returns>true if an element is at the given position.</returns>
        public bool ElementAt(Vector2Int position)
        {
            return _elements.ContainsKey(PositionToIndex(position));
        }
        
        /// <summary>
        /// Check if an element is at the given index
        /// </summary>
        /// <param name="index">index to test</param>
        /// <returns>true if an element is at the given index.</returns>
        public bool ElementAt(int index)
        {
            return _elements.ContainsKey(index);
        }

        /// <summary>
        /// Check if an element is within the given dimensions.
        /// </summary>
        /// <param name="rect">dimensions to test</param>
        /// <returns>true if an element is within the dimensions</returns>
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

        public bool ElementsIn(GridRect rect, ref List<TData> data)
        {
            bool elementFound = false;
            for (int y = rect.min.y; y < rect.max.y; y++)
            {
                for (int x = rect.min.x; x < rect.max.x; x++)
                {
                    if (!ElementAt(new Vector2Int(x, y))) continue;
                    data.Add(GetElementAtPosition(new Vector2Int(x, y)).data);
                    elementFound = true;
                }
            }

            return elementFound;
        }
        
        /// <summary>
        /// Check if an element is within the given dimensions, ignoring excludeData
        /// </summary>
        /// <param name="rect">dimensions to test</param>
        /// <param name="excludeData"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public bool ElementIn(GridRect rect, TData excludeData, IEqualityComparer<TData> comparer = null)
        {
            comparer ??= EqualityComparer<TData>.Default;
    
            for (int y = rect.min.y; y < rect.max.y; y++)
            {
                for (int x = rect.min.x; x < rect.max.x; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (TryGetDataAtPosition(pos, out TData data))
                    {
                        if (comparer.Equals(data, excludeData)) continue;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Insertion
        
        public bool InsertElement(TData data, int index, Vector2Int dimensions)
        {
            return InsertElement(data, IndexToPosition(index), dimensions);
        }

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
                    _elements.Add(PositionToIndex(new Vector2Int(x, y)), element);
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
            return Remove(PositionToIndex(pos));
        }
        
        public TData Remove(int index)
        {
            if (!ElementAt(index)) return default;

            GridElement element = _elements[index];

            // Clearing all taken slots
            for (int y = element.Dimensions.min.y; y < element.Dimensions.max.y; y++)
            {
                for (int x = element.Dimensions.min.x; x < element.Dimensions.max.x; x++)
                {
                    _elements.Remove(PositionToIndex(new Vector2Int(x, y)));
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
                            _elements.Remove(PositionToIndex(new Vector2Int(x, y)));
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

            GridElement element = _elements[PositionToIndex(pos)];

            // Updating all taken slots with new element.
            for (int y = element.Dimensions.min.y; y < element.Dimensions.max.y; y++)
            {
                for (int x = element.Dimensions.min.x; x < element.Dimensions.max.x; x++)
                {
                    _elements.Remove(PositionToIndex(new Vector2Int(x, y)));
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

        #region Utilities

        public int PositionToIndex(Vector2Int pos)
        {
            return pos.y * _width + pos.x;
        }
        
        public Vector2Int IndexToPosition(int index)
        {
            return new Vector2Int(index % _width, index / _width);
        }

        #endregion

        #endregion

        #region --- CONSTRUCTORS ---

        public SizedSparseGrid(int width, int height)
        {
            _width = width;
            _height = height;
        }
        
        public SizedSparseGrid(Vector2Int size)
        {
            _width = size.x;
            _height = size.y;
        }

        #endregion

        private readonly struct GridElement : IEquatable<GridElement>
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

            public bool Equals(GridElement other)
            {
                return EqualityComparer<TData>.Default.Equals(Data, other.Data) && Dimensions.Equals(other.Dimensions);
            }

            public override bool Equals(object obj)
            {
                return obj is GridElement other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Data, Dimensions);
            }
        }
    }

}