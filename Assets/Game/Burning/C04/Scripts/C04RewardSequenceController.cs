using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FilmInspiredGames.Burning.C04
{
    public sealed class C04RewardSequenceController : MonoBehaviour
    {
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
        [SerializeField, Min(0.01f)] private float capsuleAppearDuration = 0.55f;
        [SerializeField, Range(0.5f, 1f)] private float capsuleStartScale = 0.84f;
        [SerializeField, Range(0f, 0.2f)] private float capsuleScalePop = 0.06f;
        [SerializeField, Min(0f)] private float capsuleHold = 0.35f;

        [Header("열림")]
        [SerializeField, Min(0.01f)] private float capsuleOpenDuration = 0.7f;
        [SerializeField] private Vector2 capsuleUpOffset = new(95f, 130f);
        [SerializeField] private Vector2 capsuleDownOffset = new(-70f, -150f);
        [SerializeField] private float capsuleUpRotation = -10f;
        [SerializeField] private float capsuleDownRotation = 8f;
        [SerializeField, Range(0f, 1f)] private float openedCapsuleAlpha = 0.16f;

        [Header("시계")]
        [SerializeField, Min(0f)] private float watchAppearDelay = 0.15f;
        [SerializeField, Min(0.01f)] private float watchFadeDuration = 0.5f;
        [SerializeField, Min(0f)] private float rewardHold = 0.35f;
        [SerializeField] private bool playOnStart = true;

        [Header("장면 신호")]
        [SerializeField] private UnityEvent onCapsuleAppeared;
        [SerializeField] private UnityEvent onCapsuleOpened;
        [SerializeField] private UnityEvent onRewardShown;

        private Coroutine sequenceRoutine;

        public event Action Finished;

        private void Start()
        {
            Prepare();

            if (playOnStart)
            {
                Play();
            }
        }

        public void Play()
        {
            StopSequence();
            Prepare();
            sequenceRoutine = StartCoroutine(PlayRoutine());
        }

        public void StopSequence()
        {
            if (sequenceRoutine == null)
            {
                return;
            }

            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        private void Prepare()
        {
            SetAlpha(capsuleUp, 0f);
            SetAlpha(capsuleDown, 0f);
            SetAlpha(watch, 0f);

            ResetRect(capsuleUpRect, capsuleStartScale);
            ResetRect(capsuleDownRect, capsuleStartScale);
            ResetRect(watchRect, 1f);
        }

        private IEnumerator PlayRoutine()
        {
            yield return new WaitForSecondsRealtime(openingHold);
            yield return AnimateCapsuleAppearance();
            onCapsuleAppeared?.Invoke();

            yield return new WaitForSecondsRealtime(capsuleHold);
            yield return AnimateCapsuleOpening();
            onCapsuleOpened?.Invoke();

            if (watch != null && watch.alpha < 1f)
            {
                yield return Fade(watch, watch.alpha, 1f, watchFadeDuration);
            }

            onRewardShown?.Invoke();
            yield return new WaitForSecondsRealtime(rewardHold);

            sequenceRoutine = null;
            Finished?.Invoke();
        }

        private IEnumerator AnimateCapsuleAppearance()
        {
            float elapsed = 0f;

            while (elapsed < capsuleAppearDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / capsuleAppearDuration);
                float eased = Mathf.SmoothStep(0f, 1f, normalized);
                float scale = Mathf.LerpUnclamped(capsuleStartScale, 1f, eased)
                    + Mathf.Sin(normalized * Mathf.PI) * capsuleScalePop;

                SetAlpha(capsuleUp, eased);
                SetAlpha(capsuleDown, eased);
                SetScale(capsuleUpRect, scale);
                SetScale(capsuleDownRect, scale);
                yield return null;
            }

            SetAlpha(capsuleUp, 1f);
            SetAlpha(capsuleDown, 1f);
            SetScale(capsuleUpRect, 1f);
            SetScale(capsuleDownRect, 1f);
        }

        private IEnumerator AnimateCapsuleOpening()
        {
            float elapsed = 0f;

            while (elapsed < capsuleOpenDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / capsuleOpenDuration);
                float eased = Mathf.SmoothStep(0f, 1f, normalized);

                AnimateCapsulePiece(capsuleUp, capsuleUpRect, capsuleUpOffset, capsuleUpRotation, eased);
                AnimateCapsulePiece(capsuleDown, capsuleDownRect, capsuleDownOffset, capsuleDownRotation, eased);

                float watchNormalized = Mathf.Clamp01((elapsed - watchAppearDelay) / watchFadeDuration);
                SetAlpha(watch, Mathf.SmoothStep(0f, 1f, watchNormalized));
                yield return null;
            }

            AnimateCapsulePiece(capsuleUp, capsuleUpRect, capsuleUpOffset, capsuleUpRotation, 1f);
            AnimateCapsulePiece(capsuleDown, capsuleDownRect, capsuleDownOffset, capsuleDownRotation, 1f);
        }

        private void AnimateCapsulePiece(
            CanvasGroup group,
            RectTransform rect,
            Vector2 offset,
            float rotation,
            float normalized)
        {
            SetAlpha(group, Mathf.LerpUnclamped(1f, openedCapsuleAlpha, normalized));

            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, offset, normalized);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(0f, rotation, normalized));
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
