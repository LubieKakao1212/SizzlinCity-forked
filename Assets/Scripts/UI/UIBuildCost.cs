using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputControll;
using GameSystems;
using GridObjects;
using TMPro;

namespace UI
{
    public class UIBuildCost : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        [Space(10)]
        [SerializeField] private RectTransform _mainPanel;

        [Space(10)]
        [SerializeField] private TextMeshProUGUI _moneyLabel;
        [SerializeField] private Color _addMoneyColor;
        [SerializeField] private Color _remMoneyColor;


        private ConstructionController _constructionController;
        private InputManager _inputManager;

        private Coroutine _positionUpdater;

        
        private void OnEnable()
        {
            _constructionController = SystemsManager.Instance.Get<ConstructionController>();
            OnBuildingSelected(_constructionController.SelectedBuilding);
            _constructionController.OnBuildingSelected += OnBuildingSelected;

            _inputManager = SystemsManager.Instance.Get<InputManager>();
        }
        private void OnDisable()
        {
            if (_constructionController != null)
                _constructionController.OnBuildingSelected -= OnBuildingSelected;
        }


        private void OnBuildingSelected(GridObject obj)
        {
            if (obj == null)
            {
                if (_positionUpdater != null)
                {
                    StopCoroutine(_positionUpdater);
                    _positionUpdater = null;
                }

                _canvas.enabled = false;
                return;
            }


            int points = obj.PointsForPlaced;

            _moneyLabel.text = points.ToString("+#;-#;0");
            _moneyLabel.color = points >= 0 ? _addMoneyColor : _remMoneyColor;

            _canvas.enabled = true;
            _positionUpdater = StartCoroutine(PositionUpdate());
        }

        private IEnumerator PositionUpdate()
        {
            while (true)
            {
                Vector2 mousePos = _inputManager.MousePosition;
                Vector2 canvasPos = mousePos / _canvas.scaleFactor;
                _mainPanel.anchoredPosition = canvasPos;
                yield return null;
            }
        }
    }
}