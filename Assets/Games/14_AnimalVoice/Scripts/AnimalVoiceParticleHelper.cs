using UnityEngine;

/// <summary>
/// 動物タップ時のパーティクルエフェクトをコードで生成するヘルパー。
/// ソフトな円形グローテクスチャを動的生成し、Additiveブレンドで
/// 光り輝くカラフルなパーティクルを実現する。
/// </summary>
public static class AnimalVoiceParticleHelper
{
    // === テクスチャキャッシュ（1回だけ生成） ===
    private static Texture2D _softCircleTex;
    private static Texture2D _starTex;
    private static Material _additiveMat;
    private static Material _softAdditiveMat;

    // 通常タップ用カラー（明るく鮮やか）
    private static readonly Color[] normalColors = new Color[]
    {
        new Color(1.0f, 0.45f, 0.6f),  // ピンク
        new Color(1.0f, 0.82f, 0.2f),  // イエロー
        new Color(0.4f, 0.82f, 1.0f),  // スカイブルー
        new Color(0.45f, 1.0f, 0.55f), // ライトグリーン
        new Color(1.0f, 0.55f, 0.2f),  // オレンジ
        new Color(0.72f, 0.45f, 1.0f), // ラベンダー
    };

    // レア用カラー
    private static readonly Color[] rareColors = new Color[]
    {
        new Color(1.0f, 0.88f, 0.15f), // ゴールド
        new Color(1.0f, 0.35f, 0.55f), // ホットピンク
        new Color(0.35f, 0.78f, 1.0f), // スカイブルー
        new Color(0.5f, 1.0f, 0.35f),  // ライム
        new Color(1.0f, 0.55f, 0.15f), // オレンジ
        new Color(0.65f, 0.35f, 1.0f), // パープル
        new Color(1.0f, 1.0f, 0.5f),   // レモン
    };

    // ──────────────────────────────────────
    //  テクスチャ & マテリアル 生成（遅延初期化）
    // ──────────────────────────────────────

    /// <summary>中心が明るいソフト円形テクスチャ（64x64）</summary>
    private static Texture2D GetSoftCircleTexture()
    {
        if (_softCircleTex != null) return _softCircleTex;

        const int size = 64;
        _softCircleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // スムーズなフォールオフ
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha; // 二乗で中心を明るく、外側を柔らかく
                _softCircleTex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        _softCircleTex.Apply();
        _softCircleTex.wrapMode = TextureWrapMode.Clamp;
        return _softCircleTex;
    }

