using UnityEngine;
using System.IO;
using System.Collections;

/// <summary>
/// デバッグ用：Play mode開始後5秒でスクリーンショットを自動撮影
/// </summary>
public class AnimalVoiceDebugCapture : MonoBehaviour
{
    private const string SCREENSHOT_PATH = "C:/dev/iwaki001/screenshot_animalvoice.png";
    private const float CAPTURE_DELAY = 5f;

    private void Start()
    {
        // 既存のスクリーンショットを削除
        if (File.Exists(SCREENSHOT_PATH))
        {
            File.Delete(SCREENSHOT_PATH);
        }

        Debug.Log($"[AnimalVoiceDebugCapture] {CAPTURE_DELAY}秒後にスクリーンショットを撮影します: {SCREENSHOT_PATH}");

        // 5秒後にスクリーンショットを撮影
        StartCoroutine(CaptureScreenshotAfterDelay());
    }

    private IEnumerator CaptureScreenshotAfterDelay()
    {
        yield return new WaitForSeconds(CAPTURE_DELAY);

        Debug.Log("[AnimalVoiceDebugCapture] スクリーンショットを撮影中...");
        ScreenCapture.CaptureScreenshot(SCREENSHOT_PATH);

        Debug.Log($"[AnimalVoiceDebugCapture] スクリーンショット保存完了: {SCREENSHOT_PATH}");

        // ログに状態を出力
        LogGameState();
    }

    private void LogGameState()
    {
        Debug.Log("=== AnimalVoice ゲーム状態 ===");

        // シーン内のオブジェクト数を確認
        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        Debug.Log($"シーン内のGameObject数: {allObjects.Length}");

        // AnimalControllerを検索
        var animals = FindObjectsByType<AnimalController>(FindObjectsSortMode.None);
        Debug.Log($"AnimalController数: {animals.Length}");

        foreach (var animal in animals)
        {
            var sr = animal.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"  動物: {animal.name}, Sprite: {(sr.sprite != null ? sr.sprite.name : "null")}, " +
                         $"色: {sr.color}, 有効: {sr.enabled}, ソートレイヤー: {sr.sortingLayerName}, " +
                         $"ソート順: {sr.sortingOrder}");
            }
        }

        // 背景を確認
        var backgrounds = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        Debug.Log($"SpriteRenderer総数: {backgrounds.Length}");

        // カメラ確認
        var camera = Camera.main;
        if (camera != null)
        {
            Debug.Log($"カメラ位置: {camera.transform.position}, サイズ: {camera.orthographicSize}");
        }
    }
}
