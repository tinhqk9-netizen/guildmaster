using UnityEditor;
public class EnterPlayMode
{
    [MenuItem("Tools/Enter Play Mode")]
    static void Enter()
    {
        EditorApplication.isPlaying = true;
    }
}
