using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class TimerPauseButton : MonoBehaviour
    {
        public GameManager Manager;
        public TMP_Text Text;

        void Start()
        {
            Manager.OnTimerPauseChanged += OnTimerPauseChanged;
        }

        public void OnClick()
        {
            Manager.ToggleTimerPause();
        }

        public void OnTimerPauseChanged()
        {
            if (Manager.TimerPaused)
            {
                // Show the play button
                Text.text = "c";
            }
            else
            {
                Text.text = "d";
            }
        }
    }
}
