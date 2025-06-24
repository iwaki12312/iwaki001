using UnityEngine;
using UnityEditor;

// 円形テクスチャを生成するエディタスクリプト
public class CreateCircleTexture : MonoBehaviour
{
    [MenuItem("Tools/Create Circle Texture")]
    static void CreateTexture()
    {
        // テクスチャサイズ
        int width = 256;
        int height = 256;
        
        // 新しいテクスチャを作成
        Texture2D texture = new Texture2D(width, height);
        
        // 中心座標と半径
        Vector2 center = new Vector2(width / 2, height / 2);
        float radius = width / 2;
        
        // ピクセルごとに処理
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 中心からの距離
                float dist = Vector2.Distance(new Vector2(x, y), center);
                
                // 円の内側なら白、外側なら透明
                Color color = dist <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        
        // 変更を適用
        texture.Apply();
        
        // PNGとして保存
        byte[] bytes = texture.EncodeToPNG();
        string path = "Assets/Sprites/Circle.png";
        System.IO.File.WriteAllBytes(path, bytes);
        
        // アセットを更新
        AssetDatabase.Refresh();
        
        Debug.Log("Circle texture created at: " + path);
    }
}
