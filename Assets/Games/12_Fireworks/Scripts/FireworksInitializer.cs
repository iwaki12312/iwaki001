using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Fireworksゲームの自動初期化を行うクラス
/// - なるべくシーン編集なしで起動できるように、必要オブジェクトを動的生成する
/// </summary>
public class FireworksInitializer : MonoBehaviour
{
    [Header("Sprites (任意: 未設定でも動作します)")]
    [SerializeField] private Sprite rocketSprite;
    [Tooltip("爆発用スプライト配列（増減自由）。10枚程度想定。\n0枚/未設定の場合は簡易スプライトで代用します。\nnullが混ざっていても自動的に除外されます。")]
    [SerializeField] private Sprite[] explosionSprites;
    [SerializeField] private Sprite shootingStarSprite;
    [SerializeField] private Sprite backgroundSprite;

    [Header("Audio (任意: 未設定でも動作します)")]
    [SerializeField] private AudioClip rocketLaunchSfx;
    [SerializeField] private AudioClip explosionSfx;
    [SerializeField] private AudioClip shootingStarSfx;
    [SerializeField] private AudioClip starMineStartSfx;

    [Header("Night Sky")]
    [SerializeField] private Color cameraBackground = new Color(0.02f, 0.02f, 0.06f, 1f);

    private void Awake()
    {
#if UNITY_EDITOR
        AutoAssignTemplateAssetsIfNeeded();
#endif
        InitializeGame();
    }

    private void InitializeGame()
    {
        CreateEventSystemIfNeeded();
        ConfigureCamera();
        CreateBackgroundIfNeeded();

        FireworksSFXPlayer sfxPlayer = CreateSFXPlayer();
        CreateManager(sfxPlayer);

        Debug.Log("[Fireworks] 初期化完了");
    }

    private void ConfigureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = cameraBackground;
        cam.orthographic = true;
    }

    private void CreateEventSystemIfNeeded()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
    }

    private void CreateBackgroundIfNeeded()
    {
        if (backgroundSprite == null)
        {
            return;
        }

        if (GameObject.Find("Background") != null)
        {
            return;
        }

        var bg = new GameObject("Background");
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.sortingOrder = -100;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            bg.transform.position = new Vector3(center.x, center.y, 0f);

            // 画面を覆うようにスケール調整（大雑把でOK）
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            Vector2 spriteSize = sr.sprite.bounds.size;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                bg.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);
            }
        }
    }

    private FireworksSFXPlayer CreateSFXPlayer()
    {
        var existing = FindObjectOfType<FireworksSFXPlayer>();
        if (existing != null)
        {
            return existing;
        }

        var obj = new GameObject("FireworksSFXPlayer");
        var player = obj.AddComponent<FireworksSFXPlayer>();
        player.SetClips(rocketLaunchSfx, explosionSfx, shootingStarSfx, starMineStartSfx);
        return player;
    }

    private void CreateManager(FireworksSFXPlayer sfxPlayer)
    {
        var existing = FindObjectOfType<FireworksManager>();
        if (existing != null)
        {
            return;
        }

        var obj = new GameObject("FireworksManager");
        var manager = obj.AddComponent<FireworksManager>();

        manager.SetAssets(
            rocketSprite,
            SanitizeSprites(explosionSprites),
            shootingStarSprite,
            sfxPlayer
        );
    }

#if UNITY_EDITOR
    private void AutoAssignTemplateAssetsIfNeeded()
    {
        // Inspectorで何も設定されていない場合に、テンプレの仮アセットを自動で割り当てる
        // ※ AssetDatabase は Editor 専用。ビルド時は従来のフォールバック（ランタイム生成）で動作。

        const string basePath = "Assets/Games/12_Fireworks/";

        if (backgroundSprite == null)
        {
            backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Sprites/work_bg.png");
        }

        if (rocketSprite == null)
        {
            // 仮スプライトを流用（ロケット用の専用絵が無い場合の最小構成）
            rocketSprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Sprites/work_sprite_a.png");
        }

        if (shootingStarSprite == null)
        {
            shootingStarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Sprites/work_sprite_b.png");
        }

        if (explosionSprites == null || explosionSprites.Length == 0)
        {
            // ひとまず2枚を爆発候補として使う（ユーザーが10枚に増やしやすい）
            var a = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Sprites/work_sprite_a.png");
            var b = AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "Sprites/work_sprite_b.png");
            explosionSprites = new[] { a, b };
        }

        if (rocketLaunchSfx == null)
        {
            rocketLaunchSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "Audios/work_sfx1.mp3");
        }
        if (explosionSfx == null)
        {
            explosionSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "Audios/work_sfx2.mp3");
        }
        if (shootingStarSfx == null)
        {
            shootingStarSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "Audios/work_sfx3.mp3");
        }
        if (starMineStartSfx == null)
        {
            starMineStartSfx = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "Audios/work_sfx4.mp3");
        }

        // null混入は後段でサニタイズされるが、ここでも軽く整える
        explosionSprites = SanitizeSprites(explosionSprites);
    }
#endif

    private static Sprite[] SanitizeSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return sprites;
        }

        int count = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null) count++;
        }

        if (count == sprites.Length)
        {
            return sprites;
        }

        var result = new Sprite[count];
        int idx = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            result[idx++] = sprites[i];
        }

        return result;
    }
}
