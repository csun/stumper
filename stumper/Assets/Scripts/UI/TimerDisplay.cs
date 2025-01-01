using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class TimerDisplay : MonoBehaviour
    {
        public TMP_Text Text;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnTimerUpdated += OnTimerUpdated;
        }

        void OnTimerUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            Text.text = Mathf.CeilToInt(Mathf.Max(Manager.Timers[player], 0)).ToString();
        }
    }
}
