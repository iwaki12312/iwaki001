using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// SnowmanBuilderゲームのメインコントローラー
/// 地面をタップして雪だるま制作を開始する。マルチタッチ対応。
/// </summary>
public class SnowmanBuilderController : MonoBehaviour
{
    [Header("スプライト参照")]
    [SerializeField] private Sprite snowballSprite;
    [SerializeField] private Sprite[] decorationSets;       // 通常完成バリエーション
    [SerializeField] private Sprite[] rareDecorationSets;   // レア完成バリエーション

    [Header("スポーン設定")]
    [SerializeField] private float groundMinY = -4f;
    [SerializeField] private float groundMaxY = -1f;
    [SerializeField] private float groundMinX = -7f;
    [SerializeField] private float groundMaxX = 7f;
    [SerializeField] private int maxSnowmen = 5;

    [Header("レア確率")]
    [SerializeField, Range(0f, 1f)] private float rareChance = 0.1f;

    [Header("雪パーティクル")]
    [SerializeField] private bool enableSnowfall = true;

    private Camera mainCamera;
    private List<SnowmanController> activeSnowmen = new List<SnowmanController>();
    private GameObject snowfallParticle;

    void Start()
    {
        mainCamera = Camera.main;
        if (enableSnowfall)
        {
            CreateSnowfallParticle();
        }
    }

    /// <summary>
    /// スプライト参照をセット（Initializerから呼ばれる）
    /// </summary>
    public void SetSprites(Sprite snowball, Sprite[] decoSets, Sprite[] rareDecoSets)
    {
        snowballSprite = snowball;
        decorationSets = decoSets;
        rareDecorationSets = rareDecoSets;
    }

    void Update()
    {
        HandleTouch();
    }

    /// <summary>
    /// マルチタッチ対応: 「何もない地面部分」をタップしたら新しい雪だるまを開始
    /// 既存の雪だるまへのタップはSnowmanControllerが処理する
    /// </summary>
    private void HandleTouch()
    {
        // タッチスクリーン
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            foreach (var touch in touchscreen.touches)
            {
                if (!touch.press.wasPressedThisFrame) continue;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(touch.position.ReadValue());
                worldPos.z = 0;
                TrySpawnSnowman(worldPos);
            }
        }

        // マウス（エディタ用）
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            worldPos.z = 0;
            TrySpawnSnowman(worldPos);
        }
    }

    /// <summary>
    /// 座標が地面範囲内で、既存の雪だるまと重なっていなければスポーン
    /// </summary>
    private void TrySpawnSnowman(Vector3 worldPos)
    {
        // 地面範囲チェック
        if (worldPos.y < groundMinY || worldPos.y > groundMaxY) return;
        if (worldPos.x < groundMinX || worldPos.x > groundMaxX) return;

        // 既存の雪だるまと重なっていないかチェック
        // （SnowmanControllerのコライダーに当たっていたら、そっちがタップ処理するのでここではスポーンしない）
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null) return;

        // 最大数チェック
        CleanupDestroyedSnowmen();
        if (activeSnowmen.Count >= maxSnowmen) return;

        SpawnSnowman(worldPos);
    }

    private void SpawnSnowman(Vector3 position)
    {
        GameObject snowmanObj = new GameObject("Snowman");
        SnowmanController controller = snowmanObj.AddComponent<SnowmanController>();

        controller.Initialize(snowballSprite, decorationSets, rareDecorationSets, rareChance, position);

        controller.OnSnowmanDestroyed = () =>
        {
            activeSnowmen.Remove(controller);
        };

        activeSnowmen.Add(controller);
    }

    private void CleanupDestroyedSnowmen()
    {
        activeSnowmen.RemoveAll(s => s == null);
    }

    /// <summary>
    /// パーティクルシステムで雪を降らせる（背景演出）
    /// </summary>
    private void CreateSnowfallParticle()
    {
        snowfallParticle = new GameObject("SnowfallParticle");
        snowfallParticle.transform.position = new Vector3(0, 6f, 0);

        var ps = snowfallParticle.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(1f, 1f, 1f, 0.7f);
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.05f;

        var emission = ps.emission;
        emission.rateOverTime = 30f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 0.1f, 1f);

        // 左右にゆらゆら揺れる動き
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;

        // レンダラー設定
        var renderer = snowfallParticle.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 5;

        // デフォルトパーティクルマテリアルを使用
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = Color.white;
    }
}
