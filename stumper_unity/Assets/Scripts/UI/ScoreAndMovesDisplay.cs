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
            Manager.OnMoveCapUpdated += () => UpdateMovesText(AssociatedPlayer);
            Manager.OnMovesUpdated += UpdateMovesText;
            Manager.OnScoreUpdated += OnMoveCountUpdated;
        }

        private void UpdateMovesText(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            MovesText.text = $"{Manager.Moves[player]} / {Manager.MoveCap}";
        }

        private void OnMoveCountUpdated(int player)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            ScoreText.ChangeText(Manager.Scores[player].ToString());
        }
    }
}
