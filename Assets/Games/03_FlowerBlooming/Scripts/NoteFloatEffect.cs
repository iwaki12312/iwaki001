using System.Collections;
using UnityEngine;

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// 音符スプライトを漂わせてからフェードアウトさせる演出
    /// </summary>
    public class NoteFloatEffect : MonoBehaviour
    {
        [Header("Sprite Settings")]
        [SerializeField] private Sprite[] noteSprites;
        [SerializeField] private bool autoCreateRenderers = true;
        [SerializeField] private int noteSortingOrder = 20;
        [SerializeField] private bool hideOnStart = true;
        [SerializeField] private float hiddenAlpha = 0f;

        [Header("Layout Settings")]
        [SerializeField] private Vector2[] noteBaseOffsets;
        [SerializeField] private bool autoGenerateOffsets = true;
        [SerializeField] private float autoRadius = 0.3f;
        [SerializeField] private float autoArcDegrees = 120f;
        [SerializeField] private float autoCenterAngleDegrees = 90f;

        [Header("Float Settings")]
        [SerializeField] private float floatDuration = 1.5f;
        [SerializeField] private Vector2 floatDistance = new Vector2(0.25f, 1.0f);
        [SerializeField] private AnimationCurve floatCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool randomizeHorizontal = true;
        [SerializeField] private Vector2 horizontalJitter = new Vector2(-0.2f, 0.2f);

        [Header("Drift Settings")]
        [SerializeField] private float driftAmplitude = 0.05f;
        [SerializeField] private float driftSpeed = 1.2f;

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.7f;
        [SerializeField] private bool deactivateOnComplete = true;

        private SpriteRenderer[] spriteRenderers;
        private Vector3 initialLocalPosition;
        private Vector3[] noteInitialPositions;
        private float[] notePhaseOffsets;
        private Color[] initialColors;
        private Coroutine playRoutine;

        private void Awake()
        {
            EnsureRenderers();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            initialLocalPosition = transform.localPosition;
            SetupNoteOffsets();
            CacheInitialColors();
            ApplySpriteOverrides();
            if (hideOnStart)
            {
                HideVisuals();
            }
        }

        private void OnValidate()
        {
            EnsureRenderers();
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            SetupNoteOffsets();
            CacheInitialColors();
            ApplySpriteOverrides();
        }

        private void OnEnable()
        {
            if (hideOnStart && playRoutine == null)
            {
                HideVisuals();
            }
        }

        public void Play()
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0) return;

            ResetVisuals();
            gameObject.SetActive(true);

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }
            playRoutine = StartCoroutine(FloatAndFade());
        }

        public void ResetEffect()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            ResetVisuals();

            if (deactivateOnComplete)
            {
                gameObject.SetActive(false);
            }
            else if (hideOnStart)
            {
                HideVisuals();
            }
        }

        private void CacheInitialColors()
        {
            if (spriteRenderers == null) return;

            initialColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                initialColors[i] = spriteRenderers[i].color;
            }
        }

        private void SetupNoteOffsets()
        {
            if (spriteRenderers == null) return;

            int count = spriteRenderers.Length;
            if (count == 0) return;

            if (autoGenerateOffsets || noteBaseOffsets == null || noteBaseOffsets.Length != count)
            {
                noteBaseOffsets = new Vector2[count];
                if (count == 1)
                {
                    noteBaseOffsets[0] = Vector2.up * autoRadius;
                }
                else
                {
                    float startAngle = autoCenterAngleDegrees - autoArcDegrees * 0.5f;
                    float step = autoArcDegrees / (count - 1);
                    for (int i = 0; i < count; i++)
                    {
                        float angleDeg = startAngle + step * i;
                        float radians = angleDeg * Mathf.Deg2Rad;
                        noteBaseOffsets[i] = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * autoRadius;
                    }
                }
            }

            noteInitialPositions = new Vector3[count];
            notePhaseOffsets = new float[count];

            for (int i = 0; i < count; i++)
            {
                Transform noteTransform = spriteRenderers[i] != null ? spriteRenderers[i].transform : null;
                if (noteTransform == null) continue;

                Vector2 baseOffset = i < noteBaseOffsets.Length ? noteBaseOffsets[i] : Vector2.zero;
                noteTransform.localPosition = new Vector3(baseOffset.x, baseOffset.y, 0f);
                noteInitialPositions[i] = noteTransform.localPosition;
                notePhaseOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
            }
        }

        private void EnsureRenderers()
        {
            if (!autoCreateRenderers) return;
            if (noteSprites == null || noteSprites.Length == 0) return;

            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            int existingCount = spriteRenderers != null ? spriteRenderers.Length : 0;

            if (existingCount >= noteSprites.Length) return;

            for (int i = existingCount; i < noteSprites.Length; i++)
            {
                GameObject child = new GameObject($"NoteSprite_{i + 1}");
                child.transform.SetParent(transform, false);
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = noteSortingOrder;
            }
        }

        private void ApplySpriteOverrides()
        {
            if (noteSprites == null || noteSprites.Length == 0) return;
            if (spriteRenderers == null || spriteRenderers.Length == 0) return;

            int count = Mathf.Min(noteSprites.Length, spriteRenderers.Length);
            for (int i = 0; i < count; i++)
            {
                if (spriteRenderers[i] == null) continue;
                if (noteSprites[i] != null)
                {
                    spriteRenderers[i].sprite = noteSprites[i];
                }
                spriteRenderers[i].sortingOrder = noteSortingOrder;
            }
        }

        private void ResetVisuals()
        {
            transform.localPosition = initialLocalPosition;

            if (spriteRenderers == null) return;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;

                if (noteInitialPositions != null && i < noteInitialPositions.Length)
                {
                    spriteRenderers[i].transform.localPosition = noteInitialPositions[i];
                }

                Color baseColor = (initialColors != null && i < initialColors.Length)
                    ? initialColors[i]
                    : spriteRenderers[i].color;

                baseColor.a = 1f;
                spriteRenderers[i].color = baseColor;
            }
        }

        private IEnumerator FloatAndFade()
        {
            float elapsed = 0f;
            float randomOffsetX = 0f;

            if (randomizeHorizontal)
            {
                randomOffsetX = Random.Range(horizontalJitter.x, horizontalJitter.y);
            }

            while (elapsed < floatDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / floatDuration);
                float curve = floatCurve.Evaluate(t);

                Vector3 floatOffset = new Vector3(floatDistance.x * curve + randomOffsetX, floatDistance.y * t, 0f);

                if (spriteRenderers != null && noteInitialPositions != null)
                {
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        if (spriteRenderers[i] == null) continue;

                        Vector3 basePosition = i < noteInitialPositions.Length ? noteInitialPositions[i] : Vector3.zero;
                        float phase = i < notePhaseOffsets.Length ? notePhaseOffsets[i] : 0f;
                        float driftX = Mathf.Sin(Time.time * driftSpeed + phase) * driftAmplitude;
                        float driftY = Mathf.Cos(Time.time * driftSpeed + phase) * driftAmplitude;
                        Vector3 driftOffset = new Vector3(driftX, driftY, 0f);

                        spriteRenderers[i].transform.localPosition = basePosition + floatOffset + driftOffset;
                    }
                }

                yield return null;
            }

            float fadeElapsed = 0f;
            while (fadeElapsed < fadeDuration)
            {
                fadeElapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(fadeElapsed / fadeDuration));
                SetAlpha(alpha);
                yield return null;
            }

            if (deactivateOnComplete)
            {
                gameObject.SetActive(false);
            }

            playRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderers == null) return;

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;
                Color c = spriteRenderers[i].color;
                c.a = alpha;
                spriteRenderers[i].color = c;
            }
        }

        private void HideVisuals()
        {
            SetAlpha(hiddenAlpha);
        }
    }
}
