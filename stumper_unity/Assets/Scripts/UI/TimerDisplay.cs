using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class TimerDisplay : MonoBehaviour
    {
        public TMP_Text Text;
        public RadialProgressBar ProgressBar;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnTimerUpdated += OnTimerUpdated;
        }

        void OnTimerUpdated()
        {
            ProgressBar.SetProgress(Manager.Timer / Manager.MaxTimer);

            var time = Mathf.CeilToInt(Mathf.Max(Manager.Timer, 0));
            if (time >= 60)
            {
                var minutes = time / 60;
                var seconds = time % 60;
                Text.text = $"{minutes}:{seconds:00}";
            }
            else
            {
                Text.text = $"{time}";
            }
        }
    }
}
