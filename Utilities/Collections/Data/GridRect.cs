using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hitbox.Collections.Data
{
    public struct GridRect
    {
        #region Fields

        private int _xMin;
        private int _yMin;

        private int _width;
        private int _height;

        /// <summary>
        /// Position and minimum corner of the rectangle.
        /// </summary>
        public Vector2Int position => new(_xMin, _yMin);

        /// <summary>
        /// Size of the rectangle.
        /// </summary>
        public Vector2Int size => new(_width, _height);

        /// <summary>
        /// Centre of the rectangle.
        /// </summary>
        public Vector2 centre => min + new Vector2((float)_width / 2, (float)_height / 2);

        /// <summary>
        /// The position of the minimum corner of the rectangle.
        /// </summary>
        public Vector2Int min
        {
            get => new(xMin, yMin);
            set
            {
                xMin = value.x;
                yMin = value.y;
            }
        }

        /// <summary>
        /// The position of the maximum corner of the rectangle.
        /// </summary>
        public Vector2Int max
        {
            get => new(xMax, yMax);
            set
            {
                xMax = value.x;
                yMax = value.y;
            }
        }

        /// <summary>
        /// The minimum X coordinate of the rectangle.
        /// </summary>
        public int xMin
        {
            get => _xMin;
            set
            {
                int xMax = this.xMax;
                _xMin = value;
                _width = xMax - _xMin;
            }
        }

        /// <summary>
        /// The minimum Y coordinate of the rectangle.
        /// </summary>
        public int yMin
        {
            get => _yMin;
            set
            {
                int yMax = this.yMax;
                _yMin = value;
                _height = yMax - _yMin;
            }
        }


        /// <summary>
        /// The maximum X coordinate of the rectangle.
        /// </summary>
        public int xMax
        {
            get => _width + _xMin;
            set => _width = value - _xMin;
        }

        /// <summary>
        /// The maximum Y coordinate of the rectangle.
        /// </summary>
        public int yMax
        {
            get => _height + _yMin;
            set => _height = value - _yMin;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Checks whether the given point is within the rectangle.
        /// </summary>
        /// <param name="point">point to check</param>
        /// <returns>true if point is contained</returns>
        public bool Contains(Vector2 point)
        {
            return point.x > xMin &&
                   point.x < xMax &&
                   point.y > yMin &&
                   point.y < yMax;
        }

        /// <summary>
        /// Checks whether the given rectangle is contained within this rectangle.
        /// </summary>
        /// <param name="rect">rectangle to check</param>
        /// <returns>true if rectangle is contained</returns>
        public bool Contains(GridRect rect)
        {
            return Contains(rect.min) && Contains(rect.max);
        }

        /// <summary>
        /// Checks whether the given rectangle is contained within this rectangle.
        /// </summary>
        /// <param name="rect">rectangle to check</param>
        /// <returns>true if rectangle is contained</returns>
        public bool Contains(Rect rect)
        {
            return Contains(rect.min) && Contains(rect.max);
        }

        /// <summary>
        /// Checks whether the given rectangle overlaps with this rectangle.
        /// </summary>
        /// <param name="gridRect">rectangle to check</param>
        /// <returns>true if rectangles overlap</returns>
        public bool Overlaps(GridRect gridRect)
        {
            return xMin < gridRect.xMax && xMax > gridRect.xMin &&
                   yMin < gridRect.yMax && yMax > gridRect.yMin;
        }

        /// <summary>
        /// Checks whether the given rectangle overlaps with this rectangle.
        /// </summary>
        /// <param name="rect">rectangle to check</param>
        /// <returns>true if rectangles overlap</returns>
        public bool Overlaps(Rect rect)
        {
            return xMin < rect.xMax && xMax > rect.xMin &&
                   yMin < rect.yMax && yMax > rect.yMin;
        }

        #endregion

        #region --- CONSTRUCTORS ---

        public GridRect(Vector2Int position, Vector2Int size)
        {
            _xMin = position.x;
            _yMin = position.y;
            _width = size.x;
            _height = size.y;
        }

        public GridRect(int x, int y, int width, int height)
        {
            _xMin = x;
            _yMin = y;
            _width = width;
            _height = height;
        }

        public GridRect(Vector2Int pos, int width, int height)
        {
            _xMin = pos.x;
            _yMin = pos.y;

            _width = width;
            _height = height;
        }

        public GridRect(int x, int y, Vector2Int size)
        {
            _xMin = x;
            _yMin = y;

            _width = size.x;
            _height = size.y;
        }

        /// <summary>
        /// Creates a Grid Rectangle, with position and size rounded to nearest grid coordinate.
        /// </summary>
        /// <param name="pos">minimum corner of rectangle</param>
        /// <param name="size">size of rectangle</param>
        public GridRect(Vector2 pos, Vector2 size)
        {
            _xMin = Mathf.RoundToInt(pos.x);
            _yMin = Mathf.RoundToInt(pos.y);

            _width = Mathf.RoundToInt(size.x);
            _height = Mathf.RoundToInt(size.y);
        }

        #endregion
    }
}
