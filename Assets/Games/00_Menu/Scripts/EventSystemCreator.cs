using UnityEngine;
using UnityEngine.EventSystems;

// EventSystemが存在しない場合に自動的に作成するクラス
public class EventSystemCreator : MonoBehaviour
{
    void Awake()
    {
        // シーン内にEventSystemが存在するか確認
        if (FindObjectOfType<EventSystem>() == null)
        {
            // EventSystemが存在しない場合は作成
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Debug.Log("EventSystemを作成しました");
        }
    }
}
