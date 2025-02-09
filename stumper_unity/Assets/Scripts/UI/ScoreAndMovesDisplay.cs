using System;
using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class ScoreAndMovesDisplay : MonoBehaviour
    {
        public TMP_Text MovesText;
        public RollInText ScoreText;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnMovesUpdated += UpdateMovesText;
            if (ScoreText != null)
            {
                Manager.OnScoreUpdated += OnScoreUpdated;
            }
        }

        private void UpdateMovesText(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            MovesText.text = $"{Manager.Moves[player]}";
        }

        private void OnScoreUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            ScoreText.ChangeText(Manager.Scores[player].ToString());
        }
    }
}
