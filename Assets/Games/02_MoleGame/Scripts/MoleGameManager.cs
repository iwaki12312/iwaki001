using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MoleGameManager : MonoBehaviour
{
    public HoleController[] holes;
    public List<MoleData> moleDataList;
    
    private float minSpawnInterval = 0.5f;
    private float maxSpawnInterval = 1.4f;
    private float moleDuration = 1.1f;
    
    private void Start()
    {
        // モグラ出現コルーチンを開始
        StartCoroutine(SpawnMoles());
    }
    
    // モグラをランダムに出現させる
    private IEnumerator SpawnMoles()
    {
        while (true)
        {
            // ランダムな待機時間
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
            
            // 空いている穴を探す
            List<HoleController> availableHoles = new List<HoleController>();
            foreach (var hole in holes)
            {
                if (!hole.IsActive())
                {
                    availableHoles.Add(hole);
                }
            }
            
            // 空いている穴がなければスキップ
            if (availableHoles.Count == 0) continue;
            
            // ランダムな穴を選択
            HoleController selectedHole = availableHoles[Random.Range(0, availableHoles.Count)];
            
            // 重み付きランダムでモグラを選択
            MoleData selectedMole = GetRandomMoleByWeight();
            
            // モグラを出現させる
            selectedHole.ShowMole(selectedMole, moleDuration);
        }
    }
    
    // 重み付きランダムでモグラを選択
    private MoleData GetRandomMoleByWeight()
    {
        int totalWeight = 0;
        foreach (var mole in moleDataList)
        {
            totalWeight += mole.spawnWeight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var mole in moleDataList)
        {
            currentWeight += mole.spawnWeight;
            if (randomValue < currentWeight)
            {
                return mole;
            }
        }
        
        // 万が一の場合は最初のモグラを返す
        return moleDataList[0];
    }
    
    // メインメニューに戻る
    public void ReturnToMainMenu()
    {
        // シーン名を直接使用
        SceneManager.LoadScene("Menu");
    }
}
