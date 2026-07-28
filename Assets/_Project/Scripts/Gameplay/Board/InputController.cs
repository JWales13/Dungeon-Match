using System;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>
    /// Turns raw pointer down/up events into intents: a drag becomes a
    /// SwapRequested; a tap (press and release on the same cell) becomes a
    /// CellTapped (used to target a booster). Knows nothing about Board or
    /// match rules - only screen-to-cell conversion and drag direction.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        public event Action<Vector2Int, Vector2Int> SwapRequested;
        public event Action<Vector2Int> CellTapped;

        [SerializeField] private BoardView _boardView;

        [Tooltip("Must be a component that implements IPointerInputSource, e.g. UnityPointerInputSource.")]
        [SerializeField] private MonoBehaviour _pointerInputSourceBehaviour;

        private IPointerInputSource _pointerInputSource;
        private bool _isDragging;
        private Vector2Int _dragStartCell;

        private void Awake()
        {
            _pointerInputSource = _pointerInputSourceBehaviour as IPointerInputSource;
            if (_pointerInputSource == null)
            {
                Debug.LogError($"{nameof(_pointerInputSourceBehaviour)} on {name} must implement {nameof(IPointerInputSource)}.");
                enabled = false;
                return;
            }

            _pointerInputSource.PointerDown += HandlePointerDown;
            _pointerInputSource.PointerUp += HandlePointerUp;
        }

        private void OnDestroy()
        {
            if (_pointerInputSource == null)
            {
                return;
            }

            _pointerInputSource.PointerDown -= HandlePointerDown;
            _pointerInputSource.PointerUp -= HandlePointerUp;
        }

        private void HandlePointerDown(Vector2 screenPosition)
        {
            if (!_boardView.TryGetCellAtScreenPosition(screenPosition, out Vector2Int cell))
            {
                return;
            }

            _isDragging = true;
            _dragStartCell = cell;
        }

        private void HandlePointerUp(Vector2 screenPosition)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;

            if (!_boardView.TryGetCellAtScreenPosition(screenPosition, out Vector2Int endCell))
            {
                return;
            }

            if (endCell == _dragStartCell)
            {
                CellTapped?.Invoke(_dragStartCell);
                return;
            }

            Vector2Int targetCell = ClampToSingleStep(_dragStartCell, endCell);
            SwapRequested?.Invoke(_dragStartCell, targetCell);
        }

        /// <summary>
        /// Reduces a drag of any length/direction to a single orthogonal step
        /// from the start cell, so a sloppy diagonal swipe still resolves to
        /// the dominant direction the player intended.
        /// </summary>
        private static Vector2Int ClampToSingleStep(Vector2Int start, Vector2Int end)
        {
            Vector2Int delta = end - start;
            bool horizontalDragIsDominant = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);

            Vector2Int step = horizontalDragIsDominant
                ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
                : new Vector2Int(0, (int)Mathf.Sign(delta.y));

            return start + step;
        }
    }
}