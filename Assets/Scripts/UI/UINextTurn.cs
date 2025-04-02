using GameSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace UI
{
    public class UINextTurn : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        [FormerlySerializedAs("_nextButton")] [SerializeField] private Button _nextTurnButton;
        [SerializeField] private Button _gameEndButton;

        [Header("Colors")]
        [SerializeField] private Color _addMoneyColor;
        [SerializeField] private Color _remMoneyColor;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _lastPointsLabel;
        [SerializeField] private TextMeshProUGUI _pointsIncomeLabel;
        [FormerlySerializedAs("_heatPenaltyLabel")] [SerializeField] private TextMeshProUGUI _investmentIncomeLabel;
        [SerializeField] private TextMeshProUGUI _finallPointsLabel;

        [Space(20)]
        [SerializeField] private AudioSource _pointsSound;

        private TurnManager _turnManager;

        private void OnEnable()
        {
            _turnManager = SystemsManager.Instance.Get<TurnManager>();
            _turnManager.OnHeatSmimulationEnd += Open;
            _turnManager.OnTurnStart += Close;

            _nextTurnButton.onClick.AddListener(PlayNextRound);
            _gameEndButton.onClick.AddListener(EndGame);
        }
        private void OnDisable()
        {
            if (_turnManager != null)
            {
                _turnManager.OnHeatSmimulationEnd -= Open; 
                _turnManager.OnTurnStart -= Close;
            }

            _nextTurnButton.onClick.RemoveListener(PlayNextRound);
        }

        private void Open(bool gameEnded)
        {
            _canvas.enabled = true;

            string moneyS = "$";

            var lastPoints = _turnManager.PointsAtRoundStart;
            _lastPointsLabel.text = lastPoints + moneyS;

            var maintenanceIncome = _turnManager.Income.totalIncome;
            _pointsIncomeLabel.text = maintenanceIncome.ToString("+#;-#;0") + moneyS;
            _pointsIncomeLabel.color = maintenanceIncome >= 0 ? _addMoneyColor : _remMoneyColor;

            int investValue = _turnManager.DisplayedPoints - lastPoints - maintenanceIncome;
            _investmentIncomeLabel.text = investValue.ToString("+#;-#;0") + moneyS;
            _investmentIncomeLabel.color = investValue >= 0 ? _addMoneyColor : _remMoneyColor;
            
            _finallPointsLabel.text = _turnManager.DisplayedPoints.ToString() + moneyS;
            
            _gameEndButton.gameObject.SetActive(gameEnded);
            _nextTurnButton.gameObject.SetActive(!gameEnded);
            
            
            StartCoroutine(AnimText());
        }
        private void Close()
        {
            _canvas.enabled = false;
        }

        private IEnumerator AnimText()
        {
            var labels = new List<TextMeshProUGUI> { _lastPointsLabel, _pointsIncomeLabel, _investmentIncomeLabel, _finallPointsLabel };
            foreach (var item in labels)
                item.alpha = 0;

            yield return new WaitForSeconds(.5f);

            foreach (var item in labels)
            {
                yield return new WaitForSeconds(.3f);
                _pointsSound.Play();
                item.alpha = 1;
            }
        }

        private void PlayNextRound() => _turnManager.NextTurn();

        private void EndGame() => _turnManager.ResetGame();
    }
}