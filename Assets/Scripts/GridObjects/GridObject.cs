using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grids;
using UnityEngine.Assertions;
using System;
using GameSystems;

namespace GridObjects
{
    public class GridObject : MonoBehaviour
    {
        public event Action OnPlaced;

        public IEnumerable<Vector2Int> OccupiedTiles 
        {
            get
            {
                if (!IsPlaced)
                {
                    return Enumerable.Empty<Vector2Int>();
                }
                var gridPos = GridPos;
                return _fields.Select((cell) => gridPos + cell);
            } 
        }

        public IEnumerable<GCell> OccupiedCells
        {
            get
            {
                var worldGrid = SystemsManager.Instance.Get<WorldGrid>();
                foreach (var tile in OccupiedTiles)
                {
                    var cell = worldGrid.GetCell(tile);

                    if(cell.GridObject != this)
                    {
                        throw new System.Exception($"Overlapping objects at {tile}: ({this}) and ({cell.GridObject})");
                    }

                    yield return cell;
                }
            }
        }


        [SerializeField] private string _name = "default";
        public string DisplayedName => _name;


        [SerializeField] private Sprite _icon;
        public Sprite Icon => _icon;


        [SerializeField] private GridObjectTypeSO _type;
        public GridObjectTypeSO Type => _type;


        [SerializeField] private Vector2Int[] _fields = new Vector2Int[] { Vector2Int.zero };
        public IReadOnlyList<Vector2Int> Fields => _fields;

        public Vector2Int GridPos => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        [SerializeField] private GridObjectTypeSO[] _reqiredObjects = new GridObjectTypeSO[0];
        public IReadOnlyList<GridObjectTypeSO> ReqiredObjects => _reqiredObjects;


        [SerializeField]
        private int _deltaHot = 1;
        public int DeltaHot => _deltaHot;

        [field: SerializeField]
        public bool IsPlaced { get; private set; }


        [SerializeField] private int _pointsForPlaced = 50;
        public int PointsForPlaced => _pointsForPlaced;


        public void Place()
        {
            Assert.IsFalse(IsPlaced, $"{nameof(GridObject)}was already places");

            IsPlaced = true;

            foreach (var module in GetComponents<GridObjectModule>())
                module.OnBuildingConstructed();

            OnPlaced?.Invoke();
        }
        private void OnDestroy()
        {
            if (IsPlaced)
            {
                foreach (var module in GetComponents<GridObjectModule>())
                    module.OnBuildingDestroyed();
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var field in _fields)
            {
                Vector3 pos = transform.position + new Vector3(field.x, 0, field.y) * WorldGrid.CELL_SIZE;
                Gizmos.DrawLine(pos, pos + new Vector3(WorldGrid.CELL_SIZE, 0, WorldGrid.CELL_SIZE));
                Gizmos.DrawLine(pos + new Vector3(WorldGrid.CELL_SIZE, 0, 0), pos + new Vector3(0, 0, WorldGrid.CELL_SIZE));
            }

            UnityEditor.Handles.Label(transform.position + new Vector3(0, 0, WorldGrid.CELL_SIZE / 2), DisplayedName);
        }
#endif
    }
}
