using UnityEditor;
using UnityEditor.SceneManagement;
public class EnterMainPlayMode
{
    [MenuItem("Tools/Enter Main Play Mode")]
    static void Enter()
    {
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/Main.unity");
        EditorApplication.isPlaying = true;
    }
}
