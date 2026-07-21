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
        [SerializeField, Range(0.5f, 1f)] private float requiredPlacementOverlap = 0.9f;
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

        internal bool TryPlace(C02DraggableBox box, Camera eventCamera)
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

            Rect boxRect = box.GetVisibleScreenRect(eventCamera);
            Rect slotRect = GetScreenRect(targetSlot, eventCamera);

            if (CalculateOverlap(boxRect, slotRect) < requiredPlacementOverlap)
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

        private static float CalculateOverlap(Rect boxRect, Rect slotRect)
        {
            float width = Mathf.Max(0f, Mathf.Min(boxRect.xMax, slotRect.xMax) - Mathf.Max(boxRect.xMin, slotRect.xMin));
            float height = Mathf.Max(0f, Mathf.Min(boxRect.yMax, slotRect.yMax) - Mathf.Max(boxRect.yMin, slotRect.yMin));
            float boxArea = boxRect.width * boxRect.height;
            return boxArea > 0f ? width * height / boxArea : 0f;
        }

        private static Rect GetScreenRect(RectTransform rectTransform, Camera eventCamera)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
            Vector2 max = min;

            for (int index = 1; index < corners.Length; index++)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[index]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
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
