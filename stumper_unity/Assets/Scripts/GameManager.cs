using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common.Merge;
using UnityEngine;

namespace Stumper
{
    internal class GameManager : MonoBehaviour
    {
        public event Action OnCurrentNodeChanged;
        public event Action OnCandidateWordChanged;
        public event Action<int> OnStrikesUpdated;
        public event Action<int> OnScoreUpdated;
        public event Action OnMoveCapUpdated;
        public event Action<int> OnMovesUpdated;
        public event Action<int> OnTimerUpdated;
        public event Action<int, float> OnTimerBonusOrPenalty;

        public Graph WordGraph;
        public string CandidateWord
        {
            get => _candidateWord;
            private set
            {
                _candidateWord = value;
                OnCandidateWordChanged();
            }
        }
        string _candidateWord = "";

        public int PlayerCount;

        [Tooltip("Disables strikes if set to 0")]
        public int MaxStrikes;
        private bool strikesEnabled => MaxStrikes > 0;

        [Tooltip("Disables timer if set to 0")]
        public float MaxTimer;
        private bool timerEnabled => MaxTimer > 0;
        public float StartingTimer;
        public float PerMoveAddedTime;
        public float StrikeTimePenalty;

        [Tooltip("Disables move cap if set to 0")]
        public int InitialMoveCap;
        [HideInInspector]
        public int MoveCap;
        private bool moveCapEnabled => InitialMoveCap > 0;
        [Tooltip("Element 0 is for starting word length, each subsequent element is for next length")]
        public List<int> WordLengthAddedMoves;

        int currentPlayer;
        int nextPlayer => (currentPlayer + 1) % PlayerCount;

        public Node CurrentNode
        {
            get => _currentNode;
            set
            {
                _currentNode = value;
                OnCurrentNodeChanged.Invoke();
            }
        }
        Node _currentNode;

        [HideInInspector]
        public float[] Timers;
        [HideInInspector]
        public int[] Strikes;
        [HideInInspector]
        public int[] Moves;
        [HideInInspector]
        public int[] Scores;

        HashSet<Node> usedNodes = new();

        public void HandleBackspacePressed()
        {
            if (CandidateWord.Length == 0)
            {
                return;
            }

            CandidateWord = CandidateWord.Substring(0, CandidateWord.Length - 1);
        }

        public void HandleLetterPressed(char letter)
        {
            CandidateWord += char.ToUpper(letter);
        }

        public void SubmitWord()
        {
            var node = WordGraph.Query(CandidateWord);
            CandidateWord = "";

            if (node is null || usedNodes.Contains(node) || !CurrentNode.Children.Contains(node))
            {
                RegisterMove(false);
                return;
            }

            if (CurrentNode.Word.Length < node.Word.Length && moveCapEnabled)
            {
                MoveCap += WordLengthAddedMoves[Math.Min(CurrentNode.Word.Length - WordGraph.StartingWordLength, WordLengthAddedMoves.Count - 1)];
                OnMoveCapUpdated?.Invoke();
            }

            usedNodes.Add(node);
            // NOTE - Need to do this after adding this node as used so that it is included
            // in stumper calculations
            CurrentNode = node;
            IncrementTimer(currentPlayer, PerMoveAddedTime);
            RegisterMove(true);

            currentPlayer = nextPlayer;

            var valid = ValidMoves();
            if (valid.Count() == 0)
            {
                DeclareLoser();
            }
        }

        private void RegisterMove(bool valid)
        {
            if (strikesEnabled)
            {
                Strikes[currentPlayer]++;
                OnStrikesUpdated?.Invoke(currentPlayer);

                if (Strikes[currentPlayer] >= MaxStrikes)
                {
                    DeclareLoser();
                }
                else
                {
                    Debug.Log($"\tYou have used {Strikes[currentPlayer]} / {MaxStrikes} strikes.");
                }
            }

            if (timerEnabled)
            {
                IncrementTimer(currentPlayer, -StrikeTimePenalty);
            }

            if (valid)
            {
                Scores[currentPlayer] += CurrentNode.Word.Length;
                OnScoreUpdated?.Invoke(currentPlayer);
            }

            Moves[currentPlayer]++;
            OnMovesUpdated?.Invoke(currentPlayer);

            if (moveCapEnabled && Moves[currentPlayer] >= MoveCap)
            {
                DeclareLoser();
            }
        }

        void IncrementTimer(int player, float amount)
        {
            if (!timerEnabled || amount == 0)
            {
                return;
            }

            Timers[player] = Math.Min(MaxTimer, Timers[player] + amount);
            OnTimerUpdated?.Invoke(player);
            OnTimerBonusOrPenalty.Invoke(player, amount);
        }

        void DeclareLoser()
        {
            Debug.Log($"Player {currentPlayer} has been Stumped with score {Scores[currentPlayer]}.");
            var valid = ValidMoves();
            var validStr = "Valid choices were: ";
            foreach (var node in valid)
            {
                validStr += node.Word + ", ";
            }
            Debug.Log(validStr);
            ResetGameState();
        }

        public IEnumerable<Node> ValidMoves()
        {
            return ValidMoves(CurrentNode);
        }

        public IEnumerable<Node> ValidMoves(Node node)
        {
            return node.Children.Except(usedNodes);
        }

        void ResetGameState()
        {
            Strikes = new int[PlayerCount];
            Timers = new float[PlayerCount];
            Moves = new int[PlayerCount];
            Scores = new int[PlayerCount];

            CurrentNode = WordGraph.GetRandomStartNode();
            currentPlayer = 0;
            usedNodes.Clear();

            for (var i = 0; i < PlayerCount; i++)
            {
                Timers[i] = StartingTimer;
                OnTimerUpdated?.Invoke(i);
                OnStrikesUpdated?.Invoke(i);
                OnScoreUpdated?.Invoke(i);
                OnMovesUpdated?.Invoke(i);
            }

            MoveCap = InitialMoveCap;
            OnMoveCapUpdated?.Invoke();
        }

        void Start()
        {
            WordGraph.RegenerateValidStartNodes();
            ResetGameState();
        }

        void Update()
        {
            UpdateCurrentTimer();
        }

        void UpdateCurrentTimer()
        {
            if (!timerEnabled)
            {
                return;
            }

            Timers[currentPlayer] -= Time.deltaTime;
            OnTimerUpdated?.Invoke(currentPlayer);
            if (Timers[currentPlayer] <= 0)
            {
                DeclareLoser();
            }
        }
    }
}
