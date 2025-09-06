using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

public class PianoAndViolinAutoSetup : EditorWindow
{
    [MenuItem("Tools/PianoAndViolin/Auto Setup Scene")]
    public static void ShowWindow()
    {
        GetWindow<PianoAndViolinAutoSetup>("PianoAndViolin Auto Setup");
    }

    void OnGUI()
    {
        if (GUILayout.Button("セットアップ実行"))
        {
            SetupScene();
        }
    }

    static void SetupScene()
    {
        string scenePath = "Assets/Games/06_PianoAndViolin/Scenes/PianoAndViolin.unity";
        EditorSceneManager.OpenScene(scenePath);

        string spritePath = "Assets/Games/06_PianoAndViolin/Sprites/";

        GameObject piano = new GameObject("Piano");
        var pianoSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "piano.png");
        var pianoRenderer = piano.AddComponent<SpriteRenderer>();
        pianoRenderer.sprite = pianoSprite;
        piano.AddComponent<BoxCollider2D>();
        var pianoAnimator = piano.AddComponent<Animator>();
        piano.transform.position = new Vector3(-2, 0, 0);

        GameObject violin = new GameObject("Violin");
        var violinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "violin.png");
        var violinRenderer = violin.AddComponent<SpriteRenderer>();
        violinRenderer.sprite = violinSprite;
        violin.AddComponent<BoxCollider2D>();
        var violinAnimator = violin.AddComponent<Animator>();
        violin.transform.position = new Vector3(2, 0, 0);

        GameObject note = new GameObject("Note");
        var noteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "note.png");
        var noteRenderer = note.AddComponent<SpriteRenderer>();
        noteRenderer.sprite = noteSprite;
        note.AddComponent<BoxCollider2D>();
        var noteAnimator = note.AddComponent<Animator>();
        note.transform.position = new Vector3(0, 2, 0);

        GameObject sfxPlayerObj = new GameObject("PianoAndViolinSFXPlayer");
        var sfxPlayer = sfxPlayerObj.AddComponent<PianoAndViolinSFXPlayer>();
        sfxPlayer.pianoSource = sfxPlayerObj.AddComponent<AudioSource>();
        sfxPlayer.violinSource = sfxPlayerObj.AddComponent<AudioSource>();
        sfxPlayer.pianoViolinSource = sfxPlayerObj.AddComponent<AudioSource>();

        GameObject controllerObj = new GameObject("PianoAndViolinGameController");
        var controller = controllerObj.AddComponent<PianoAndViolinGameController>();
        controller.pianoObject = piano;
        controller.violinObject = violin;
        controller.noteObject = note;
        controller.sfxPlayer = sfxPlayer;
        controller.pianoAnimator = pianoAnimator;
        controller.violinAnimator = violinAnimator;
        controller.noteAnimator = noteAnimator;
        // AudioClip配列はInspectorで割り当て

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("完了", "PianoAndViolinシーンのセットアップが完了しました。", "OK");
    }
}
