using UnityEngine;
using System.Collections;

/// <summary>
/// 少年に向かって矢印を表示するクラス
/// </summary>
public class ArrowController : MonoBehaviour
{
    [Header("動きの設定")]
    [SerializeField] private float moveDistance = 0.3f;  // 動く距離
    [SerializeField] private float moveSpeed = 2.0f;     // 動くスピード
    [SerializeField] private float rotateAngle = 10.0f;  // 回転角度
    [SerializeField] private float rotateSpeed = 3.0f;   // 回転スピード

    private Vector3 startPosition;
    private Transform boyTransform;

    void Start()
    {
        // 初期位置を保存
        startPosition = transform.position;

        // 少年オブジェクトを検索
        GameObject boy = GameObject.FindGameObjectWithTag("Boy");
        if (boy != null)
        {
            boyTransform = boy.transform;

            // 少年の左上に配置
            PositionArrowAboveBoy();
        }
        else
        {
            Debug.LogError("少年オブジェクトが見つかりません");
        }

        // アニメーション開始
        StartCoroutine(AnimateArrow());
    }

    // 少年の左上に矢印を配置
    private void PositionArrowAboveBoy()
    {
        if (boyTransform != null)
        {
            // 少年の左上に配置（少し離す）
            Vector3 newPosition = boyTransform.position + new Vector3(-2.0f, 3.5f, 0);
            transform.position = newPosition;
            startPosition = newPosition;

            // 手動で角度を指定（例：45度）
            float manualAngle = 45f; // ここの値を変更することで矢印の向きを調整できます
            transform.rotation = Quaternion.Euler(0, 0, manualAngle);
        }
    }

    // 矢印をアニメーションさせるコルーチン
    private IEnumerator AnimateArrow()
    {
        float timePassed = 0;

        while (true)
        {
            timePassed += Time.deltaTime;

            // 前後に動く
            float moveOffset = Mathf.Sin(timePassed * moveSpeed) * moveDistance;
            transform.position = startPosition + transform.right * moveOffset;

            // 少し回転する
            float rotateOffset = Mathf.Sin(timePassed * rotateSpeed) * rotateAngle;
            transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + rotateOffset * Time.deltaTime);

            yield return null;
        }
    }

    // 矢印を非表示にする
    public void HideArrow()
    {
        gameObject.SetActive(false);
    }
}
