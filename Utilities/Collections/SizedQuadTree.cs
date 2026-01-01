using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Hitbox.Collections
{
    public class SizedQuadTree<TData>
    {
        #region Fields

        // Subdivisions, can be null
        [CanBeNull] private SizedQuadTree<TData> _topLeft;
        [CanBeNull] private SizedQuadTree<TData> _topRight;
        [CanBeNull] private SizedQuadTree<TData> _bottomLeft;
        [CanBeNull] private SizedQuadTree<TData> _bottomRight;

        /// <summary>
        /// Bounds of the QuadTree, elements can't inserted if they aren't encapsulated by this!
        /// </summary>
        public Rect Bounds { get; protected set; }

        /// <summary>
        /// The amount of elements that this quad can hold before subdividing, elements can still be added if
        /// this is the lowest level quad that encapsulates the rectangle.
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// Has the QuadTree been divided, used to avoid null references before subdivision.
        /// </summary>
        private bool _isDivided;

        /// <summary>
        /// Depth limit of the entire QuadTree, used to limit subdivisions
        /// </summary>
        private readonly int _depthLimit = 4;

        /// <summary>
        /// All of the contained elements for this quad.
        /// </summary>
        private List<ContainedElement> _containedElements = new List<ContainedElement>();

        #endregion

        #region Methods

        #region Retrieval

        public bool TryGetDataAtPosition(Vector2 point, out TData data)
        {
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Rectangle.Contains(point))
                {
                    data = element.Data;
                    return true;
                }
            }

            if (!_isDivided) // Stop if not divided
            {
                data = default;
                return false;
            }

            try
            {
                // Check subdivisions for data
                if (_topLeft!.Bounds.Contains(point)) return _topLeft.TryGetDataAtPosition(point, out data);
                if (_topRight!.Bounds.Contains(point)) return _topRight.TryGetDataAtPosition(point, out data);
                if (_bottomLeft!.Bounds.Contains(point)) return _bottomLeft.TryGetDataAtPosition(point, out data);
                if (_bottomRight!.Bounds.Contains(point)) return _bottomRight.TryGetDataAtPosition(point, out data);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }

            data = default;
            return false;
        }
        
        public bool TryGetElementAtPosition(Vector2 point, out (TData data, Rect rectangle) elementData)
        {
            List<ContainedElement> elements = new List<ContainedElement>();
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Rectangle.Contains(point))
                {
                    elements.Add(element);
                }
            }


            float currentDist = float.PositiveInfinity;
            ContainedElement closestElement = new ContainedElement();
            foreach (ContainedElement element in elements)
            {
                float newDist = Vector2.Distance(element.Rectangle.center, point);
                if (newDist > currentDist) continue;

                closestElement = element;
                currentDist = newDist;
            }

            if (!float.IsInfinity(currentDist))
            {
                elementData = (closestElement.Data, closestElement.Rectangle);
                return true;
            }
            
            if (!_isDivided) // Stop if not divided
            {
                elementData = default;
                return false;
            }

            try
            {
                // Check subdivisions for data
                if (_topLeft!.Bounds.Contains(point)) return _topLeft.TryGetElementAtPosition(point, out elementData);
                if (_topRight!.Bounds.Contains(point)) return _topRight.TryGetElementAtPosition(point, out elementData);
                if (_bottomLeft!.Bounds.Contains(point)) return _bottomLeft.TryGetElementAtPosition(point, out elementData);
                if (_bottomRight!.Bounds.Contains(point)) return _bottomRight.TryGetElementAtPosition(point, out elementData);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }

            elementData = default;
            return false;
        }
        
        public void GetElementsAtPosition(Vector2 point, ref List<(TData data, Rect rectangle)> elementData)
        {
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Rectangle.Contains(point))
                {
                    elementData.Add((element.Data, element.Rectangle));
                }
            }
            
            if (!_isDivided) // Stop if not divided
            {
                return;
            }

            try
            {
                // Check subdivisions for data
                if (_topLeft!.Bounds.Contains(point)) _topLeft.GetElementsAtPosition(point, ref elementData);
                if (_topRight!.Bounds.Contains(point)) _topRight.GetElementsAtPosition(point, ref elementData);
                if (_bottomLeft!.Bounds.Contains(point)) _bottomLeft.GetElementsAtPosition(point, ref elementData);
                if (_bottomRight!.Bounds.Contains(point)) _bottomRight.GetElementsAtPosition(point, ref elementData);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }

        }
        
        public bool TryGetFirstElementAtPosition(Vector2 point, out (TData data, Rect rectangle) elementData)
        {
            foreach (ContainedElement element in _containedElements.Where(element => element.Rectangle.Contains(point)))
            {
                elementData = (element.Data, element.Rectangle);
                return true;
            }
            
            if (!_isDivided) // Stop if not divided
            {
                elementData = default;
                return false;
            }

            try
            {
                // Check subdivisions for data
                if (_topLeft!.Bounds.Contains(point)) return _topLeft.TryGetElementAtPosition(point, out elementData);
                if (_topRight!.Bounds.Contains(point)) return _topRight.TryGetElementAtPosition(point, out elementData);
                if (_bottomLeft!.Bounds.Contains(point)) return _bottomLeft.TryGetElementAtPosition(point, out elementData);
                if (_bottomRight!.Bounds.Contains(point)) return _bottomRight.TryGetElementAtPosition(point, out elementData);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }

            elementData = default;
            return false;
        }

        public bool TryGetRectFromData(TData data, out Rect rect)
        {
            rect = default;
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Data.Equals(data))
                {
                    Debug.Log("!!!");
                    rect = element.Rectangle;
                    return true;
                }
            }
            
            // Stop if not divided.
            if (!_isDivided) return false;

            try
            {
                // Checking subdivisions for data
                if (_topLeft!.TryGetRectFromData(data, out rect)) return true;
                if (_topRight!.TryGetRectFromData(data, out rect)) return true;
                if (_bottomLeft!.TryGetRectFromData(data, out rect)) return true;
                if (_bottomRight!.TryGetRectFromData(data, out rect)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }

            return false;
        }

        public Rect[] GetAllElementRectangles()
        {
            List<Rect> rectangles = new List<Rect>();

            foreach (ContainedElement element in _containedElements)
            {
                rectangles.Add(element.Rectangle);
            }

            return rectangles.ToArray();
        }

        public bool IsElementAtPosition(Vector2 point)
        {
            // Checking every element in this quad.
            if (_containedElements.Any(element => element.Rectangle.Contains(point))) return true;

            // Stop if not divided.
            if (!_isDivided) return false;

            try
            {
                // Checking subdivisions for data
                if (_topLeft!.Bounds.Contains(point)) return true;
                if (_topRight!.Bounds.Contains(point)) return true;
                if (_bottomLeft!.Bounds.Contains(point)) return true;
                if (_bottomRight!.Bounds.Contains(point)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }


            // No element found containing given point.
            return false;
        }

        public void RetrieveAllElements(ref List<(TData data, Rect rect)> elements)
        {
            // Adding all elements from this quad.
            elements.AddRange(_containedElements.Select(element => (element.Data, element.Rectangle)));

            // Stop if not divided.
            if (!_isDivided) return;

            try // Getting all elements from subdivisions of quad.
            {
                _topLeft!.RetrieveAllElements(ref elements);
                _topRight!.RetrieveAllElements(ref elements);
                _bottomLeft!.RetrieveAllElements(ref elements);
                _bottomRight!.RetrieveAllElements(ref elements);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogWarning($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                _isDivided = false;
            }
        }

        public void RetrieveAllBounds(ref List<Rect> bounds)
        {
            bounds.Add(Bounds);

            // Stop if not divided.
            if (!_isDivided) return;

            try // Getting all elements from subdivisions of quad.
            {
                _topLeft!.RetrieveAllBounds(ref bounds);
                _topRight!.RetrieveAllBounds(ref bounds);
                _bottomLeft!.RetrieveAllBounds(ref bounds);
                _bottomRight!.RetrieveAllBounds(ref bounds);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogWarning($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }
        }

        public void RetrieveAllQuads(ref List<SizedQuadTree<TData>> quads)
        {
            quads.Add(this);

            // Stop if not divided.
            if (!_isDivided) return;

            try // Getting all elements from subdivisions of quad.
            {
                _topLeft!.RetrieveAllQuads(ref quads);
                _topRight!.RetrieveAllQuads(ref quads);
                _bottomLeft!.RetrieveAllQuads(ref quads);
                _bottomRight!.RetrieveAllQuads(ref quads);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogWarning($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }
        }

        public bool Contains(TData data)
        {
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Data.Equals(data)) return true;
            }
            
            // Stop if not divided.
            if (!_isDivided) return false;

            try // Getting all elements from subdivisions of quad.
            {
                if (_topLeft!.Contains(data)) return true;
                if (_topRight!.Contains(data)) return true;
                if (_bottomLeft!.Contains(data)) return true;
                if (_bottomRight!.Contains(data)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogWarning($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }

            return false;
        }

        #endregion

        #region Insertion

        public bool Insert(Vector2 point, TData data)
        {
            if (!Bounds.Contains(point)) return false; // Element doesn't fit in quad.

            // Subdivide if capacity has been reached (and not already divided)
            if (!_isDivided && _containedElements.Count + 1 > _capacity)
            {
                Subdivide();
            }

            if (_isDivided)
            {
                try
                {
                    if (_topLeft!.Insert(point, data)) return true;
                    if (_topRight!.Insert(point, data)) return true;
                    if (_bottomLeft!.Insert(point, data)) return true;
                    if (_bottomRight!.Insert(point, data)) return true;
                }
                catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
                {
                    Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                }
            }

            // Creating a new, small, rectangle to represent the point in the tree.
            Rect pointRect = Rect.MinMaxRect(point.x - 0.05f, point.y - 0.05f, point.x + 0.05f, point.y + 0.05f);

            if (!Bounds.ContainsRect(pointRect)) return false;

            _containedElements.Add(new ContainedElement(data, pointRect));
            return true;
        }

        /// <summary>
        /// Insert a rectangle into QuadTree containing the given data.
        /// </summary>
        /// <param name="rect">size of the data on the tree</param>
        /// <param name="data">data contained within the rect</param>
        /// <param name="checkCollision">should check for collisions</param>
        /// <returns>true if insertion was successful</returns>
        public bool Insert(Rect rect, TData data, bool checkCollision = false)
        {
            if (!Bounds.ContainsRect(rect)) return false; // Element doesn't fit in quad.
            
            if (checkCollision)
            {
                if (CollisionCheck(rect)) return false;
            }

            // Subdivide if capacity has been reached (and not already divided)
            if (!_isDivided && _depthLimit > 0 && _containedElements.Count + 1 > _capacity)
            {
                Subdivide();
            }

            if (_isDivided)
            {
                try
                {
                    if (_topLeft!.Insert(rect, data)) return true;
                    if (_topRight!.Insert(rect, data)) return true;
                    if (_bottomLeft!.Insert(rect, data)) return true;
                    if (_bottomRight!.Insert(rect, data)) return true;
                }
                catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
                {
                    Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                }
            }


            _containedElements.Add(new ContainedElement(data, rect));
            return true;
        }

        private bool Insert(ContainedElement element)
        {
            return Insert(element.Rectangle, element.Data);
        }

        #endregion

        #region Removal

        public bool TryRemoveElementAtPoint(Vector2 point, out TData data)
        {
            // Checking every element in this quad.
            foreach (ContainedElement element in _containedElements)
            {
                if (!element.Rectangle.Contains(point)) continue; // Continue if element doesn't contain point
                _containedElements.Remove(element);
                data = element.Data;
                return true;
            }

            if (!_isDivided)
            {
                data = default;
                return false;
            }

            try
            {
                // Attempting removal in sub divisions
                if (_topLeft!.TryRemoveElementAtPoint(point, out data)) return true;
                if (_topRight!.TryRemoveElementAtPoint(point, out data)) return true;
                if (_bottomLeft!.TryRemoveElementAtPoint(point, out data)) return true;
                if (_bottomRight!.TryRemoveElementAtPoint(point, out data)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }

            data = default;
            return false;
        }

        public bool RemoveElement(Vector2 point)
        {
            // Checking every element in this quad.
            foreach (ContainedElement element in _containedElements)
            {
                if (!element.Rectangle.Contains(point)) continue; // Continue if element doesn't contain point
                _containedElements.Remove(element);
                return true;
            }

            if (!_isDivided) return false;

            try
            {
                // Attempting removal in sub divisions
                if (_topLeft!.RemoveElement(point)) return true;
                if (_topRight!.RemoveElement(point)) return true;
                if (_bottomLeft!.RemoveElement(point)) return true;
                if (_bottomRight!.RemoveElement(point)) return true;
            }
            catch (NullReferenceException err) // This should never happem!
            {
                Debug.LogWarning($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                return false;
            }

            return false;
        }
        
        public void Clear()
        {
            _containedElements.Clear();
            
            _bottomRight?.Clear();
            _bottomLeft?.Clear();
            _topLeft?.Clear();
            _topRight?.Clear();
        }

        #endregion

        /// <summary>
        /// Subdivide the quad into four corners.
        /// </summary>
        private void Subdivide()
        {
            Vector2 halfSize = Bounds.size / 2;
            Rect bottomLeftRect = new Rect(Bounds.center - halfSize, halfSize);
            Rect topLeftRect = new Rect(Bounds.center + new Vector2(-halfSize.x, 0), halfSize);
            Rect topRightRect = new Rect(Bounds.center, halfSize);
            Rect bottomRightRect = new Rect(Bounds.center + new Vector2(0, -halfSize.y), halfSize);

            _topLeft = new SizedQuadTree<TData>(topLeftRect, _capacity, _depthLimit - 1);
            _topRight = new SizedQuadTree<TData>(topRightRect, _capacity, _depthLimit - 1);
            _bottomLeft = new SizedQuadTree<TData>(bottomLeftRect, _capacity, _depthLimit - 1);
            _bottomRight = new SizedQuadTree<TData>(bottomRightRect, _capacity, _depthLimit - 1);

            for (int i = _containedElements.Count - 1; i >= 0; i--)
            {
                ContainedElement element = _containedElements[i];
                if (_topLeft.Insert(element) || _topRight.Insert(element) ||
                    _bottomLeft.Insert(element) || _bottomRight.Insert(element))
                {
                    _containedElements.RemoveAt(i);
                }
            }

            _isDivided = true;
        }

        /// <summary>
        /// Check if the given rectangle overlaps with any objects within the grid
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool CollisionCheck(Rect rect)
        {
            if (!Bounds.Overlaps(rect)) return false; // Rect doesn't overlap with this quad.

            foreach (ContainedElement element in _containedElements)
            {
                if (element.Rectangle.Overlaps(rect)) return true;
            }

            if (_isDivided)
            {
                try
                {
                    if (_topLeft!.CollisionCheck(rect)) return true;
                    if (_topRight!.CollisionCheck(rect)) return true;
                    if (_bottomLeft!.CollisionCheck(rect)) return true;
                    if (_bottomRight!.CollisionCheck(rect)) return true;
                }
                catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
                {
                    Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
                }
            }

            return false;
        }
        
        public TData this[Vector2 point] => TryGetDataAtPosition(point, out TData data) ? data : default;

        #endregion

        #region --- CONSTRUCTORS ---

        public SizedQuadTree(Rect bounds, int capacity)
        {
            // Rectangle takes half width.
            Bounds = bounds;
            _capacity = capacity;
        }

        public SizedQuadTree(Rect bounds, int capacity, int maxDepth = 1)
        {
            Bounds = bounds;
            _capacity = capacity;
            _depthLimit = maxDepth;
        }

        #endregion

        private readonly struct ContainedElement
        {
            public TData Data { get; }
            public Rect Rectangle { get; }

            public ContainedElement(TData data, Rect rectangle)
            {
                Data = data;
                Rectangle = rectangle;
            }
        }
    }

}