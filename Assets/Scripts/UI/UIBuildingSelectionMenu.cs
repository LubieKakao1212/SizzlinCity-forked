using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GridObjects;
using GameSystems;
using DG.Tweening;

namespace UI.HUD
{
    public class UIBuildingSelectionMenu : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _mainPanel;

        [Space(10)]
        [SerializeField] private UIHUDCard _cardPrefab;
        [SerializeField] private Transform _cardsParent;

        [SerializeField] private GridObject _temptBuild;

        [Header("open values")]
        [SerializeField] private float _openYPos = 10;
        [SerializeField] private float _closeYPos = -160;
        [SerializeField] private float _openTime = 0.3f;
        [SerializeField] private Button _switchButton;
        [SerializeField] private TextMeshProUGUI _switchButtonLabel;


        private readonly List<UIHUDCard> _displayedCards = new();
        private readonly List<UIHUDCard> _unusedCards = new();

        private bool _isOpen = true;

        private TurnManager _turnManager;

        private void OnEnable()
        {
            _turnManager = FindAnyObjectByType<TurnManager>();
            if (_turnManager == null)
            {
                Debug.LogWarning($"{typeof(TurnManager)} not found");
                enabled = false;
                return;
            }

            UpdateCards();
            _turnManager.OnHandChanged += UpdateCards;

            _switchButton.onClick.AddListener(SwitchOpen);

            SetOpen(_isOpen);
        }
        private void OnDisable()
        {
            if (_turnManager != null)
                _turnManager.OnHandChanged -= UpdateCards;

            _switchButton.onClick.RemoveListener(SwitchOpen);
        }

        private void UpdateCards()
        {
            for (int i = _displayedCards.Count - 1; i >= 0; i--)
                HideCard(_displayedCards[i]);

            for (int i = 0; i < _turnManager.HandCards.Count; i++)
            {
                var card = _turnManager.HandCards[i];
                AddCard(card.GridObject, i, card.IsSelected);
            }

            _canvas.enabled = _displayedCards.Count > 0;
        }

        public void AddCard(GridObject obj, int index, bool isSelected)
        {
            if (_unusedCards.Count == 0)
                _unusedCards.Add(Instantiate(_cardPrefab, _cardsParent));

            var card = _unusedCards[0];
            _unusedCards.RemoveAt(0);
            card.transform.SetSiblingIndex(_displayedCards.Count);

            card.Show(obj.DisplayedName, obj.Icon, obj.DeltaHot, isSelected, () => CardClick(index));

            _displayedCards.Add(card);
        }
        public void HideCard(UIHUDCard card)
        {
            if (_displayedCards.Remove(card))
            {
                _unusedCards.Add(card);
                card.Hide();
            }
        }

        private void CardClick(int index)
        {
            var card = _turnManager.GetCard(index);
            if (card == null)
                return;

            _turnManager.SelectCard(index, !card.IsSelected);
        }


        private void SwitchOpen() => SetOpen(!_isOpen);
        private void SetOpen(bool open)
        {
            _isOpen = open;
            _switchButtonLabel.text = open ? "hide" : "show";
            _mainPanel.DOKill();
            _mainPanel.DOAnchorPosY(open ? _openYPos : _closeYPos, _openTime).SetEase(Ease.Linear);
        }
    }
}