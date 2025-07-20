using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// FlowerBloomingゲームのメインロジックを管理するクラス
    /// </summary>
    public class FlowerBloomingGameManager : MonoBehaviour
    {
        #region Singleton
        public static FlowerBloomingGameManager Instance { get; private set; }

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
        }
        #endregion

        #region SerializeFields
        [Header("Flower Prefabs")]
        [SerializeField] private GameObject[] normalFlowerPrefabs;
        [SerializeField] private GameObject[] specialFlowerPrefabs;

        [Header("Probability Settings")]
        [SerializeField] private float[] normalFlowerProbabilities = { 14f, 14f, 14f, 14f, 14f, 14f }; // 各14%
        [SerializeField] private float[] specialFlowerProbabilities = { 6f, 6f, 6f }; // 各6%

        [Header("References")]
        [SerializeField] private FlowerBloomingSFXPlayer sfxPlayer;

        [Header("Animation Settings")]
        [SerializeField] private float fadeOutDuration = 0.7f;
        #endregion

        #region Private Fields
        private Dictionary<GameObject, bool> planterAvailability = new Dictionary<GameObject, bool>();
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // SFXPlayerの参照を取得
            if (sfxPlayer == null)
            {
                sfxPlayer = FindObjectOfType<FlowerBloomingSFXPlayer>();
                if (sfxPlayer == null)
                {
                    Debug.LogError("FlowerBloomingSFXPlayerが見つかりません");
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// プランターをタップした時の処理
        /// </summary>
        /// <param name="planter">タップされたプランター</param>
        public void OnPlanterTapped(GameObject planter)
        {
            // プランターが使用可能かチェック
            if (planterAvailability.ContainsKey(planter) && !planterAvailability[planter])
            {
                return; // 既に花が咲いている場合は何もしない
            }

            // プランターを使用不可に設定
            planterAvailability[planter] = false;

            // ランダムに花を選択して生成
            GameObject flowerPrefab = SelectRandomFlower();
            if (flowerPrefab != null)
            {
                // 花を生成
                GameObject flower = Instantiate(flowerPrefab, planter.transform.position, Quaternion.identity);
                flower.transform.SetParent(planter.transform);
                
                // 花のSpriteRendererのSorting Orderを設定
                SpriteRenderer flowerRenderer = flower.GetComponent<SpriteRenderer>();
                if (flowerRenderer != null)
                {
                    // プランターよりも前面に表示されるようにSorting Orderを設定
                    flowerRenderer.sortingOrder = 10;
                    Debug.Log($"花のSorting Orderを設定: {flowerRenderer.sortingOrder}");
                }
                
                // 効果音はフェードアウト処理内で再生するため、ここでは再生しない

                // フェードアウト処理を開始
                StartCoroutine(FadeOutFlower(flower, planter));
            }
            else
            {
                Debug.LogError("花のプレハブが選択できませんでした");
                planterAvailability[planter] = true; // プランターを再利用可能に
            }
        }

        /// <summary>
        /// プランターを登録する
        /// </summary>
        /// <param name="planter">登録するプランター</param>
        public void RegisterPlanter(GameObject planter)
        {
            if (!planterAvailability.ContainsKey(planter))
            {
                planterAvailability.Add(planter, true);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// ランダムに花を選択する
        /// </summary>
        /// <returns>選択された花のプレハブ</returns>
        private GameObject SelectRandomFlower()
        {
            float totalProbability = 0f;

            // 全確率の合計を計算
            for (int i = 0; i < normalFlowerProbabilities.Length; i++)
            {
                totalProbability += normalFlowerProbabilities[i];
            }

            for (int i = 0; i < specialFlowerProbabilities.Length; i++)
            {
                totalProbability += specialFlowerProbabilities[i];
            }

            // 0〜100%の範囲でランダムな値を生成
            float randomValue = Random.Range(0f, totalProbability);
            float currentProbability = 0f;

            // 通常の花をチェック
            for (int i = 0; i < normalFlowerProbabilities.Length; i++)
            {
                currentProbability += normalFlowerProbabilities[i];
                if (randomValue <= currentProbability && i < normalFlowerPrefabs.Length)
                {
                    return normalFlowerPrefabs[i];
                }
            }

            // 特殊な花をチェック
            for (int i = 0; i < specialFlowerProbabilities.Length; i++)
            {
                currentProbability += specialFlowerProbabilities[i];
                if (randomValue <= currentProbability && i < specialFlowerPrefabs.Length)
                {
                    return specialFlowerPrefabs[i];
                }
            }

            // 万が一何も選ばれなかった場合は最初の通常花を返す
            return normalFlowerPrefabs.Length > 0 ? normalFlowerPrefabs[0] : null;
        }

        /// <summary>
        /// 花の種類に応じたSFXを再生する
        /// </summary>
        /// <param name="flowerName">花の名前</param>
        private void PlayFlowerSFX(string flowerName)
        {
            if (sfxPlayer == null) return;

            if (flowerName.Contains("Special"))
            {
                sfxPlayer.PlaySpecial();
            }
            else
            {
                sfxPlayer.PlayNormal();
            }
        }

        /// <summary>
        /// 花をフェードアウトさせる
        /// </summary>
        /// <param name="flower">フェードアウトさせる花</param>
        /// <param name="planter">花が咲いたプランター</param>
        /// <returns>コルーチン</returns>
        private IEnumerator FadeOutFlower(GameObject flower, GameObject planter)
        {
            // アニメーション情報を取得
            Animator animator = flower.GetComponent<Animator>();
            float animationLength = 1.0f; // デフォルト値
            string flowerName = flower.name;
            
            if (animator != null)
            {
                // アニメーションの長さを取得
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    animationLength = clipInfo[0].clip.length;
                    Debug.Log($"アニメーション長: {animationLength}秒");
                    
                    // アニメーションの最終フレームまで待機（全体の90%程度で最終フレームと仮定）
                    float waitTime = animationLength * 0.9f;
                    yield return new WaitForSeconds(waitTime);
                    
                    // 花が咲く最終フレームで効果音を再生
                    PlayFlowerSFX(flowerName);
                    Debug.Log($"花が咲くタイミングで効果音を再生: {flowerName}");
                    
                    // 残りのアニメーション時間を待機
                    yield return new WaitForSeconds(animationLength - waitTime);
                }
                else
                {
                    yield return new WaitForSeconds(1.0f); // デフォルト待機時間
                    // 効果音を再生
                    PlayFlowerSFX(flowerName);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f); // デフォルト待機時間
                // 効果音を再生
                PlayFlowerSFX(flowerName);
            }

            // フェードアウト処理
            SpriteRenderer spriteRenderer = flower.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                float elapsedTime = 0f;
                Color originalColor = spriteRenderer.color;

                while (elapsedTime < fadeOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }

            // 花を削除
            Destroy(flower);

            // プランターを再利用可能に設定
            planterAvailability[planter] = true;
        }
        #endregion
    }
}
