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
        private enum FlowerType
        {
            Normal,
            Special,
            Singing
        }

        private struct FlowerSelection
        {
            public GameObject Prefab;
            public FlowerType Type;

            public FlowerSelection(GameObject prefab, FlowerType type)
            {
                Prefab = prefab;
                Type = type;
            }
        }

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
        [SerializeField] private GameObject[] singingFlowerPrefabs;

        [Header("Probability Settings")]
        [SerializeField] private float[] normalFlowerProbabilities = { 14f, 14f, 14f, 14f, 14f, 14f }; // 各14%
        [SerializeField] private float[] specialFlowerProbabilities = { 6f, 6f, 6f }; // 各6%
        [SerializeField] private float[] singingFlowerProbabilities = { 2f }; // 歌う花の初期確率（低めに設定）

        [Header("References")]
        [SerializeField] private FlowerBloomingSFXPlayer sfxPlayer;

        [Header("Animation Settings")]
        [SerializeField] private float fadeOutDuration = 0.7f;

        // Flowerのplanterに対する初期位置のy軸オフセット
        [SerializeField] private float flowerOffsetY = 1.0f;
        #endregion

        #region Private Fields
        private Dictionary<GameObject, bool> planterAvailability = new Dictionary<GameObject, bool>();
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
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
        public void OnPlanterTapped(GameObject planter)
        {
            if (planterAvailability.ContainsKey(planter) && !planterAvailability[planter])
            {
                return; // 既に花が咲いている場合は何もしない
            }

            planterAvailability[planter] = false;

            FlowerSelection flowerSelection = SelectRandomFlower();
            if (flowerSelection.Prefab != null)
            {
                GameObject flower = Instantiate(flowerSelection.Prefab, planter.transform.position, Quaternion.identity);
                flower.transform.SetParent(planter.transform);

                ConfigureFlower(flower, flowerSelection.Type);

                flower.transform.localPosition = new Vector3(0, flowerOffsetY, 0);

                SpriteRenderer flowerRenderer = flower.GetComponent<SpriteRenderer>();
                if (flowerRenderer != null)
                {
                    flowerRenderer.sortingOrder = 10;
                    Debug.Log($"花のSorting Orderを設定: {flowerRenderer.sortingOrder}");
                }

                StartCoroutine(FadeOutFlower(flower, planter, flowerSelection.Type));
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
        public void RegisterPlanter(GameObject planter)
        {
            if (!planterAvailability.ContainsKey(planter))
            {
                planterAvailability.Add(planter, true);
            }
        }
        #endregion

        #region Private Methods
        private FlowerSelection SelectRandomFlower()
        {
            List<FlowerSelection> candidates = new List<FlowerSelection>();
            List<float> probabilities = new List<float>();

            AddFlowerCandidates(normalFlowerPrefabs, normalFlowerProbabilities, FlowerType.Normal, candidates, probabilities);
            AddFlowerCandidates(specialFlowerPrefabs, specialFlowerProbabilities, FlowerType.Special, candidates, probabilities);
            AddFlowerCandidates(singingFlowerPrefabs, singingFlowerProbabilities, FlowerType.Singing, candidates, probabilities);

            if (candidates.Count == 0)
            {
                return new FlowerSelection(null, FlowerType.Normal);
            }

            float totalProbability = 0f;
            for (int i = 0; i < probabilities.Count; i++)
            {
                totalProbability += probabilities[i];
            }

            if (totalProbability <= 0f)
            {
                return candidates[0];
            }

            float randomValue = Random.Range(0f, totalProbability);
            float currentProbability = 0f;

            for (int i = 0; i < candidates.Count; i++)
            {
                currentProbability += probabilities[i];
                if (randomValue <= currentProbability)
                {
                    return candidates[i];
                }
            }

            return candidates[0];
        }

        private void AddFlowerCandidates(
            GameObject[] prefabs,
            float[] probabilities,
            FlowerType type,
            List<FlowerSelection> candidateList,
            List<float> probabilityList)
        {
            if (prefabs == null || probabilities == null) return;

            int count = Mathf.Min(prefabs.Length, probabilities.Length);
            for (int i = 0; i < count; i++)
            {
                if (prefabs[i] == null) continue;
                float probability = probabilities[i];
                if (probability <= 0f) continue;

                candidateList.Add(new FlowerSelection(prefabs[i], type));
                probabilityList.Add(probability);
            }
        }

        private void PlayFlowerSFX(FlowerType flowerType, string flowerName)
        {
            if (sfxPlayer == null) return;

            switch (flowerType)
            {
                case FlowerType.Singing:
                    sfxPlayer.PlaySinging();
                    break;
                case FlowerType.Special:
                    sfxPlayer.PlaySpecial();
                    break;
                default:
                    // 名前にSpecialが含まれている場合の後方互換も担保
                    if (flowerName.Contains("Special"))
                    {
                        sfxPlayer.PlaySpecial();
                    }
                    else
                    {
                        sfxPlayer.PlayNormal();
                    }
                    break;
            }
        }

        private void ConfigureFlower(GameObject flowerObject, FlowerType flowerType)
        {
            if (flowerObject == null) return;

            Flower flower = flowerObject.GetComponent<Flower>();
            if (flower == null) return;

            flower.SetSpecial(flowerType == FlowerType.Special);
            flower.SetSinging(flowerType == FlowerType.Singing);
        }

        private void TriggerSingingNotes(FlowerType flowerType, Flower flowerComponent)
        {
            if (flowerType != FlowerType.Singing) return;
            if (flowerComponent == null) return;

            flowerComponent.PlayNoteEffect();
        }

        private IEnumerator FadeOutFlower(GameObject flower, GameObject planter, FlowerType flowerType)
        {
            Animator animator = flower.GetComponent<Animator>();
            float animationLength = 1.0f;
            string flowerName = flower.name;
            Flower flowerComponent = flower.GetComponent<Flower>();
            
            if (animator != null)
            {
                AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfo.Length > 0)
                {
                    animationLength = clipInfo[0].clip.length;
                    Debug.Log($"アニメーション長: {animationLength}秒");
                    
                    float waitTime = animationLength * 0.9f;
                    yield return new WaitForSeconds(waitTime);
                    
                    PlayFlowerSFX(flowerType, flowerName);
                    TriggerSingingNotes(flowerType, flowerComponent);
                    Debug.Log($"花が咲くタイミングで効果音を再生: {flowerName}");
                    
                    yield return new WaitForSeconds(animationLength - waitTime);
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                    PlayFlowerSFX(flowerType, flowerName);
                    TriggerSingingNotes(flowerType, flowerComponent);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
                PlayFlowerSFX(flowerType, flowerName);
                TriggerSingingNotes(flowerType, flowerComponent);
            }

            yield return new WaitForSeconds(0.5f);

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

            Destroy(flower);
            planterAvailability[planter] = true;
        }
        #endregion
    }
}
