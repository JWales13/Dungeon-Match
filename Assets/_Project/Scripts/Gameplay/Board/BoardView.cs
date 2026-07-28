using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Presentation;

namespace Game.Gameplay
{
    /// <summary>
    /// Renders a Core.Board and translates screen taps into grid cells.
    /// Layout values (cell size, tile scale, camera background) come from the
    /// active Theme, so board spacing is styled in code, not the Inspector.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private TileView _tilePrefab;
        [SerializeField] private Camera _camera;

        private Board _board;
        private TileView[,] _tileViews;
        private float _cellSize;

        public void Initialize(Board board)
        {
            Teardown();

            _board = board;
            _cellSize = Theme.Current.CellSize;
            ApplyCameraBackground();
            BuildTileViews();

            _board.TilesMatched += HandleTilesMatched;
            _board.BoardSettled += HandleBoardSettled;
        }

        /// <summary>
        /// Detaches from the previous board and destroys its tile objects so a
        /// new room can be rendered on the same BoardView. Safe to call when
        /// nothing has been initialized yet.
        /// </summary>
        private void Teardown()
        {
            if (_board != null)
            {
                _board.TilesMatched -= HandleTilesMatched;
                _board.BoardSettled -= HandleBoardSettled;
            }

            if (_tileViews == null)
            {
                return;
            }

            foreach (TileView view in _tileViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }

            _tileViews = null;
        }

        public bool TryGetCellAtScreenPosition(Vector2 screenPosition, out Vector2Int cell)
        {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(screenPosition);
            int x = Mathf.RoundToInt(worldPosition.x / _cellSize);
            int y = Mathf.RoundToInt(worldPosition.y / _cellSize);
            cell = new Vector2Int(x, y);

            return IsWithinBoard(cell);
        }

        private void ApplyCameraBackground()
        {
            if (_camera != null)
            {
                _camera.backgroundColor = Theme.Current.BackgroundColor;
            }
        }

        private bool IsWithinBoard(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < _board.Width && cell.y >= 0 && cell.y < _board.Height;
        }

        private void BuildTileViews()
        {
            _tileViews = new TileView[_board.Width, _board.Height];

            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    CreateTileView(x, y);
                }
            }
        }

        private void CreateTileView(int x, int y)
        {
            Vector3 position = new Vector3(x * _cellSize, y * _cellSize, 0f);
            TileView view = Instantiate(_tilePrefab, position, Quaternion.identity, transform);
            view.Display(_board.GetTile(new Vector2Int(x, y)));
            _tileViews[x, y] = view;
        }

        private void HandleTilesMatched(IReadOnlyList<Vector2Int> matchedCells)
        {
            foreach (var cell in matchedCells)
            {
                _tileViews[cell.x, cell.y].PlayMatchedEffect();
            }
        }

        private void HandleBoardSettled()
        {
            RefreshAllTileVisuals();
        }

        private void RefreshAllTileVisuals()
        {
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    _tileViews[x, y].Display(_board.GetTile(new Vector2Int(x, y)));
                }
            }
        }
    }
}