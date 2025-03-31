using System;
using System.Collections;
using System.Collections.Generic;
using GridObjects;
using Grids;
using InputControll;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystems
{
    public class TurnManager : GameSystem {
        public const int Order_HeatManager = 0;
        
        public class HandField
        {
            public GridObject GridObject { get; private set; }
            public bool IsSelected { get; set; }

            public HandField(GridObject gridObject)
            {
                this.GridObject = gridObject;
                this.IsSelected = false;
            }
            public HandField(GridObject gridObject, bool isSelected)
            {
                this.GridObject = gridObject;
                this.IsSelected = isSelected;
            }
        }


        public event Action OnTurnStart;
        public event Action OnReachTurnSkippPoint;
        public event Action OnBuildingTurnEnd;

        public event Action<GridObject, int> OnIncome;
        
        // public event Action OnTurnPasses;
        private SortedDictionary<int, Func<IEnumerable>> _onTurnPasses = new SortedDictionary<int, Func<IEnumerable>>();
        public event Action OnHeatSmimulationEnd;
    

        [SerializeField] private TurnCostManager _turnCost;
        [SerializeField] private AudioSource _placeSound;

        [SerializeField] private BucketRandom<GridObject> _objectsRandomiser;


        private ConstructionController _constructionController;
        private InputManager _inputManager;
        private WorldGrid _worldGrid;
        
        // private const float HEAT_PATIENT_POINTS_MULTIPLIER = 1.5f;

        private const int CARD_IN_TOUR = 5;


        private int _points = 5;
        private int _pointsAtRoundStart = 0;
        public int DisplayedPoints => _points;
        public IncomeData Income { get; private set; }


        protected override void InitSystem()
        {
            _worldGrid = _systems.Get<WorldGrid>();
            _turnCost.Init(this, _worldGrid);

            _constructionController = _systems.Get<ConstructionController>();
            _constructionController.OnBuildingBuild += OnBuildingBuild;

            _inputManager = _systems.Get<InputManager>();
            _inputManager.GameResetAction.Ended += ResetGame;

            StartCoroutine(StartFirstTour());
        }
        
        protected override void DeinitSystem()
        {
            _constructionController.OnBuildingBuild -= OnBuildingBuild;

            _inputManager.GameResetAction.Ended -= ResetGame;
        }


        private readonly List<HandField> _handCards = new();
        public IReadOnlyList<HandField> HandCards => _handCards;
        public event Action OnHandChanged;

        public void SelectCard(int index, bool isSelected)
        {
            Debug.Log($"Selected card on {index} index, is selected {isSelected}");

            foreach (var card in _handCards)
                card.IsSelected = false;

            _constructionController.SetObject(null);


            if (isSelected)
            {
                var card = GetCard(index);
                if (card == null)
                    return;


                _handCards[index] = new(card.GridObject, isSelected);
                _constructionController.SetObject(card.GridObject);
            }
            
            OnHandChanged?.Invoke();
        }
        public HandField GetCard(int index)
        {
            if (index < 0 || index >= _handCards.Count)
            {
                Debug.LogWarning($"Missing card on index {index}");
                return null;
            }

            return _handCards[index];
        }


        private void OnBuildingBuild((GridObject placed, GridObject pattern) value)
        {
            bool wasBuildingInHand = false;
            foreach (var item in _handCards)
            {
                if (item.IsSelected && item.GridObject == value.pattern)
                {
                    _handCards.Remove(item);
                    OnHandChanged?.Invoke();
                    wasBuildingInHand = true;
                    break;
                }
            }

            if (!wasBuildingInHand)
                return;

            _placeSound.Play();

            if (_handCards.Count <= 3)
                OnReachTurnSkippPoint?.Invoke();

            AddPointForBuilding(value.pattern);

            if (_handCards.Count == 0)
                EndTour();
        }
        private void AddPointForBuilding(GridObject building)
        {
            int pointsIncome = building.PointsForPlaced;
            _points += pointsIncome;
        }


        public void EndTour() => StartCoroutine(EndTurnSequence());
        private IEnumerator EndTurnSequence()
        {
            Debug.Log("End tour");

            _constructionController.SetObject(null);
            OnBuildingTurnEnd?.Invoke();

            _handCards.Clear();
            OnHandChanged?.Invoke();

            yield return new WaitForSeconds(1);

            
            foreach (var func in _onTurnPasses.Values) {
                foreach (var ret in func()) {
                    yield return ret;
                }
            }
            Income = _turnCost.NextTurnIncome(_worldGrid);

            foreach (var incomeInstance in Income.perObjectIncome) {
                _points += incomeInstance.income;
                OnIncome?.Invoke(incomeInstance.Item1, incomeInstance.income);
                yield return new WaitForSeconds(0.25f);
            }
            
            // yield return new WaitForSeconds(3.5f);     // wait for heat simulation

            
            // _heatPenalty = _turnCost.NextTurnIncome(_worldGrid);
            
            // _points -= (int)(_heatPenalty * HEAT_PATIENT_POINTS_MULTIPLIER);
            
            OnHeatSmimulationEnd?.Invoke();
        }

        public void NextTurn() => StartCoroutine(NextTurnSequence());

        public void RegisterTurnPassingCallback(Func<IEnumerable> callback, int order) {
            _onTurnPasses.Add(order, callback);
        }

        public void UnregisterTurnPassingCallback(int order) {
            _onTurnPasses.Remove(order);
        }
        
        private IEnumerator NextTurnSequence()
        {
            Debug.Log("Start tour");

            _pointsAtRoundStart = _points;

            OnTurnStart?.Invoke();

            _handCards.Clear();

            for (int i = 0; i < CARD_IN_TOUR; i++)
            {
                yield return new WaitForSeconds(0.2f);
                _handCards.Add(new(_objectsRandomiser.GetRandom()));
                OnHandChanged?.Invoke();
            }
        }


        private IEnumerator StartFirstTour()
        {
            yield return new WaitForSeconds(1);
            NextTurn();
        }


        private void ResetGame()
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        // public static int PointsToDisplayedPoints(float points) => Mathf.RoundToInt(points * 50);
    }
}