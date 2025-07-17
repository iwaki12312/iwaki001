# メニュー画面の設定手順

## 概要
このドキュメントでは、メニュー画面からバブルゲームとモグラたたきゲームに遷移するための設定手順を説明します。

## 手順

1. **MenuControllerの追加**
   - Menu.unityシーンを開く
   - Hierarchy上で右クリック → Create Empty を選択
   - 作成したGameObjectの名前を "MenuController" に変更
   - Inspector上で "Add Component" をクリック
   - "MenuController" を検索して追加

2. **Bubbleオブジェクトの設定**
   - Bubbleオブジェクトが存在することを確認
   - 存在しない場合は、Hierarchy上で右クリック → 2D Object → Sprite を選択
   - 作成したSpriteの名前を "Bubble" に変更
   - Inspector上でSpriteを設定（バブルゲームのアイコン画像）
   - 位置を適切に調整（例：X: -2, Y: 0, Z: 0）

3. **Moleオブジェクトの設定**
   - Moleオブジェクトが存在することを確認
   - 存在しない場合は、Hierarchy上で右クリック → 2D Object → Sprite を選択
   - 作成したSpriteの名前を "Mole" に変更
   - Inspector上でSpriteを設定（モグラたたきゲームのアイコン画像）
   - 位置を適切に調整（例：X: 2, Y: 0, Z: 0）

4. **EventSystemの確認**
   - Hierarchy上にEventSystemが存在することを確認
   - 存在しない場合は、Hierarchy上で右クリック → UI → Event System を選択

5. **動作確認**
   - Play ボタンを押す
   - Bubbleアイコンをクリックすると、バブルゲームに遷移することを確認
   - Moleアイコンをクリックすると、モグラたたきゲームに遷移することを確認

## 注意点

- BubbleとMoleオブジェクトには、SpriteRendererコンポーネントが必要です
- MenuControllerスクリプトは、自動的にBubbleとMoleオブジェクトにBoxCollider2DとGameButtonコンポーネントを追加します
- ゲームシーン（BubbleGame.unityとMoleGame.unity）はビルド設定に含まれている必要があります

## カスタマイズ

- BubbleとMoleオブジェクトの位置やサイズを調整することで、レイアウトをカスタマイズできます
- SpriteRendererのColorプロパティを変更することで、アイコンの色を調整できます
- 背景画像を変更したい場合は、BackGroundオブジェクトのSpriteを変更してください
