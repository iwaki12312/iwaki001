# PianoAndViolin Input System Update

## 概要
PianoAndViolinゲームの入力システムを古いInput Managerから新しいInput System Packageに対応するように修正しました。

## 変更内容

### 1. 追加されたusing文
```csharp
using UnityEngine.InputSystem;
```

### 2. 新しい入力変数
```csharp
/// <summary>新しい入力システム用のInputAction</summary>
private InputAction pointerAction;
/// <summary>ポインター位置取得用のInputAction</summary>
private InputAction pointerPositionAction;
```

### 3. 入力システムの初期化
```csharp
// Awake()メソッド内
pointerAction = new InputAction("pointer", binding: "<Pointer>/press");
pointerPositionAction = new InputAction("pointerPosition", binding: "<Pointer>/position");

// OnEnable()とOnDisable()メソッドを追加
void OnEnable()
{
    pointerAction.Enable();
    pointerPositionAction.Enable();
}

void OnDisable()
{
    pointerAction.Disable();
    pointerPositionAction.Disable();
}
```

### 4. 統一された入力処理
- 古いInput.touchesとInput.GetMouseButtonDown()を削除
- 新しいInput Systemを使用した統一的な入力処理に変更
- タッチとマウスの両方に対応

```csharp
void Update()
{
    // 新しい入力システムを使用したタップ・クリック判定
    if (pointerAction.WasPressedThisFrame())
    {
        Vector2 screenPos = pointerPositionAction.ReadValue<Vector2>();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        
        HandleInput(worldPos);
    }
}
```

## 利点

1. **タブレット対応の改善**: 新しいInput Systemはタブレットのタッチ入力をより適切に処理します
2. **統一された入力処理**: マウスとタッチの処理が統一され、コードが簡潔になりました
3. **将来性**: Unity推奨の新しいInput Systemを使用することで、将来のUnityバージョンとの互換性が向上します
4. **デバッグの容易さ**: 入力処理が一箇所に集約されているため、問題の特定が容易です

## テスト方法

### PCでのテスト
1. Unityエディターでゲームを実行
2. マウスクリックでピアノ、バイオリン、音符をクリック
3. 音楽が正常に再生されることを確認

### タブレットでのテスト
1. プロジェクトをAndroid/iOSビルド
2. タブレットにインストール
3. タップでピアノ、バイオリン、音符をタップ
4. 音楽が正常に再生されることを確認

## 注意事項

- この変更により、古いInput Managerとの混在による問題が解決されます
- プロジェクト設定でInput System Packageが有効になっている必要があります（既に設定済み）
- 他のゲームでも同様の問題がある場合は、同じ手法で修正できます

## トラブルシューティング

もしタッチ入力がまだ動作しない場合は、以下を確認してください：

1. **Input System Packageのバージョン**: 最新版を使用しているか
2. **プロジェクト設定**: Player Settings > Configuration > Active Input Handling が "Input System Package (New)" または "Both" に設定されているか
3. **コライダーの設定**: ピアノ、バイオリン、音符のCollider2Dが正しく設定されているか
4. **カメラの設定**: Camera.mainが正しく設定されているか

## 追加修正: シーン遷移時の誤入力防止

### 問題
メニューから06_PianoAndViolinゲームに遷移した際、クリック判定が残ってしまい、遷移直後にキャラクターが自動的にクリックされた状態でゲームが始まってしまう問題が発生していました。

### 解決策
04_Cookゲームで実装されていた手法と同様に、シーン読み込み直後の一定時間（0.5秒）は入力を無視する仕組みを実装しました。

```csharp
/// <summary>シーン読み込み時刻を記録（タップ遅延処理用）</summary>
private float sceneLoadTime;

void Start()
{
    // シーン読み込み時刻を記録
    sceneLoadTime = Time.time;
}

private void HandleInput(Vector2 worldPos)
{
    // シーン読み込み直後の0.5秒間はタップを無視
    if (Time.time - sceneLoadTime < 0.5f)
    {
        Debug.Log("PianoAndViolin: シーン読み込み直後のタップを無視しました");
        return;
    }
    
    // 以下、通常の入力処理...
}
```

### 効果
- メニューからの遷移時に発生していた誤入力が防止されます
- ゲーム開始時に意図しない音楽再生が発生しなくなります
- ユーザーが意図的にタップするまで待機状態を維持します

## 修正日時
- 初回修正: 2025年9月7日 14:57 (Input System対応)
- 追加修正: 2025年9月7日 15:03 (シーン遷移時誤入力防止)
