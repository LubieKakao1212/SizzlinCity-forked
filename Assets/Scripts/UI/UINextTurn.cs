using GameSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UINextTurn : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        [SerializeField] private Button _nextButton;

        [Header("Colors")]
        [SerializeField] private Color _addMoneyColor;
        [SerializeField] private Color _remMoneyColor;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _lastPointsLabel;
        [SerializeField] private TextMeshProUGUI _pointsIncomeLabel;
        [SerializeField] private TextMeshProUGUI _heatPenaltyLabel;
        [SerializeField] private TextMeshProUGUI _finallPointsLabel;

        [Space(20)]
        [SerializeField] private AudioSource _pointsSound;

        private TurnManager _turnManager;

        private void OnEnable()
        {
            _turnManager = SystemsManager.Instance.Get<TurnManager>();
            _turnManager.OnHeatSmimulationEnd += Open;
            _turnManager.OnTurnStart += Close;

            _nextButton.onClick.AddListener(PlayNextRound);
        }
        private void OnDisable()
        {
            if (_turnManager != null)
            {
                _turnManager.OnHeatSmimulationEnd -= Open; 
                _turnManager.OnTurnStart -= Close;
            }

            _nextButton.onClick.RemoveListener(PlayNextRound);
        }

        private void Open()
        {
            _canvas.enabled = true;

            string moneyS = "$";

            // int lastValue = _turnManager.DisplayedPoints + _turnManager.HeatPenalty - _turnManager.PointsIncom;
            // _lastPointsLabel.text = lastValue.ToString() + moneyS;

            _pointsIncomeLabel.text = _turnManager.Income.totalIncome.ToString("+#;-#;0") + moneyS;
            _pointsIncomeLabel.color = _turnManager.Income.totalIncome >= 0 ? _addMoneyColor : _remMoneyColor;

            // _heatPenaltyLabel.text = _turnManager.HeatPenalty.ToString("-#;-#;0") + moneyS;

            _finallPointsLabel.text = _turnManager.DisplayedPoints.ToString() + moneyS;

            StartCoroutine(AnimText());
        }
        private void Close()
        {
            _canvas.enabled = false;
        }

        private IEnumerator AnimText()
        {
            var labels = new List<TextMeshProUGUI> { _lastPointsLabel, _pointsIncomeLabel, _heatPenaltyLabel, _finallPointsLabel };
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
    }
}