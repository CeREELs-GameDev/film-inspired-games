using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace FilmInspiredGames.Burning.C04
{
    public sealed class C04RewardSequenceController : MonoBehaviour
    {
        [Header("손잡이")]
        [SerializeField] private RectTransform handleRect;
        [SerializeField, Min(30f)] private float handleInteractionRadius = 82f;
        [SerializeField, Min(45f)] private float requiredHandleRotation = 180f;
        [SerializeField, Min(0f)] private float handleCompleteHold = 0.25f;

        [Header("캡슐")]
        [SerializeField] private CanvasGroup capsuleUp;
        [SerializeField] private CanvasGroup capsuleDown;
        [SerializeField] private RectTransform capsuleUpRect;
        [SerializeField] private RectTransform capsuleDownRect;

        [Header("보상")]
        [SerializeField] private CanvasGroup watch;
        [SerializeField] private RectTransform watchRect;

        [Header("등장")]
        [SerializeField, Min(0f)] private float openingHold = 0.45f;
        [SerializeField, Min(0.01f)] private float capsuleAppearDuration = 0.68f;
        [SerializeField, Range(0.5f, 1f)] private float capsuleStartScale = 0.84f;
        [SerializeField, Range(0f, 0.2f)] private float capsuleScalePop = 0.075f;
        [SerializeField, Range(0f, 0.1f)] private float capsuleSettleDip = 0.018f;
        [SerializeField, Min(0f)] private float capsuleHold = 0.35f;

        [Header("입력 대기")]
        [SerializeField, Min(0.1f)] private float idlePulseDuration = 1f;
        [SerializeField, Range(0f, 0.1f)] private float idleScaleAmount = 0.025f;
        [SerializeField, Min(0f)] private float idleLiftAmount = 8f;
        [SerializeField] private Vector2 capsuleHitAreaMin = new(0.14f, 0.27f);
        [SerializeField] private Vector2 capsuleHitAreaMax = new(0.86f, 0.72f);

        [Header("열림")]
        [SerializeField, Min(0.01f)] private float capsuleOpenDuration = 0.46f;
        [SerializeField] private Vector2 capsuleUpOffset = new(95f, 130f);
        [SerializeField] private Vector2 capsuleDownOffset = new(-70f, -150f);
        [SerializeField] private float capsuleUpRotation = -10f;
        [SerializeField] private float capsuleDownRotation = 8f;
        [SerializeField, Range(0f, 1f)] private float openedCapsuleAlpha = 0.16f;

        [Header("시계")]
        [SerializeField, Min(0f)] private float watchAppearDelay = 0.08f;
        [SerializeField, Min(0.01f)] private float watchFadeDuration = 0.42f;
        [SerializeField, Min(0f)] private float rewardHold = 0.35f;
        [SerializeField] private bool playOnStart = true;

        [Header("장면 신호")]
        [SerializeField] private UnityEvent onCapsuleAppeared;
        [SerializeField] private UnityEvent onCapsuleOpened;
        [SerializeField] private UnityEvent onRewardShown;

        private Coroutine sequenceRoutine;
        private bool openRequested;
        private bool draggingHandle;
        private bool handleTurnComplete;
        private bool hasTouchedHandle;
        private float previousPointerAngle;
        private float handleRotation;
        private Vector2 handleStartPosition;
        private Camera canvasCamera;

        public event Action Finished;
        public bool IsWaitingForHandle { get; private set; }
        public bool IsWaitingForOpen { get; private set; }
        public float HandleProgress => Mathf.Clamp01(handleRotation / requiredHandleRotation);
        public string CurrentState { get; private set; } = "대기";

        private void Start()
        {
            if (handleRect != null)
            {
                handleStartPosition = handleRect.anchoredPosition;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            Prepare();

            if (playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            ReadPointer(out Vector2 pointerPosition, out bool pressed, out bool held, out bool released);

            if (IsWaitingForHandle)
            {
                UpdateHandle(pointerPosition, pressed, held, released);
                return;
            }

            if (!IsWaitingForOpen)
            {
                return;
            }

            if (pressed && IsInsideCapsule(pointerPosition))
            {
                RequestOpen();
            }
        }

        public void Play()
        {
            StopSequence();
            Prepare();
            sequenceRoutine = StartCoroutine(PlayRoutine());
        }

        public void RequestOpen()
        {
            if (IsWaitingForOpen)
            {
                openRequested = true;
            }
        }

        public void StopSequence()
        {
            if (sequenceRoutine == null)
            {
                return;
            }

            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
            IsWaitingForHandle = false;
            IsWaitingForOpen = false;
            openRequested = false;
        }

        private void Prepare()
        {
            SetAlpha(capsuleUp, 0f);
            SetAlpha(capsuleDown, 0f);
            SetAlpha(watch, 0f);

            ResetHandle();
            ResetRect(capsuleUpRect, capsuleStartScale);
            ResetRect(capsuleDownRect, capsuleStartScale);
            ResetRect(watchRect, 1f);
            IsWaitingForHandle = false;
            IsWaitingForOpen = false;
            openRequested = false;
            draggingHandle = false;
            handleTurnComplete = false;
            hasTouchedHandle = false;
            handleRotation = 0f;
            CurrentState = "손잡이 돌리기 대기";
        }

        private IEnumerator PlayRoutine()
        {
            yield return new WaitForSecondsRealtime(openingHold);
            yield return AnimateHandleUntilTurned();
            yield return new WaitForSecondsRealtime(handleCompleteHold);

            CurrentState = "캡슐 등장";
            yield return AnimateCapsuleAppearance();
            onCapsuleAppeared?.Invoke();

            yield return AnimateCapsuleIdleUntilClicked();
            CurrentState = "캡슐 열리는 중";
            yield return AnimateCapsuleOpening();
            onCapsuleOpened?.Invoke();

            if (watch != null && watch.alpha < 1f)
            {
                yield return Fade(watch, watch.alpha, 1f, watchFadeDuration);
            }

            onRewardShown?.Invoke();
            CurrentState = "시계 획득";
            yield return new WaitForSecondsRealtime(rewardHold);

            sequenceRoutine = null;
            Finished?.Invoke();
        }

        private IEnumerator AnimateHandleUntilTurned()
        {
            IsWaitingForHandle = true;
            CurrentState = "손잡이를 시계 방향으로 돌리기";
            float hintTime = 0f;

            while (!handleTurnComplete)
            {
                if (!draggingHandle && !hasTouchedHandle && handleRect != null)
                {
                    hintTime += Time.unscaledDeltaTime;
                    float pulse = (1f - Mathf.Cos(hintTime * Mathf.PI * 1.6f)) * 0.5f;
                    handleRect.localRotation = Quaternion.Euler(0f, 0f, -pulse * 7f);
                }
                yield return null;
            }

            IsWaitingForHandle = false;
            CurrentState = "손잡이 회전 완료";
            float start = handleRotation;
            float elapsed = 0f;
            const float snapDuration = 0.28f;

            while (elapsed < snapDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / snapDuration);
                float overshoot = Mathf.Sin(t * Mathf.PI) * 12f;
                SetHandleRotation(Mathf.Lerp(start, requiredHandleRotation, EaseOutCubic(t)) + overshoot);
                yield return null;
            }

            handleRotation = requiredHandleRotation;
            SetHandleRotation(handleRotation);
        }

        private void UpdateHandle(Vector2 pointerPosition, bool pressed, bool held, bool released)
        {
            if (pressed && IsInsideHandle(pointerPosition))
            {
                draggingHandle = true;
                hasTouchedHandle = true;
                previousPointerAngle = PointerAngle(pointerPosition);
                SetHandleRotation(handleRotation);
            }

            if (draggingHandle && held)
            {
                float angle = PointerAngle(pointerPosition);
                float delta = Mathf.DeltaAngle(previousPointerAngle, angle);
                previousPointerAngle = angle;

                if (Mathf.Abs(delta) < 70f)
                {
                    handleRotation = Mathf.Clamp(
                        handleRotation + Mathf.Max(0f, -delta), 0f, requiredHandleRotation);
                    SetHandleRotation(handleRotation);
                }

                if (handleRotation >= requiredHandleRotation)
                {
                    draggingHandle = false;
                    handleTurnComplete = true;
                }
            }

            if (released || !held)
            {
                draggingHandle = false;
            }
        }

        private IEnumerator AnimateCapsuleAppearance()
        {
            float elapsed = 0f;

            while (elapsed < capsuleAppearDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / capsuleAppearDuration);
                float alpha = EaseOutCubic(normalized);
                float scale = EvaluateCapsulePop(normalized);

                SetAlpha(capsuleUp, alpha);
                SetAlpha(capsuleDown, alpha);
                SetScale(capsuleUpRect, scale);
                SetScale(capsuleDownRect, scale);
                yield return null;
            }

            SetAlpha(capsuleUp, 1f);
            SetAlpha(capsuleDown, 1f);
            SetScale(capsuleUpRect, 1f);
            SetScale(capsuleDownRect, 1f);
        }

        private IEnumerator AnimateCapsuleIdleUntilClicked()
        {
            IsWaitingForOpen = true;
            CurrentState = "캡슐을 눌러 열기";
            float elapsed = 0f;

            while (!openRequested || elapsed < capsuleHold)
            {
                elapsed += Time.unscaledDeltaTime;
                float phase = elapsed / idlePulseDuration * Mathf.PI * 2f;
                float pulse = (1f - Mathf.Cos(phase)) * 0.5f;
                float scale = 1f + pulse * idleScaleAmount;
                float lift = pulse * idleLiftAmount;

                SetIdlePose(capsuleUpRect, scale, lift);
                SetIdlePose(capsuleDownRect, scale, lift);
                yield return null;
            }

            IsWaitingForOpen = false;
            SetIdlePose(capsuleUpRect, 1f, 0f);
            SetIdlePose(capsuleDownRect, 1f, 0f);
        }

        private IEnumerator AnimateCapsuleOpening()
        {
            float elapsed = 0f;

            while (elapsed < capsuleOpenDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / capsuleOpenDuration);
                float movement = EaseOutBack(normalized, 1.35f);
                float fade = EaseOutCubic(normalized);

                AnimateCapsulePiece(capsuleUp, capsuleUpRect, capsuleUpOffset, capsuleUpRotation, movement, fade);
                AnimateCapsulePiece(capsuleDown, capsuleDownRect, capsuleDownOffset, capsuleDownRotation, movement, fade);

                float watchNormalized = Mathf.Clamp01((elapsed - watchAppearDelay) / watchFadeDuration);
                SetAlpha(watch, Mathf.SmoothStep(0f, 1f, watchNormalized));
                yield return null;
            }

            AnimateCapsulePiece(capsuleUp, capsuleUpRect, capsuleUpOffset, capsuleUpRotation, 1f, 1f);
            AnimateCapsulePiece(capsuleDown, capsuleDownRect, capsuleDownOffset, capsuleDownRotation, 1f, 1f);
        }

        private float EvaluateCapsulePop(float normalized)
        {
            const float popEnd = 0.62f;
            const float reboundEnd = 0.84f;

            if (normalized < popEnd)
            {
                float t = EaseOutCubic(normalized / popEnd);
                return Mathf.LerpUnclamped(capsuleStartScale, 1f + capsuleScalePop, t);
            }

            if (normalized < reboundEnd)
            {
                float t = Mathf.SmoothStep(0f, 1f, (normalized - popEnd) / (reboundEnd - popEnd));
                return Mathf.LerpUnclamped(1f + capsuleScalePop, 1f - capsuleSettleDip, t);
            }

            float settle = Mathf.SmoothStep(0f, 1f, (normalized - reboundEnd) / (1f - reboundEnd));
            return Mathf.LerpUnclamped(1f - capsuleSettleDip, 1f, settle);
        }

        private void AnimateCapsulePiece(
            CanvasGroup group,
            RectTransform rect,
            Vector2 offset,
            float rotation,
            float movement,
            float fade)
        {
            SetAlpha(group, Mathf.LerpUnclamped(1f, openedCapsuleAlpha, fade));

            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, offset, movement);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(0f, rotation, movement));
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - Mathf.Clamp01(value);
            return 1f - inverse * inverse * inverse;
        }

        private static float EaseOutBack(float value, float overshoot)
        {
            float shifted = Mathf.Clamp01(value) - 1f;
            return 1f + (overshoot + 1f) * shifted * shifted * shifted
                + overshoot * shifted * shifted;
        }

        private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                SetAlpha(group, Mathf.LerpUnclamped(from, to, normalized));
                yield return null;
            }

            SetAlpha(group, to);
        }

        private static void ResetRect(RectTransform rect, float scale)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            SetScale(rect, scale);
        }

        private void ResetHandle()
        {
            if (handleRect == null)
            {
                return;
            }

            handleRect.anchoredPosition = handleStartPosition;
            handleRect.localRotation = Quaternion.identity;
            SetScale(handleRect, 1f);
        }

        private static void SetIdlePose(RectTransform rect, float scale, float lift)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = new Vector2(0f, lift);
            SetScale(rect, scale);
        }

        private bool IsInsideCapsule(Vector2 screenPosition)
        {
            if (capsuleUpRect == null
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    capsuleUpRect,
                    screenPosition,
                    null,
                    out Vector2 localPosition))
            {
                return false;
            }

            Rect rect = capsuleUpRect.rect;
            Vector2 normalized = new(
                Mathf.InverseLerp(rect.xMin, rect.xMax, localPosition.x),
                Mathf.InverseLerp(rect.yMin, rect.yMax, localPosition.y));

            return normalized.x >= capsuleHitAreaMin.x
                && normalized.x <= capsuleHitAreaMax.x
                && normalized.y >= capsuleHitAreaMin.y
                && normalized.y <= capsuleHitAreaMax.y;
        }

        private bool IsInsideHandle(Vector2 screenPosition)
        {
            if (handleRect == null)
            {
                return false;
            }

            Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCamera, handleRect.position);
            float radius = handleInteractionRadius * Screen.width / 540f;
            return Vector2.Distance(center, screenPosition) <= radius;
        }

        private float PointerAngle(Vector2 screenPosition)
        {
            Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCamera, handleRect.position);
            Vector2 direction = screenPosition - center;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private void SetHandleRotation(float clockwiseRotation)
        {
            if (handleRect != null)
            {
                handleRect.localRotation = Quaternion.Euler(0f, 0f, -clockwiseRotation);
            }
        }

        private static void ReadPointer(
            out Vector2 position,
            out bool pressed,
            out bool held,
            out bool released)
        {
            if (Touchscreen.current != null
                && (Touchscreen.current.primaryTouch.press.isPressed
                    || Touchscreen.current.primaryTouch.press.wasPressedThisFrame
                    || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
            {
                position = Touchscreen.current.primaryTouch.position.ReadValue();
                pressed = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                held = Touchscreen.current.primaryTouch.press.isPressed;
                released = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
                return;
            }

            if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                pressed = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
                return;
            }

            position = default;
            pressed = false;
            held = false;
            released = false;
        }

        private static void SetScale(RectTransform rect, float scale)
        {
            if (rect != null)
            {
                rect.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private static void SetAlpha(CanvasGroup group, float alpha)
        {
            if (group != null)
            {
                group.alpha = alpha;
            }
        }
    }
}
