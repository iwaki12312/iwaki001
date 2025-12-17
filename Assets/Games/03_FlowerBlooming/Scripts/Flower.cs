using UnityEngine;

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// 花のエフェクト制御（特殊/歌う花）
    /// </summary>
    public class Flower : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool isSpecial = false;
        [SerializeField] private bool isSinging = false;
        [SerializeField] private ParticleSystem particleEffect;
        [SerializeField] private NoteFloatEffect noteEffect;

        private bool hasPlayedNoteEffect = false;

        private void Start()
        {
            SetupSpecialEffect();
        }

        private void SetupSpecialEffect()
        {
            if (!isSpecial || particleEffect == null) return;

            var renderer = particleEffect.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 20;
            }

            // 既存挙動に合わせて少し遅らせて再生
            Invoke(nameof(PlayParticleEffect), 1.2f);
        }

        private void PlayParticleEffect()
        {
            particleEffect.Play();
        }

        public void SetSpecial(bool special)
        {
            isSpecial = special;

            if (!isSpecial && particleEffect != null && particleEffect.isPlaying)
            {
                particleEffect.Stop();
            }
        }

        public void SetSinging(bool singing)
        {
            isSinging = singing;
            hasPlayedNoteEffect = false;

            if (!isSinging && noteEffect != null)
            {
                noteEffect.ResetEffect();
            }
        }

        public void SetParticleEffect(ParticleSystem effect)
        {
            particleEffect = effect;

            if (particleEffect != null)
            {
                var renderer = particleEffect.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = 20;
                }
            }
        }

        public void SetNoteEffect(NoteFloatEffect effect)
        {
            noteEffect = effect;
            hasPlayedNoteEffect = false;
        }

        public void PlayNoteEffect()
        {
            if (!isSinging) return;
            if (noteEffect == null) return;
            if (hasPlayedNoteEffect) return;

            noteEffect.Play();
            hasPlayedNoteEffect = true;
        }
    }
}
