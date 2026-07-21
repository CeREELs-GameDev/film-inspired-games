using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FilmInspiredGames.Burning.C13
{
    public sealed class C13WatchMemoryController : MonoBehaviour
    {
        [Header("시곗바늘")]
        [SerializeField] private RectTransform minuteHand;
        [SerializeField] private RectTransform hourHand;
        [SerializeField] private RectTransform watchCenter;
        [SerializeField, Min(1f)] private float interactionRadius = 115f;
        [SerializeField, Min(1f)] private float finalRotation = 1350f;

        [Header("기억 이미지")]
        [SerializeField] private CanvasGroup walkImage;
        [SerializeField] private CanvasGroup glance1Image;
        [SerializeField] private CanvasGroup glance2Image;
        [SerializeField] private CanvasGroup stopImage;
        [SerializeField] private CanvasGroup stopMark1;
        [SerializeField] private CanvasGroup stopMark2;
        [SerializeField] private CanvasGroup pubImage;
        [SerializeField, Min(1f)] private float revealAngle = 55f;

        [Header("다음 장면")]
        [SerializeField] private CanvasGroup transitionOverlay;
        [SerializeField] private string nextSceneName = "Burning_C14_Playable";
        [SerializeField, Min(0.1f)] private float transitionDuration = 1.1f;

        private bool dragging;
        private bool hasInteracted;
        private float previousPointerAngle;
        private float accumulatedRotation;
        private Camera canvasCamera;
        private bool transitioning;

        public string CurrentChapter => "C13";
        public string CurrentState { get; private set; } = "시곗바늘을 돌려 기억 탐색";
        public float Progress => Mathf.Clamp01(accumulatedRotation / finalRotation);

        private void Start()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            ApplyVisuals();
        }

        private void Update()
        {
            Vector2 pointerPosition = default;
            bool pressed = false;
            bool held = false;
            bool released = false;

            if (Touchscreen.current?.primaryTouch.press.isPressed == true)
            {
                pointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                pressed = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
                held = true;
                released = Touchscreen.current.primaryTouch.press.wasReleasedThisFrame;
            }
            else if (Mouse.current != null)
            {
                pointerPosition = Mouse.current.position.ReadValue();
                pressed = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
            }

            if (pressed && IsNearWatch(pointerPosition))
            {
                dragging = true;
                hasInteracted = true;
                previousPointerAngle = PointerAngle(pointerPosition);
                ApplyVisuals();
            }

            if (pressed && accumulatedRotation >= finalRotation && !dragging && !transitioning)
            {
                StartCoroutine(LoadNextScene());
            }

            if (dragging && held)
            {
                float angle = PointerAngle(pointerPosition);
                float delta = Mathf.DeltaAngle(previousPointerAngle, angle);
                previousPointerAngle = angle;

                if (Mathf.Abs(delta) < 50f)
                {
                    accumulatedRotation = Mathf.Clamp(
                        accumulatedRotation - delta, 0f, finalRotation);
                    ApplyVisuals();
                }
            }

            if (released || !held)
            {
                dragging = false;
            }

            if (!hasInteracted && minuteHand != null)
            {
                float hint = Mathf.Sin(Time.unscaledTime * 2.8f) * 5f;
                minuteHand.localRotation = Quaternion.Euler(0f, 0f, hint);
            }
        }

        private bool IsNearWatch(Vector2 screenPosition)
        {
            if (watchCenter == null)
            {
                return false;
            }

            Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCamera, watchCenter.position);
            float scaledRadius = interactionRadius * Screen.width / 540f;
            return Vector2.Distance(center, screenPosition) <= scaledRadius;
        }

        private float PointerAngle(Vector2 screenPosition)
        {
            Vector2 center = RectTransformUtility.WorldToScreenPoint(canvasCamera, watchCenter.position);
            Vector2 direction = screenPosition - center;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private void ApplyVisuals()
        {
            if (minuteHand != null)
            {
                minuteHand.localRotation = Quaternion.Euler(0f, 0f, -accumulatedRotation);
            }

            if (hourHand != null)
            {
                hourHand.localRotation = Quaternion.Euler(0f, 0f, -accumulatedRotation / 12f);
            }

            float walk = WindowAlpha(accumulatedRotation, 270f, 630f);
            float glance = WindowAlpha(accumulatedRotation, 630f, 990f);
            float stop = WindowAlpha(accumulatedRotation, 990f, 1350f);
            float pub = Ramp(accumulatedRotation, 1350f);
            float glanceSwap = Ramp(accumulatedRotation, 810f);

            SetAlpha(walkImage, walk);
            SetAlpha(glance1Image, glance * (1f - glanceSwap));
            SetAlpha(glance2Image, glance * glanceSwap);
            SetAlpha(stopImage, stop);
            SetAlpha(stopMark1, stop * Ramp(accumulatedRotation, 1080f));
            SetAlpha(stopMark2, stop * Ramp(accumulatedRotation, 1170f));
            SetAlpha(pubImage, pub);

            CurrentState = accumulatedRotation switch
            {
                < 270f => "시곗바늘을 돌려 기억 탐색",
                < 630f => "함께 걷던 시간",
                < 990f => "힐끔거리는 종수",
                < 1080f => "멈춘 발걸음",
                < 1170f => "멈춤 표시 첫 번째",
                < 1350f => "멈춤 표시 두 번째",
                _ => "봉천동 포차 / 클릭하여 C14 이동"
            };
        }

        private IEnumerator LoadNextScene()
        {
            transitioning = true;
            CurrentState = "C14로 전환";
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (transitionOverlay != null)
                {
                    float t = Mathf.Clamp01(elapsed / transitionDuration);
                    transitionOverlay.alpha = t * t * (3f - 2f * t);
                }
                yield return null;
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private float WindowAlpha(float value, float start, float end)
        {
            float fadeIn = Ramp(value, start);
            float fadeOut = 1f - Ramp(value, end);
            return Mathf.Min(fadeIn, fadeOut);
        }

        private float Ramp(float value, float start)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, start + revealAngle, value));
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