    /// <summary>4頂点の星型テクスチャ（64x64）</summary>
    private static Texture2D GetStarTexture()
    {
        if (_starTex != null) return _starTex;

        const int size = 64;
        _starTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center) / center;
                float dy = Mathf.Abs(y - center) / center;
                // 十字＋円を合成してキラッと光る星型に
                float cross = Mathf.Max(
                    Mathf.Clamp01(1f - dx * 4f) * Mathf.Clamp01(1f - dy),
                    Mathf.Clamp01(1f - dy * 4f) * Mathf.Clamp01(1f - dx)
                );
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float circle = Mathf.Clamp01(1f - dist * 1.8f);
                circle = circle * circle;
                float alpha = Mathf.Clamp01(cross + circle * 0.6f);
                _starTex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        _starTex.Apply();
        _starTex.wrapMode = TextureWrapMode.Clamp;
        return _starTex;
    }

    /// <summary>Additive ブレンドマテリアル（発光感）</summary>
    private static Material GetAdditiveMaterial(Texture2D tex)
    {
        // テクスチャごとにキャッシュ
        if (tex == GetSoftCircleTexture())
        {
            if (_additiveMat != null) return _additiveMat;
            _additiveMat = CreateAdditiveMaterial(tex);
            return _additiveMat;
        }
        if (tex == GetStarTexture())
        {
            if (_softAdditiveMat != null) return _softAdditiveMat;
            _softAdditiveMat = CreateAdditiveMaterial(tex);
            return _softAdditiveMat;
        }
        return CreateAdditiveMaterial(tex);
    }

    private static Material CreateAdditiveMaterial(Texture2D tex)
    {
        // Particles/Standard Unlit がURP環境でも安定して光る
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.mainTexture = tex;

        // Additive blending
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 1);   // Additive
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3100;
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        return mat;
    }

    // ──────────────────────────────────────
    //  ヘルパー
    // ──────────────────────────────────────

    /// <summary>フェードアウトするアルファグラデーション生成</summary>
    private static Gradient MakeFadeGradient(float holdUntil = 0.5f)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, holdUntil),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return g;
    }

    /// <summary>Rendererにマテリアルとソート順を設定</summary>
    private static void SetupRenderer(GameObject obj, Material mat, int sortOrder)
    {
        var r = obj.GetComponent<ParticleSystemRenderer>();
        r.material = mat;
        r.sortingOrder = sortOrder;
        r.minParticleSize = 0f;
        r.maxParticleSize = 2f;
    }

    // ──────────────────────────────────────
    //  通常タップ パーティクル
    // ──────────────────────────────────────

    /// <summary>
    /// 通常動物タップ時のパーティクル:
    /// ① ソフトグロー粒子のバースト（カラフル、ふわっと広がって消える）
    /// ② 小さなキラキラ星がパラパラと舞い落ちる
    /// </summary>
    public static void SpawnNormalTapParticle(Vector3 position)
    {
        Material glowMat = GetAdditiveMaterial(GetSoftCircleTexture());
        Material starMat = GetAdditiveMaterial(GetStarTexture());

        GameObject root = new GameObject("NormalTapParticle");
        root.transform.position = position;

        // ── ① メインのグローバースト ──
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.4f;
        main.loop = false;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.gravityModifier = -0.3f; // ふわっと浮き上がる
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // ランダムに3色を選ぶ
        Color c1 = normalColors[Random.Range(0, normalColors.Length)];
        Color c2 = normalColors[Random.Range(0, normalColors.Length)];
        main.startColor = new ParticleSystem.MinMaxGradient(c1, c2);

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 10, 14)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        // サイズ: ポンと膨らんで縮む
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sc = new AnimationCurve();
        sc.AddKey(new Keyframe(0f, 0.2f));
        sc.AddKey(new Keyframe(0.12f, 1.0f));
        sc.AddKey(new Keyframe(0.5f, 0.6f));
        sc.AddKey(new Keyframe(1f, 0f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // フェードアウト
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(0.4f));

        SetupRenderer(root, glowMat, 60);

        // ── ② キラキラ星 ──
        GameObject starObj = new GameObject("Sparkles");
        starObj.transform.SetParent(root.transform);
        starObj.transform.localPosition = Vector3.zero;

        ParticleSystem starPs = starObj.AddComponent<ParticleSystem>();
        var starMain = starPs.main;
        starMain.duration = 1.2f;
        starMain.loop = false;
        starMain.playOnAwake = true;
        starMain.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        starMain.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
        starMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        starMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        starMain.gravityModifier = 0.4f; // ちょっと落ちる
        starMain.maxParticles = 12;
        starMain.simulationSpace = ParticleSystemSimulationSpace.World;

        // 白〜淡い黄色でキラッと
        starMain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 0.85f),
            new Color(1f, 0.95f, 0.6f)
        );

        var starEmission = starPs.emission;
        starEmission.rateOverTime = 0;
        starEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.05f, 6, 10)
        });

        var starShape = starPs.shape;
        starShape.shapeType = ParticleSystemShapeType.Sphere;
        starShape.radius = 0.3f;

        // 回転アニメーション
        var starRot = starPs.rotationOverLifetime;
        starRot.enabled = true;
        starRot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

        // サイズ: キラッと光ってすーっと消える
        var starSol = starPs.sizeOverLifetime;
        starSol.enabled = true;
        AnimationCurve starSC = new AnimationCurve();
        starSC.AddKey(new Keyframe(0f, 0.3f));
        starSC.AddKey(new Keyframe(0.15f, 1.0f));
        starSC.AddKey(new Keyframe(0.4f, 0.5f));
        starSC.AddKey(new Keyframe(1f, 0f));
        starSol.size = new ParticleSystem.MinMaxCurve(1f, starSC);

        // フェードアウト
        var starCol = starPs.colorOverLifetime;
        starCol.enabled = true;
        starCol.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(0.3f));

        SetupRenderer(starObj, starMat, 62);

        Object.Destroy(root, 2f);
    }

    // ──────────────────────────────────────
    //  レアタップ パーティクル
    // ──────────────────────────────────────

    /// <summary>
    /// レア動物タップ時の豪華パーティクル:
    /// ① レインボーグローの大きなバースト
    /// ② ゴールドのリング状スパーク
    /// ③ キラキラ星の雨
    /// </summary>
    public static void SpawnRareTapParticle(Vector3 position)
    {
        Material glowMat = GetAdditiveMaterial(GetSoftCircleTexture());
        Material starMat = GetAdditiveMaterial(GetStarTexture());

        GameObject root = new GameObject("RareTapParticle");
        root.transform.position = position;

        // ── ① メインのレインボーバースト ──
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.8f;
        main.loop = false;
        main.playOnAwake = true;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.0f, 6.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
        main.gravityModifier = -0.15f;
        main.maxParticles = 40;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // レインボーグラデーション
        Gradient startGrad = new Gradient();
        startGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(rareColors[0], 0f),
                new GradientColorKey(rareColors[1], 0.17f),
                new GradientColorKey(rareColors[2], 0.33f),
                new GradientColorKey(rareColors[3], 0.5f),
                new GradientColorKey(rareColors[4], 0.67f),
                new GradientColorKey(rareColors[5], 0.83f),
                new GradientColorKey(rareColors[6], 1f),
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(startGrad);

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 16, 22),
            new ParticleSystem.Burst(0.08f, 8, 12)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        // サイズ
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sc = new AnimationCurve();
        sc.AddKey(new Keyframe(0f, 0.2f));
        sc.AddKey(new Keyframe(0.1f, 1.0f));
        sc.AddKey(new Keyframe(0.4f, 0.65f));
        sc.AddKey(new Keyframe(1f, 0f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // フェードアウト
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(0.45f));

        // ノイズ（ゆらゆらキラキラ感）
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 2.5f;
        noise.scrollSpeed = 1.5f;

        SetupRenderer(root, glowMat, 65);

        // ── ② ゴールドのリングスパーク ──
        GameObject ringObj = new GameObject("GoldRing");
        ringObj.transform.SetParent(root.transform);
        ringObj.transform.localPosition = Vector3.zero;

        ParticleSystem ringPs = ringObj.AddComponent<ParticleSystem>();
        var ringMain = ringPs.main;
        ringMain.duration = 0.8f;
        ringMain.loop = false;
        ringMain.playOnAwake = true;
        ringMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        ringMain.startSpeed = new ParticleSystem.MinMaxCurve(5f, 8f);
        ringMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        ringMain.gravityModifier = 0f;
        ringMain.maxParticles = 28;
        ringMain.simulationSpace = ParticleSystemSimulationSpace.World;

        ringMain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.92f, 0.4f),
            new Color(1f, 0.75f, 0.25f)
        );

        var ringEmission = ringPs.emission;
        ringEmission.rateOverTime = 0;
        ringEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.03f, 16, 24)
        });

        var ringShape = ringPs.shape;
        ringShape.shapeType = ParticleSystemShapeType.Circle;
        ringShape.radius = 0.15f;
        ringShape.radiusThickness = 0f;

        // サイズ
        var ringSol = ringPs.sizeOverLifetime;
        ringSol.enabled = true;
        AnimationCurve rsc = new AnimationCurve();
        rsc.AddKey(new Keyframe(0f, 1f));
        rsc.AddKey(new Keyframe(0.3f, 0.6f));
        rsc.AddKey(new Keyframe(1f, 0f));
        ringSol.size = new ParticleSystem.MinMaxCurve(1f, rsc);

        // フェードアウト
        var ringCol = ringPs.colorOverLifetime;
        ringCol.enabled = true;
        ringCol.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(0.2f));

        SetupRenderer(ringObj, glowMat, 64);

        // ── ③ キラキラ星の雨 ──
        GameObject starObj = new GameObject("StarShower");
        starObj.transform.SetParent(root.transform);
        starObj.transform.localPosition = Vector3.zero;

        ParticleSystem starPs = starObj.AddComponent<ParticleSystem>();
        var starMain = starPs.main;
        starMain.duration = 1.5f;
        starMain.loop = false;
        starMain.playOnAwake = true;
        starMain.startDelay = 0.05f;
        starMain.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        starMain.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        starMain.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        starMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        starMain.gravityModifier = 0.5f; // 星が舞い落ちる
        starMain.maxParticles = 18;
        starMain.simulationSpace = ParticleSystemSimulationSpace.World;

        starMain.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 0.8f),
            new Color(1f, 0.9f, 0.5f)
        );

        var starEmission = starPs.emission;
        starEmission.rateOverTime = 0;
        starEmission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.05f, 8, 14),
            new ParticleSystem.Burst(0.2f, 4, 8)
        });

        var starShape = starPs.shape;
        starShape.shapeType = ParticleSystemShapeType.Sphere;
        starShape.radius = 0.5f;

        // 回転
        var starRot = starPs.rotationOverLifetime;
        starRot.enabled = true;
        starRot.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

        // サイズ
        var starSol = starPs.sizeOverLifetime;
        starSol.enabled = true;
        AnimationCurve ssc = new AnimationCurve();
        ssc.AddKey(new Keyframe(0f, 0.2f));
        ssc.AddKey(new Keyframe(0.12f, 1.0f));
        ssc.AddKey(new Keyframe(0.5f, 0.4f));
        ssc.AddKey(new Keyframe(1f, 0f));
        starSol.size = new ParticleSystem.MinMaxCurve(1f, ssc);

        // フェードアウト
        var starCol = starPs.colorOverLifetime;
        starCol.enabled = true;
        starCol.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(0.35f));

        SetupRenderer(starObj, starMat, 66);

        Object.Destroy(root, 2.5f);
    }
}
