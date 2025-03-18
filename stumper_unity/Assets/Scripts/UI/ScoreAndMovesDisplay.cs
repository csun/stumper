using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stumper
{
    internal class ScoreAndMovesDisplay : MonoBehaviour
    {
        public RollInText MovesText;
        public RollInText ScoreText;
        public Image ActivePlayerUnderline;

        public int AssociatedPlayer;
        public GameManager Manager;

        void Start()
        {
            Manager.OnMovesUpdated += UpdateMovesText;
            if (ScoreText != null)
            {
                Manager.OnScoreUpdated += OnScoreUpdated;
            }
            if (ActivePlayerUnderline != null)
            {
                Manager.OnCurrentPlayerChanged += OnPlayerChanged;
            }
        }

        private void UpdateMovesText(int player, int oldMoves)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            MovesText.ChangeNumericalValue(Manager.Moves[player], oldMoves);
        }

        private void OnScoreUpdated(int player, int oldScore)
        {
            if (player != AssociatedPlayer)
            {
                return;
            }

            ScoreText.ChangeNumericalValue(Manager.Scores[player], oldScore);
        }

        private void OnPlayerChanged()
        {
            ActivePlayerUnderline.enabled = Manager.CurrentPlayer == AssociatedPlayer;
        }
    }
}
