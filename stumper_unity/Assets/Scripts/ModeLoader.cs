using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stumper
{
    public class ModeLoader : MonoBehaviour
    {
        public void LoadSingleplayer()
        {
            SceneManager.LoadScene("SingleplayerMode", LoadSceneMode.Single);
        }

        public void LoadVersus()
        {
            SceneManager.LoadScene("CompetitiveMode", LoadSceneMode.Single);
        }
    }
}
