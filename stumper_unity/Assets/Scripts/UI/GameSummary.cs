using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class GameSummary : MonoBehaviour
    {
        public GameManager Manager;

        public TMP_Text LoserStatusText;

        public GameObject PossibleMovesSection;
        public GameObject PossibleMovesPrefab;
        public RollInText PossibleMoves;
        public GameObject NoPossibleMovesText;

        void Start()
        {

        }

        public void RefreshStats()
        {
            // It gets janky wiht coroutines and the way that rollin text is implemented if we're
            // activating and deactivating the gameobject (like we do with menus). Just destroy
            // it and start fresh.
            var oldRollin = PossibleMoves;
            PossibleMoves = Instantiate(PossibleMovesPrefab, PossibleMoves.transform.parent).GetComponent<RollInText>();
            Destroy(oldRollin.gameObject);

            var loserName = Manager.PlayerCount > 1 ? $"P{Manager.CurrentPlayer + 1}" : "You";
            string reason;
            switch (Manager.LastLossReason)
            {
                case GameManager.LossReason.Conceded:
                    reason = "conceded on";
                    break;
                case GameManager.LossReason.OutOfMoves:
                    reason = "ran out of moves on";
                    break;
                case GameManager.LossReason.OutOfTime:
                    reason = "ran out of time on";
                    break;
                case GameManager.LossReason.Stumper:
                    reason = "got stumped by";
                    break;
                default:
                    reason = "lost on";
                    break;
            }

            LoserStatusText.text = $"{loserName} {reason} {Manager.CurrentNode.Word}";

            var wasStumper = Manager.LastLossReason == GameManager.LossReason.Stumper;
            NoPossibleMovesText.SetActive(wasStumper);
            PossibleMovesSection.SetActive(!wasStumper);

            if (!wasStumper)
            {
                var valid = Manager.ValidMoves();
                foreach (var move in valid)
                {
                    PossibleMoves.ChangeText(move.Word);
                }
            }
        }
    }
}
