using UnityEngine;

namespace Minigames.FlowerBlooming
{
    /// <summary>プランター（鉢）のタップ検出</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Planter : MonoBehaviour
    {
        /* ------------ Serialized ------------- */
        [Header("Settings")]
        [SerializeField] private bool isInteractable = true;

        /* ------------ キャッシュ ------------- */
        Collider2D  myCol;
        Camera      mainCam;

        /* ============================================= */

        void Start()
        {
            /* ―― Collider が無ければ自動追加 ――――――――――――――― */
            myCol = GetComponent<BoxCollider2D>();
            if (!myCol)
            {
                var col = gameObject.AddComponent<BoxCollider2D>();
                // Sprite サイズに合わせる
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr && sr.sprite)
                    col.size = sr.sprite.bounds.size;
                myCol = col;
            }

            /* ―― GameManager へ登録 ――――――――――――――― */
            if (FlowerBloomingGameManager.Instance)
                FlowerBloomingGameManager.Instance.RegisterPlanter(gameObject);
            else
                Debug.LogError("[Planter] FlowerBloomingGameManager が見つかりません");

            mainCam = Camera.main;
            if (!mainCam) Debug.LogWarning("[Planter] MainCamera が見つかりません。タグを確認してください");
        }

        /* ===============  多指対応  =============== */
        void Update()
        {
            if (!isInteractable || !mainCam || !myCol) return;

            /* ❶ すべてのタッチを調べる（実機） */
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && HitPlanter(t.position))
                    NotifyTapped();
            }

#if UNITY_EDITOR      // ❷ エディタ用：左クリックでテスト
            if (Input.GetMouseButtonDown(0) && HitPlanter(Input.mousePosition))
                NotifyTapped();
#endif
        }

        /* ===============  判定 & 通知  =============== */
        bool HitPlanter(Vector2 screenPos)
        {
            Vector2 world = mainCam.ScreenToWorldPoint(screenPos);
            return myCol.OverlapPoint(world);
        }

        void NotifyTapped()
        {
            if (FlowerBloomingGameManager.Instance)
                FlowerBloomingGameManager.Instance.OnPlanterTapped(gameObject);
        }

        /* ===============  Public  =============== */
        /// <summary>外部から操作可否を切り替え</summary>
        public void SetInteractable(bool interactable) => isInteractable = interactable;
    }
}
