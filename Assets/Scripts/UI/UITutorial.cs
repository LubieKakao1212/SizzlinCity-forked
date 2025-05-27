using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameSystems;
using GridObjects;
using HeatSimulation;

namespace UI.Tutorial
{
    public class UITutorial : MonoBehaviour
    {
        [SerializeField] private UITutorialMess _templateMess;
        [SerializeField] private float _unactivityTime = 6f;

        [Header("Texts")]
        [SerializeField] private TextAsset _resetTipText;
        [SerializeField] private TextAsset _selectBuildingTipText;
        [SerializeField] private TextAsset _buildTipText;
        [SerializeField] private TextAsset _endTurnTipText;
        [SerializeField] private TextAsset _heatOverlayTipText;

        [SerializeField] private TextAsset[] _buildingTips;
        [SerializeField] private GridObject[] _buildingsForTips;
        
        private TurnManager _turnManager;
        private HeatManager _heatManager;


        private Coroutine _showResetTipC;
        private Coroutine _showSelectCardTipC;
        private Coroutine _showBuildTipC;
        private Coroutine _showEndTurnTipC;


        private readonly HashSet<TextAsset> _usedMessanges = new();

        private readonly List<UITutorialMess> _activeMessanges = new();
        private readonly List<UITutorialMess> _unactiveMessanges = new();


        private void OnEnable()
        {
            _turnManager = SystemsManager.Instance.Get<TurnManager>();
            _turnManager.OnTurnStart += ShowResetTip;
            _turnManager.OnHandChanged += ShowSelectCardTip;
            _turnManager.OnHandChanged += ShowBuildTip;
            _turnManager.OnHandChanged += ShowSelectedBuildingTip;
            _turnManager.OnHeatSmimulationEnd += ShowEndTurnTip;

            _heatManager = SystemsManager.Instance.Get<HeatManager>();
            _heatManager.OnOverlaySwitch += ShowHeatOverlayTip;

            _templateMess.SetActive(false);
        }
        private void OnDisable()
        {
            _turnManager.OnTurnStart -= ShowResetTip;
            _turnManager.OnHandChanged -= ShowSelectCardTip;
            _turnManager.OnHandChanged -= ShowBuildTip;
            _turnManager.OnHandChanged -= ShowSelectedBuildingTip;
            _turnManager.OnHeatSmimulationEnd -= ShowEndTurnTip;

            _heatManager.OnOverlaySwitch -= ShowHeatOverlayTip;
        }


        private void ShowResetTip()
        {
            if (_showResetTipC == null)
                _showResetTipC = StartCoroutine(ShowResetTipC());
        }
        private IEnumerator ShowResetTipC()
        {
            if (!CanShowMess(_resetTipText))
                yield break;

            yield return new WaitForSeconds(4);
            TryShowMess(_resetTipText);
        }


        private void ShowSelectCardTip()
        {
            if (IsCardSelected())
                return;

            if (_showSelectCardTipC == null)
                _showSelectCardTipC = StartCoroutine(ShowSelectCardTipC());
        }
        private IEnumerator ShowSelectCardTipC()
        {
            float time = _unactivityTime;
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;

                if (!ShouldShow())
                {
                    _showSelectCardTipC = null;
                    yield break;
                }
            }
            
            var messObj = TryShowMess(_selectBuildingTipText, false);
            while (ShouldShow())
                yield return null;

            HideMess(messObj);
            _showSelectCardTipC = null;

            bool ShouldShow() => _turnManager.HandCards.Count > 0 && !IsCardSelected();
        }

        private void ShowSelectedBuildingTip() {
            var building = _turnManager.SelectedCard;
            if (building != null) {
                //Cursed
                TryShowMess(_buildingsForTips.Zip(_buildingTips, (o, asset) => o == building ? asset : null).First(asset => asset != null));
            }
        }
        

        private void ShowBuildTip()
        {
            if (!IsCardSelected())
                return;
            
            if (_showBuildTipC == null)
                _showBuildTipC = StartCoroutine(ShowBuildTipC());
        }
        private IEnumerator ShowBuildTipC()
        {
            float time = _unactivityTime;
            while (time > 0)
            {
                time -= Time.deltaTime;
                yield return null;

                if (!IsCardSelected())
                {
                    _showBuildTipC = null;
                    yield break;
                }
            }

            var messObj = TryShowMess(_buildTipText, false);
            while (IsCardSelected())
                yield return null;

            HideMess(messObj);
            _showBuildTipC = null;
        }


        private void ShowEndTurnTip(bool gameHasEnded)
        {
            if(gameHasEnded) {
                return;
            }
            
            if (_showEndTurnTipC == null)
                _showEndTurnTipC = StartCoroutine(ShowEndTurnTipC());
        }
        private IEnumerator ShowEndTurnTipC()
        {
            if (!CanShowMess(_endTurnTipText))
                yield break;

            yield return new WaitForSeconds(2);
            TryShowMess(_endTurnTipText);
        }


        private void ShowHeatOverlayTip(bool active)
        {
            if (active)
                TryShowMess(_heatOverlayTipText);
        }


        private bool IsCardSelected()
        {
            foreach (var item in _turnManager.HandCards)
                if (item.IsSelected)
                    return true;
            return false;
        }

        private bool CanShowMess(TextAsset messText) => !_usedMessanges.Contains(messText);
        private UITutorialMess TryShowMess(TextAsset messText, bool showOnlyOnce = true)
        {
            if (showOnlyOnce && !CanShowMess(messText))
                return null;

            _usedMessanges.Add(messText);

            UITutorialMess messObj;
            if (_unactiveMessanges.Count == 0)
            {
                messObj = Instantiate(_templateMess, _templateMess.transform.parent);
                messObj.name = $"Mess{_unactiveMessanges.Count + _activeMessanges.Count}";
            }
            else
            {
                messObj = _unactiveMessanges[0];
                _unactiveMessanges.RemoveAt(0);
            }

            _activeMessanges.Add(messObj);
            messObj.transform.SetAsFirstSibling();
            messObj.Set(messText.text, () => HideMess(messObj));

            return messObj;
        }
        private void HideMess(UITutorialMess messObj)
        {
            if (!_activeMessanges.Contains(messObj))
            {
                Debug.LogWarning($"Mess {messObj.name} in not active!");
                return;
            }

            messObj.SetActive(false);
            _activeMessanges.Remove(messObj);
            _unactiveMessanges.Add(messObj);
        }
    }
}