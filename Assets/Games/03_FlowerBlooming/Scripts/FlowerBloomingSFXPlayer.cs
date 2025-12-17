using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// FlowerBloomingゲームの効果音を管理するクラス
    /// </summary>
    public class FlowerBloomingSFXPlayer : MonoBehaviour
    {
        #region Singleton
        public static FlowerBloomingSFXPlayer Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            // AudioSourceの初期化
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // AudioClipの読み込み
            LoadAudioClips();
        }
        #endregion

        #region SerializeFields
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip normalFlowerSFX;
        [SerializeField] private AudioClip specialFlowerSFX;
        [SerializeField] private float volume = 1.0f;
        [SerializeField] private float normalVolume = 1.0f;
        [SerializeField] private float specialVolume = 1.0f;
        [SerializeField] private AudioClip singingFlowerSFX;
        [SerializeField] private float singingVolume = 1.0f;
        #endregion

    #region Private Fields
        private const string NormalSFXPath = "Assets/Games/03_FlowerBlooming/Audios/SFX_Flower_Normal_Appear.mp3";
        private const string SpecialSFXPath = "Assets/Games/03_FlowerBlooming/Audios/SFX_Flower_Special_Appear.mp3";
        private const string SingingSFXPath = "Assets/Games/03_FlowerBlooming/Audios/SFX_Flower_Singing.mp3";
        private const string NormalSFXResourcePath = "03_FlowerBlooming/Audios/SFX_Flower_Normal_Appear";
        private const string SpecialSFXResourcePath = "03_FlowerBlooming/Audios/SFX_Flower_Special_Appear";
        private const string SingingSFXResourcePath = "03_FlowerBlooming/Audios/SFX_Flower_Singing";
    #endregion

        #region Public Methods
        /// <summary>
        /// 通常の花の効果音を再生する
        /// </summary>
        public void PlayNormal()
        {
            if (normalFlowerSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(normalFlowerSFX, volume * normalVolume);
            }
            else
            {
                Debug.LogWarning("通常の花の効果音が設定されていません");
            }
        }

        /// <summary>
        /// 特殊な花の効果音を再生する
        /// </summary>
        public void PlaySpecial()
        {
            if (specialFlowerSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(specialFlowerSFX, volume * specialVolume);
            }
            else
            {
                Debug.LogWarning("特殊な花の効果音が設定されていません");
            }
        }

        /// <summary>
        /// 歌う花の効果音を再生する
        /// </summary>
        public void PlaySinging()
        {
            if (singingFlowerSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(singingFlowerSFX, volume * singingVolume);
            }
            else
            {
                Debug.LogWarning("歌う花の効果音が設定されていません");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 効果音ファイルを読み込む
        /// </summary>
        private void LoadAudioClips()
        {
            // 通常の花の効果音を読み込む
            if (normalFlowerSFX == null)
            {
#if UNITY_EDITOR
                // エディタ実行時はAssetDatabaseを使用
                normalFlowerSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(NormalSFXPath);
#else
                // ビルド実行時はResourcesを使用
                normalFlowerSFX = Resources.Load<AudioClip>(NormalSFXResourcePath);
#endif
                if (normalFlowerSFX == null)
                {
                    Debug.LogError($"通常の花の効果音が見つかりません");
                }
            }

            // 特殊な花の効果音を読み込む
            if (specialFlowerSFX == null)
            {
#if UNITY_EDITOR
                // エディタ実行時はAssetDatabaseを使用
                specialFlowerSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(SpecialSFXPath);
#else
                // ビルド実行時はResourcesを使用
                specialFlowerSFX = Resources.Load<AudioClip>(SpecialSFXResourcePath);
#endif
                if (specialFlowerSFX == null)
                {
                    Debug.LogError($"特殊な花の効果音が見つかりません");
                }
            }

            // 歌う花の効果音を読み込む
            if (singingFlowerSFX == null)
            {
#if UNITY_EDITOR
                // エディタ実行時はAssetDatabaseを使用
                singingFlowerSFX = AssetDatabase.LoadAssetAtPath<AudioClip>(SingingSFXPath);
#else
                // ビルド実行時はResourcesを使用
                singingFlowerSFX = Resources.Load<AudioClip>(SingingSFXResourcePath);
#endif
                if (singingFlowerSFX == null)
                {
                    Debug.LogError($"歌う花の効果音が見つかりません");
                }
            }
        }
        #endregion
    }
}
