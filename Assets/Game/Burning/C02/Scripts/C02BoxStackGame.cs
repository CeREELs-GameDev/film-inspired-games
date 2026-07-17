using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FilmInspiredGames.Burning.C02
{
    public sealed class C02BoxStackGame : MonoBehaviour
    {
        public enum PlacementGuideMode
        {
            None,
            GhostBoxes
        }

        [Header("드래그")]
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private List<C02DraggableBox> boxes = new();

        [Header("놓을 자리")]
        [SerializeField] private List<RectTransform> targetSlots = new();
        [SerializeField, Min(1f)] private float snapDistance = 110f;
        [SerializeField] private bool fitBoxToSlot;
        [SerializeField] private bool preserveLayerAlignment;
        [SerializeField] private Vector2 alignedTargetPosition;

        [Header("자리 안내")]
        [SerializeField] private PlacementGuideMode placementGuideMode = PlacementGuideMode.GhostBoxes;
        [SerializeField, Range(0f, 1f)] private float placementGuideOpacity = 0.1f;
        [SerializeField] private List<Image> placementGuides = new();

        [Header("완료")]
        [SerializeField] private UnityEvent onCompleted;

        private readonly HashSet<RectTransform> occupiedSlots = new();
        private int placedCount;
        private int validBoxCount;
        private bool interactionEnabled;
        private bool completed;

        public event Action Completed;
        public Canvas Canvas => rootCanvas;

        private void Awake()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }

            if (dragLayer == null)
            {
                dragLayer = (RectTransform)transform;
            }

            foreach (C02DraggableBox box in boxes)
            {
                if (box != null)
                {
                    validBoxCount++;
                    box.Initialize(this, dragLayer);
                }
            }

            EnsurePlacementGuides();

            if (rootCanvas == null)
            {
                Debug.LogError("C02 상자 게임에 Canvas 연결 필요", this);
            }

            SetInteraction(false);
        }

        public void Begin()
        {
            occupiedSlots.Clear();
            placedCount = 0;
            validBoxCount = 0;
            completed = false;

            foreach (C02DraggableBox box in boxes)
            {
                if (box == null)
                {
                    continue;
                }

                validBoxCount++;
                box.Initialize(this, dragLayer);
                box.SetInteractable(true);
                box.ResetHome();
            }

            EnsurePlacementGuides();
            ResetPlacementGuides();

            interactionEnabled = true;
        }

        private void EnsurePlacementGuides()
        {
            if (placementGuideMode != PlacementGuideMode.GhostBoxes || placementGuides.Count > 0)
            {
                return;
            }

            GameObject rootObject = new("PlacementGuides", typeof(RectTransform));
            RectTransform root = (RectTransform)rootObject.transform;
            root.SetParent(transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.SetSiblingIndex(Mathf.Min(1, transform.childCount - 1));

            for (int index = 0; index < boxes.Count; index++)
            {
                C02DraggableBox box = boxes[index];

                if (box == null || !box.TryGetComponent(out Image sourceImage))
                {
                    placementGuides.Add(null);
                    continue;
                }

                GameObject guideObject = new(
                    $"BoxGuide{index + 1:00}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                RectTransform guideRect = (RectTransform)guideObject.transform;
                guideRect.SetParent(root, false);
                guideRect.anchorMin = new Vector2(0.5f, 0.5f);
                guideRect.anchorMax = new Vector2(0.5f, 0.5f);
                guideRect.pivot = new Vector2(0.5f, 0.5f);
                guideRect.anchoredPosition = alignedTargetPosition;
                guideRect.sizeDelta = box.RectTransform.sizeDelta;

                Image guide = guideObject.GetComponent<Image>();
                guide.sprite = sourceImage.sprite;
                guide.preserveAspect = false;
                guide.raycastTarget = false;
                guide.color = new Color(1f, 1f, 1f, placementGuideOpacity);
                placementGuides.Add(guide);
            }
        }

        public void SetInteraction(bool value)
        {
            interactionEnabled = value;

            foreach (C02DraggableBox box in boxes)
            {
                if (box != null)
                {
                    box.SetInteractable(value && !completed);
                }
            }
        }

        internal bool TryPlace(C02DraggableBox box, Vector2 pointerPosition, Camera eventCamera)
        {
            if (!interactionEnabled || completed)
            {
                return false;
            }

            int boxIndex = boxes.IndexOf(box);

            if (boxIndex < 0 || boxIndex >= targetSlots.Count)
            {
                return false;
            }

            RectTransform targetSlot = targetSlots[boxIndex];

            if (targetSlot == null || occupiedSlots.Contains(targetSlot))
            {
                return false;
            }

            Vector2 slotPosition = RectTransformUtility.WorldToScreenPoint(eventCamera, targetSlot.position);

            if (Vector2.Distance(pointerPosition, slotPosition) > snapDistance)
            {
                return false;
            }

            occupiedSlots.Add(targetSlot);
            placedCount++;
            box.SnapTo(targetSlot, fitBoxToSlot, preserveLayerAlignment, alignedTargetPosition);
            HidePlacementGuide(box);

            if (validBoxCount > 0 && placedCount >= validBoxCount)
            {
                Complete();
            }

            return true;
        }

        private void ResetPlacementGuides()
        {
            bool showGuides = placementGuideMode == PlacementGuideMode.GhostBoxes;

            foreach (Image guide in placementGuides)
            {
                if (guide == null)
                {
                    continue;
                }

                Color color = guide.color;
                color.a = placementGuideOpacity;
                guide.color = color;
                guide.raycastTarget = false;
                guide.gameObject.SetActive(showGuides);
            }
        }

        private void HidePlacementGuide(C02DraggableBox box)
        {
            int index = boxes.IndexOf(box);

            if (index < 0 || index >= placementGuides.Count || placementGuides[index] == null)
            {
                return;
            }

            placementGuides[index].gameObject.SetActive(false);
        }

        private void Complete()
        {
            completed = true;
            SetInteraction(false);
            onCompleted?.Invoke();
            Completed?.Invoke();
        }
    }
}
