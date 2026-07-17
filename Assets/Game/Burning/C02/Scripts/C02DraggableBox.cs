using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FilmInspiredGames.Burning.C02
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class C02DraggableBox : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICanvasRaycastFilter
    {
        [SerializeField, Min(0.01f)] private float returnDuration = 0.16f;
        [SerializeField] private Vector2 normalizedRaycastMin;
        [SerializeField] private Vector2 normalizedRaycastMax = Vector2.one;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private C02BoxStackGame game;
        private Transform dragLayer;
        private Transform homeParent;
        private int homeSiblingIndex;
        private Vector2 homePosition;
        private Vector2 homeSize;
        private Vector3 homeScale;
        private Quaternion homeRotation;
        private bool homeCaptured;
        private bool interactable;
        private Coroutine moveRoutine;

        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            EnsureReferences();
        }

        internal void Initialize(C02BoxStackGame owner, Transform topDragLayer)
        {
            EnsureReferences();
            game = owner;
            dragLayer = topDragLayer;

            if (!homeCaptured)
            {
                CaptureHome();
            }
        }

        internal void SetInteractable(bool value)
        {
            EnsureReferences();
            interactable = value;
            canvasGroup.blocksRaycasts = value;
        }

        internal void ResetHome()
        {
            if (!homeCaptured)
            {
                CaptureHome();
            }

            StopMove();
            transform.SetParent(homeParent, false);
            transform.SetSiblingIndex(homeSiblingIndex);
            rectTransform.anchoredPosition = homePosition;
            rectTransform.sizeDelta = homeSize;
            rectTransform.localScale = homeScale;
            rectTransform.localRotation = homeRotation;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = interactable;
        }

        internal void SnapTo(RectTransform slot, bool fitToSlot, bool preserveLayerAlignment, Vector2 alignedPosition)
        {
            StopMove();

            if (preserveLayerAlignment)
            {
                transform.SetParent(homeParent, false);
                rectTransform.anchoredPosition = alignedPosition;
                rectTransform.localRotation = Quaternion.identity;
                rectTransform.localScale = Vector3.one;
                SetInteractable(false);
                return;
            }

            transform.SetParent(slot, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;

            if (fitToSlot)
            {
                rectTransform.sizeDelta = slot.rect.size;
            }

            SetInteractable(false);
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            EnsureReferences();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    screenPoint,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = rectTransform.rect;
            Vector2 normalizedPoint = new(
                Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x),
                Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y));

            return normalizedPoint.x >= normalizedRaycastMin.x
                && normalizedPoint.x <= normalizedRaycastMax.x
                && normalizedPoint.y >= normalizedRaycastMin.y
                && normalizedPoint.y <= normalizedRaycastMax.y;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!interactable || game == null || game.Canvas == null)
            {
                return;
            }

            StopMove();
            transform.SetParent(dragLayer, true);
            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!interactable || game == null || game.Canvas == null)
            {
                return;
            }

            rectTransform.anchoredPosition += eventData.delta / game.Canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!interactable || game == null)
            {
                return;
            }

            canvasGroup.blocksRaycasts = true;

            if (!game.TryPlace(this, eventData.position, eventData.pressEventCamera))
            {
                ReturnHome();
            }
        }

        private void CaptureHome()
        {
            EnsureReferences();
            homeParent = transform.parent;
            homeSiblingIndex = transform.GetSiblingIndex();
            homePosition = rectTransform.anchoredPosition;
            homeSize = rectTransform.sizeDelta;
            homeScale = rectTransform.localScale;
            homeRotation = rectTransform.localRotation;
            homeCaptured = true;
        }

        private void EnsureReferences()
        {
            if (rectTransform == null)
            {
                rectTransform = (RectTransform)transform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void ReturnHome()
        {
            StopMove();
            transform.SetParent(homeParent, true);
            moveRoutine = StartCoroutine(ReturnHomeRoutine());
        }

        private IEnumerator ReturnHomeRoutine()
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < returnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / returnDuration);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, homePosition, t);
                yield return null;
            }

            rectTransform.anchoredPosition = homePosition;
            transform.SetSiblingIndex(homeSiblingIndex);
            moveRoutine = null;
        }

        private void StopMove()
        {
            if (moveRoutine == null)
            {
                return;
            }

            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }
}
