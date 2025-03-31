using System.Linq;
using DG.Tweening;
using GameSystems;
using GridObjects;
using InputControll;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class UIMoneyPanel : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color _addMoneyColor;
        [SerializeField] private Color _remMoneyColor;

        [Header("Main panel")]
        [SerializeField] private TextMeshProUGUI _mainMoneyLabel;

        [Header("Animation panel")]
        // [SerializeField] private RectTransform _animationPanel;
        // [SerializeField] private TextMeshProUGUI _animationMoneyLabel;
        [SerializeField] private RectTransform _animationTarget;
        [SerializeField] private RectTransform _incomeLabelPrefab;
        
        // private Sequence _animationSeq;

        private TurnManager _turnManager;
        private ConstructionController _constructionController;
        private Tweener moneyShakeTweener = null;

        private void OnEnable() {
            _constructionController = SystemsManager.Instance.Get<ConstructionController>();
            _constructionController.OnBuildingBuild += AnimOnBuildingBuild;

            _turnManager = SystemsManager.Instance.Get<TurnManager>();

            _turnManager.OnIncome += AnimateIncome;

            _turnManager.OnTurnStart += UpdateMoneyLabel;
            UpdateMoneyLabel();
        }

        private void OnDisable() {
            if (_constructionController != null)
                _constructionController.OnBuildingBuild -= AnimOnBuildingBuild;
            
            if (_turnManager != null) {
                _turnManager.OnTurnStart -= UpdateMoneyLabel;
                _turnManager.OnIncome += AnimateIncome;
            }
        }

        private void AnimOnBuildingBuild((GridObject placedBuilding, GridObject objectPattern) placeEvt)
        {
            if (placeEvt.placedBuilding == null)
                return;

            int points = placeEvt.objectPattern.PointsForPlaced;//TurnManager.PointsToDisplayedPoints(placeEvt.objectPattern.PointsForPlaced);
            AnimateIncome(placeEvt.placedBuilding, points);
        }

        private void AnimateIncome([CanBeNull] GridObject source, int amount) {
            if (amount == 0) {
                return;
            }
            
            var labelTransform = Instantiate(_incomeLabelPrefab, transform);
            var labelText = labelTransform.GetComponentInChildren<TextMeshProUGUI>(); 
            
            labelText.text = amount.ToString("+#;-#;0");
            labelText.color = amount >= 0 ? _addMoneyColor : _remMoneyColor;

            var worldPos = Vector3.zero;
            if (source != null) {
                var gridPos = source.OccupiedTiles.Aggregate((i, vector2Int) => i + vector2Int);
                worldPos = new Vector3(gridPos.x + 0.5f, source.transform.position.y + 1f, gridPos.y + 0.5f);
            }
            Vector3 screenPos = Camera.main!.WorldToScreenPoint(worldPos);

            Hide();
            
            // _animationSeq.Kill();
            var sequence = DOTween.Sequence()
                    .AppendInterval(.4f)
                    .AppendCallback(Show)
                    .Append(labelTransform.DOScale(Vector3.one, .4f).SetEase(Ease.OutBack))
                    .AppendInterval(.1f)
                    .Append(labelTransform.DOMove(_animationTarget.position, .5f).SetEase(Ease.OutQuad))
                    .Append(labelTransform.DOScale(Vector3.zero, .2f).SetEase(Ease.OutQuad))
                    .AppendCallback(UpdateMoneyLabel)
                    .AppendCallback(() => Destroy(labelTransform.gameObject))
            ;
            
            void Show()
            {
                labelTransform.localScale = Vector3.zero;
                labelTransform.position = screenPos;
                labelTransform.gameObject.SetActive(true);
            }
            void Hide()
            {
                labelTransform.gameObject.SetActive(false);
            }
        }
        
        private void UpdateMoneyLabel()
        {
            int points = _turnManager.DisplayedPoints;
            _mainMoneyLabel.text = points.ToString();
            _mainMoneyLabel.color = points >= 0 ? _addMoneyColor : _remMoneyColor;
            moneyShakeTweener?.Complete();
            moneyShakeTweener = _mainMoneyLabel.rectTransform.DOShakeScale(.4f, .5f);
        }
    }
}