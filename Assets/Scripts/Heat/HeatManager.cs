using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Grids;
using GameSystems;

namespace HeatSimulation
{
    public class HeatManager : GameSystem
    {
        private HashSet<HeatGenerator> heatGenerators;

        [SerializeField] private float[] kernel;
        [SerializeField] private int _heatUpdateIterations;
        [SerializeField] private float _heatIterationInterval;

        [SerializeField] private MeshFilter overlay;
        [SerializeField] private MeshRenderer overlayRenderer;


        public Action<bool> OnOverlaySwitch;
        public bool IsOverlayActive { get; private set; } = false;


        private TurnManager _turnManager;
        private WorldGrid _worldGrid;

        private Coroutine _overlayAcriveAnimation;

        protected override void InitSystem()
        {
            heatGenerators = new HashSet<HeatGenerator>();

            _worldGrid = _systems.Get<WorldGrid>();

            SetupKernel();

            GenerateOverlay();

            _turnManager = _systems.Get<TurnManager>();
            _turnManager.RegisterTurnPassingCallback(UpdateHeat, TurnManager.Order_HeatManager);
            _turnManager.OnTurnStart += DisenableOverlay;
        }
        protected override void DeinitSystem() 
        {
            _turnManager.UnregisterTurnPassingCallback(TurnManager.Order_HeatManager);
            _turnManager.OnTurnStart -= DisenableOverlay;
        }


        public void RegisterGenerator(HeatGenerator generator)
        {
            heatGenerators.Add(generator);
        }
        public void RemoveGenerator(HeatGenerator generator)
        {
            heatGenerators.Remove(generator);
        }


        // private void StartHeatUpdate() => StartCoroutine(UpdateHeat());
        private IEnumerable UpdateHeat()
        {
            EnableOverlay(true);
            yield return new WaitForSeconds(0.8f);

            for (int i = 0; i < _heatUpdateIterations; i++) 
            {
                GenerateHeat();
                PropagateHeat();
                GenerateOverlay();
                yield return new WaitForSeconds(_heatIterationInterval);
            }
        }

        private void GenerateHeat()
        {
            foreach (var generator in heatGenerators)
            {
                foreach (var tile in generator.HeatedCells)
                {
                    tile.Heat = Mathf.Max(tile.Heat + (generator.HeatGeneration / generator.ZoneSize), 0);
                }
            }
        }
        private void PropagateHeat()
        {
            ApplyKernel((x, y, i) => GetHeatAt(x + i, y));
            ApplyKernel((x, y, i) => GetHeatAt(x, y + i));
        }

        private void GenerateOverlay()
        {
            var sizeX = _worldGrid.GridX + 1;
            var sizeY = _worldGrid.GridX + 1;

            Mesh mesh = new Mesh();

            var verts = new Vector3[sizeX * sizeY * 4];
            var tris = new int[sizeX * sizeY * 6];
            var uv0 = new Vector4[sizeX * sizeY * 4];
            var uv1 = new Vector4[sizeX * sizeY * 4];

            for (int x = 0; x < sizeX; x++)
                for (int y = 0; y < sizeX; y++)
                {
                    int i = sizeX * y + x;
                    int v = i * 4;
                    int t = i * 6;
                    verts[v + 0] = new Vector3(x - 0.5f, 0f, y - 0.5f);
                    verts[v + 1] = new Vector3(x + 0.5f, 0f, y - 0.5f);
                    verts[v + 2] = new Vector3(x + 0.5f, 0f, y + 0.5f);
                    verts[v + 3] = new Vector3(x - 0.5f, 0f, y + 0.5f);

                    uv0[v + 0] = new Vector2(0, 0);
                    uv0[v + 1] = new Vector2(1, 0);
                    uv0[v + 2] = new Vector2(1, 1);
                    uv0[v + 3] = new Vector2(0, 1);

                    var weights = new Vector4(
                        GetHeatAt(x, y),
                        GetHeatAt(x, y - 1),
                        GetHeatAt(x - 1, y - 1),
                        GetHeatAt(x - 1, y)
                        );

                    uv1[v + 0] = weights;
                    uv1[v + 1] = weights;
                    uv1[v + 2] = weights;
                    uv1[v + 3] = weights;

                    tris[t + 0] = v + 0;
                    tris[t + 1] = v + 2;
                    tris[t + 2] = v + 1;
                    tris[t + 3] = v + 0;
                    tris[t + 4] = v + 3;
                    tris[t + 5] = v + 2;
                }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv1);

            mesh.SetTriangles(tris, 0);

            overlay.mesh = mesh;
        }

        private void SetupKernel()
        {
            var k = kernel;
            int k2 = k.Length - 1;
            kernel = new float[2 * k2 + 1];
            float sum = 0;
            for (int i = 0; i < k2 + 1; i++)
            {
                sum += kernel[k2 + i] = k[i];
                sum += kernel[k2 - i] = k[i];
            }

            sum -= k[0];

            Debug.Log(sum);

            float sum2 = 0;

            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= sum;
                sum2 += kernel[i];
            }

            Debug.Log(sum2);
        }

        private float GetHeatAt(int x, int y)
        {
            if (x < 0 || x >= _worldGrid.GridX || y < 0 || y >= _worldGrid.GridY)
            {
                return 0f;
            }
            return _worldGrid.GetCell(x, y).Heat;
        }

        private void ApplyKernel(Func<int, int, int, float> heatGetter)
        {
            float[,] newHeat = new float[_worldGrid.GridX, _worldGrid.GridY];

            int k2 = kernel.Length / 2;

            //Suboptimal code below
            for (int x = 0; x < _worldGrid.GridX; x++)
                for (int y = 0; y < _worldGrid.GridY; y++)
                {
                    for (int i = 0; i < kernel.Length; i++)
                    {
                        var v = heatGetter(x, y, i - k2);
                        newHeat[x, y] += kernel[i] * v;
                    }
                }

            for (int x = 0; x < _worldGrid.GridX; x++)
                for (int y = 0; y < _worldGrid.GridY; y++)
                {
                    _worldGrid.GetCell(x, y).Heat = newHeat[x, y];
                }
        }

        private void DisenableOverlay() => EnableOverlay(false);
        public void EnableOverlay(bool active)
        {
            if (_overlayAcriveAnimation != null)
                StopCoroutine(_overlayAcriveAnimation);
            _overlayAcriveAnimation = StartCoroutine(SetOverlayActive(active));
        }
        private IEnumerator SetOverlayActive(bool active)
        {
            IsOverlayActive = active;
            OnOverlaySwitch?.Invoke(active);

            if (active)
                yield return new WaitForSeconds(0.5f); // wait for camera movement

            overlayRenderer.enabled = active;

            _overlayAcriveAnimation = null;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_worldGrid == null)
            {
                return;
            }
            var c = Gizmos.color;
            foreach (var pos in _worldGrid.GridSize.allPositionsWithin)
            {
                var h = _worldGrid.GetCell(pos).Heat;
                //h *= h;
                var color = new Color(
                    -Mathf.Cos(h) / 2f + 0.5f,
                    0f,
                    Mathf.Sin(h) / 2f + 0.5f
                    );
                Gizmos.color = color;
                Gizmos.DrawCube(new Vector3(pos.x + 0.5f, 0, pos.y + 0.5f), Vector3.one * 0.9f);
            }
            Gizmos.color = c;
        }
#endif
    }
}