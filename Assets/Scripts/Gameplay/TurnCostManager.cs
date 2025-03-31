using Grids;
using System;
using System.Collections;
using System.Collections.Generic;
using GridObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace GameSystems
{
    [Serializable]
    public class TurnCostManager
    {
        public float CurrentPoints => (float)points.Time;

        [FormerlySerializedAs("DefaultTurnCost")] [SerializeField]
        public int DefaultTurnIncome = 0;

        [SerializeField]
        private TimeMachine points;

        public void DepositPoints(float amount)
        {
            points.Accumulate(amount);
        }

        public IncomeData NextTurnIncome(WorldGrid grid)
        {
            var totalIncome = DefaultTurnIncome;
            var incomeList = new List<(GridObject, int income)>();
            incomeList.Add((null, DefaultTurnIncome));
            
            HashSet<GridObject> calculated = new HashSet<GridObjects.GridObject>();

            foreach (var cellPos in grid.GridSize.allPositionsWithin)
            {
                var cell = grid.GetCell(cellPos);

                if (cell.GridObject)
                {
                    if (calculated.Contains(cell.GridObject))
                    {
                        continue;
                    }
                    calculated.Add(cell.GridObject);
                    var objectIncome = 0;
                    foreach (var incomeProvider in cell.GridObject.GetComponents<IIncomeProvider>())
                    {
                        var c = incomeProvider.CurrentIncome;
                        Debug.Log(c);
                        objectIncome += c;
                    }
                    totalIncome += objectIncome;
                    incomeList.Add((cell.GridObject, objectIncome));
                }
            }
            return new IncomeData(incomeList, totalIncome);
        }

        // public void HandlePointConsumption(WorldGrid grid)
        // {
        //     var cost = NextTurnIncome(grid);
        //     Debug.Log(cost);
        //     if (!points.TryRetrieve(cost))
        //     {
        //         //TODO Handle game end
        //     }
        // }

        public void Init(TurnManager manager, WorldGrid grid)
        {
            //manager.TurnPasses += () => HandlePointConsumption(grid);
        }
    }

    public struct IncomeData {
        public readonly int totalIncome;
        public readonly List<(GridObject, int income)> perObjectIncome;

        public IncomeData(List<(GridObject, int income)> perObjectIncome, int totalIncome) {
            this.perObjectIncome = perObjectIncome;
            this.totalIncome = totalIncome;
        }
    }
}