using UnityEngine;

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// 花の機能を提供するクラス
    /// </summary>
    public class Flower : MonoBehaviour
    {
        #region SerializeFields
        [Header("Settings")]
        [SerializeField] private bool isSpecial = false;
        [SerializeField] private ParticleSystem particleEffect;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // 特殊な花の場合、パーティクルエフェクトを再生
            if (isSpecial && particleEffect != null)
            {
                particleEffect.Play();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 花が特殊かどうかを設定する
        /// </summary>
        /// <param name="special">特殊かどうか</param>
        public void SetSpecial(bool special)
        {
            isSpecial = special;
            
            // 特殊な花に設定された場合、パーティクルエフェクトを再生
            if (isSpecial && particleEffect != null && !particleEffect.isPlaying)
            {
                particleEffect.Play();
            }
            // 通常の花に設定された場合、パーティクルエフェクトを停止
            else if (!isSpecial && particleEffect != null && particleEffect.isPlaying)
            {
                particleEffect.Stop();
            }
        }

        /// <summary>
        /// パーティクルエフェクトを設定する
        /// </summary>
        /// <param name="effect">パーティクルエフェクト</param>
        public void SetParticleEffect(ParticleSystem effect)
        {
            particleEffect = effect;
            
            // 特殊な花の場合、パーティクルエフェクトを再生
            if (isSpecial && particleEffect != null && !particleEffect.isPlaying)
            {
                particleEffect.Play();
            }
        }
        #endregion
    }
}
