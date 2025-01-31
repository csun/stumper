using System;
using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class ScoreAndStrikesDisplay : MonoBehaviour
    {
        public TMP_Text StrikesText;
        public RollInText ScoreText;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnStrikesUpdated += OnStrikesUpdated;
            Manager.OnScoreUpdated += OnMoveCountUpdated;
        }

        private void OnMoveCountUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            ScoreText.ChangeText(Manager.Scores[player].ToString());
        }

        private void OnStrikesUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            var remaining = Manager.MaxStrikes - Manager.Strikes[player];
            var strikesText = "";
            for (var i = 0; i < remaining - 1; i++)
            {
                strikesText += "* ";
            }
            if (remaining > 0)
            {
                strikesText += "*";
            }

            StrikesText.text = strikesText;
        }
    }
}
