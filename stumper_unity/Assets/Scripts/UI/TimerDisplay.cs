using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class TimerDisplay : MonoBehaviour
    {
        public TMP_Text Text;
        public RollInText RollInText;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnTimerUpdated += OnTimerUpdated;
            Manager.OnTimerBonusOrPenalty += OnTimerBonusOrPenalty;
        }

        void OnTimerUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            var time = Mathf.CeilToInt(Mathf.Max(Manager.Timers[player], 0));
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

        void OnTimerBonusOrPenalty(int player, float amount)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            var sign = amount > 0 ? "+" : "";
            RollInText.ChangeText($"{sign}{Mathf.CeilToInt(amount).ToString()}s");
        }
    }
}
