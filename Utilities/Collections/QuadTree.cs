using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Hitbox.Collections
{
    public class QuadTree<TData>
    {
        #region Fields

        // Subdivisions, can be null
        [CanBeNull] private QuadTree<TData> _topLeft;
        [CanBeNull] private QuadTree<TData> _topRight;
        [CanBeNull] private QuadTree<TData> _bottomLeft;
        [CanBeNull] private QuadTree<TData> _bottomRight;

        /// <summary>
        /// Bounds of the QuadTree, elements can't inserted if they aren't encapsulated by this!
        /// </summary>
        private readonly Rect _bounds;

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
                if (element.Point == point)
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
                if (_topLeft!._bounds.Contains(point)) return _topLeft.TryGetDataAtPosition(point, out data);
                if (_topRight!._bounds.Contains(point)) return _topRight.TryGetDataAtPosition(point, out data);
                if (_bottomLeft!._bounds.Contains(point)) return _bottomLeft.TryGetDataAtPosition(point, out data);
                if (_bottomRight!._bounds.Contains(point)) return _bottomRight.TryGetDataAtPosition(point, out data);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }

            data = default;
            return false;
        }
        
        public bool TryGetElementAtPosition(Vector2 point, out (TData data, Vector2 point) elementData)
        {
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Point == point)
                {
                    elementData = (element.Data, element.Point);
                    return true;
                }
            }

            if (!_isDivided) // Stop if not divided
            {
                elementData = default;
                return false;
            }

            try
            {
                // Check subdivisions for data
                if (_topLeft!._bounds.Contains(point)) return _topLeft.TryGetElementAtPosition(point, out elementData);
                if (_topRight!._bounds.Contains(point)) return _topRight.TryGetElementAtPosition(point, out elementData);
                if (_bottomLeft!._bounds.Contains(point)) return _bottomLeft.TryGetElementAtPosition(point, out elementData);
                if (_bottomRight!._bounds.Contains(point)) return _bottomRight.TryGetElementAtPosition(point, out elementData);
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }

            elementData = default;
            return false;
        }

        public bool TryGetPointFromData(TData data, out Vector2 point)
        {
            point = default;
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Data.Equals(data))
                {
                    point = element.Point;
                    return true;
                }
            }
            
            // Stop if not divided.
            if (!_isDivided) return false;

            try
            {
                // Checking subdivisions for data
                if (_topLeft!.TryGetPointFromData(data, out point)) return true;
                if (_topRight!.TryGetPointFromData(data, out point)) return true;
                if (_bottomLeft!.TryGetPointFromData(data, out point)) return true;
                if (_bottomRight!.TryGetPointFromData(data, out point)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }

            return false;
        }

        public Vector2[] GetAllElementPoints()
        {
            List<Vector2> points = new List<Vector2>();

            foreach (ContainedElement element in _containedElements)
            {
                points.Add(element.Point);
            }

            return points.ToArray();
        }

        public bool IsElementAtPosition(Vector2 point)
        {
            // Checking every element in this quad.
            if (_containedElements.Any(element => element.Point == point)) return true;

            // Stop if not divided.
            if (!_isDivided) return false;

            try
            {
                // Checking subdivisions for data
                if (_topLeft!._bounds.Contains(point)) return true;
                if (_topRight!._bounds.Contains(point)) return true;
                if (_bottomLeft!._bounds.Contains(point)) return true;
                if (_bottomRight!._bounds.Contains(point)) return true;
            }
            catch (NullReferenceException err) // This will only happen if _isDivided has been tampered with.
            {
                Debug.LogError($"isDivided is true but QuadTree hasn't been divided yet! Error: {err.Message}");
            }


            // No element found containing given point.
            return false;
        }

        public void RetrieveAllElements(ref List<(TData data, Vector2 point)> elements)
        {
            // Adding all elements from this quad.
            elements.AddRange(_containedElements.Select(element => (element.Data, element.Point)));

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
            }
        }

        public void RetrieveAllBounds(ref List<Rect> bounds)
        {
            bounds.Add(_bounds);

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

        public void RetrieveAllQuads(ref List<QuadTree<TData>> quads)
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
            if (!_bounds.Contains(point)) return false; // Element doesn't fit in quad.

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
            

            if (!IsElementAtPosition(point)) return false;

            _containedElements.Add(new ContainedElement(data, point));
            return true;
        }

        private bool Insert(ContainedElement element)
        {
            return Insert(element.Point, element.Data);
        }

        #endregion

        #region Removal

        public bool TryRemoveElementAtPoint(Vector2 point, out TData data)
        {
            // Checking every element in this quad.
            foreach (ContainedElement element in _containedElements)
            {
                if (element.Point != point) continue; // Continue if element doesn't contain point
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
                if (element.Point != point) continue; // Continue if element doesn't contain point
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

        #endregion

        /// <summary>
        /// Subdivide the quad into four corners.
        /// </summary>
        private void Subdivide()
        {
            Vector2 halfSize = _bounds.size / 2;
            Rect bottomLeftRect = new Rect(_bounds.center - halfSize, halfSize);
            Rect topLeftRect = new Rect(_bounds.center + new Vector2(-halfSize.x, 0), halfSize);
            Rect topRightRect = new Rect(_bounds.center, halfSize);
            Rect bottomRightRect = new Rect(_bounds.center + new Vector2(0, -halfSize.y), halfSize);

            _topLeft = new QuadTree<TData>(topLeftRect, _capacity, _depthLimit - 1);
            _topRight = new QuadTree<TData>(topRightRect, _capacity, _depthLimit - 1);
            _bottomLeft = new QuadTree<TData>(bottomLeftRect, _capacity, _depthLimit - 1);
            _bottomRight = new QuadTree<TData>(bottomRightRect, _capacity, _depthLimit - 1);

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

        public Rect GetBounds() => _bounds;

        public TData this[Vector2 point] => TryGetDataAtPosition(point, out TData data) ? data : default;

        #endregion

        #region --- CONSTRUCTORS ---

        public QuadTree(Rect bounds, int capacity)
        {
            // Rectangle takes half width.
            _bounds = bounds;
            _capacity = capacity;
        }

        public QuadTree(Rect bounds, int capacity, int maxDepth = 1)
        {
            _bounds = bounds;
            _capacity = capacity;
            _depthLimit = maxDepth;
        }

        #endregion

        private readonly struct ContainedElement
        {
            public TData Data { get; }
            public Vector2 Point { get; }

            public ContainedElement(TData data, Vector2 point)
            {
                Data = data;
                Point = point;
            }
        }
    }

}