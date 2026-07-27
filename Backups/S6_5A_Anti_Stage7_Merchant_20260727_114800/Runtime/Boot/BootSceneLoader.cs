using UnityEngine;
using UnityEngine.SceneManagement;

namespace GuildMaster.Runtime.Boot
{
    /// <summary>
    /// Minimal Boot -> Main scene transition for real builds (Standalone/APK), where
    /// Boot.unity is the first scene in Build Settings. Editor Play Mode on Main.unity
    /// directly does not go through this. No composition/service wiring here — that
    /// stays in UIRuntimeBootstrap (S5) inside Main.unity.
    /// </summary>
    public class BootSceneLoader : MonoBehaviour
    {
        [SerializeField] private string _mainSceneName = "Main";

        private void Start()
        {
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
