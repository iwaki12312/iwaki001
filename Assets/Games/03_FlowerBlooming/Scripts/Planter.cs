using UnityEngine;

namespace Minigames.FlowerBlooming
{
    /// <summary>
    /// プランターの機能を提供するクラス
    /// </summary>
    public class Planter : MonoBehaviour
    {
        #region SerializeFields
        [Header("Settings")]
        [SerializeField] private bool isInteractable = true;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // BoxCollider2Dがなければ追加
            if (GetComponent<BoxCollider2D>() == null)
            {
                BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
                
                // スプライトのサイズに合わせる
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    collider.size = spriteRenderer.sprite.bounds.size;
                }
            }

            // GameManagerに登録
            if (FlowerBloomingGameManager.Instance != null)
            {
                FlowerBloomingGameManager.Instance.RegisterPlanter(gameObject);
            }
            else
            {
                Debug.LogError("FlowerBloomingGameManagerが見つかりません");
            }
        }

        private void OnMouseDown()
        {
            if (!isInteractable) return;

            // GameManagerに通知
            if (FlowerBloomingGameManager.Instance != null)
            {
                FlowerBloomingGameManager.Instance.OnPlanterTapped(gameObject);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// プランターの操作可能状態を設定する
        /// </summary>
        /// <param name="interactable">操作可能かどうか</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
        }
        #endregion
    }
}
