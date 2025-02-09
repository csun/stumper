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
        public event Action<int> OnMovesUpdated;
        public event Action OnTimerUpdated;

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

        [Tooltip("Disables move cap if set to 0")]
        public int InitialMoves;
        private bool MoveLimitEnabled => InitialMoves > 0;
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
        public float Timer;
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
            ResetTimer();

            if (node is null || usedNodes.Contains(node) || !CurrentNode.Children.Contains(node))
            {
                RegisterMove(false);
                return;
            }

            if (CurrentNode.Word.Length < node.Word.Length && MoveLimitEnabled)
            {
                AddMoves(currentPlayer, WordLengthAddedMoves[Math.Min(CurrentNode.Word.Length - WordGraph.StartingWordLength, WordLengthAddedMoves.Count - 1)]);
            }

            usedNodes.Add(node);
            // NOTE - Need to do this after adding this node as used so that it is included
            // in stumper calculations
            CurrentNode = node;
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

            if (valid)
            {
                Scores[currentPlayer] += CurrentNode.Word.Length;
                OnScoreUpdated?.Invoke(currentPlayer);
            }

            AddMoves(currentPlayer, -1);

            if (MoveLimitEnabled && Moves[currentPlayer] <= 0)
            {
                DeclareLoser();
            }
        }

        void AddMoves(int player, int amount)
        {
            Moves[player] += amount;
            OnMovesUpdated?.Invoke(player);
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

        void ResetTimer()
        {
            if (!timerEnabled)
            {
                return;
            }

            Timer = MaxTimer;
            OnTimerUpdated?.Invoke();
        }

        void ResetGameState()
        {
            Strikes = new int[PlayerCount];
            Moves = new int[PlayerCount];
            Scores = new int[PlayerCount];

            ResetTimer();
            CurrentNode = WordGraph.GetRandomStartNode();
            currentPlayer = 0;
            usedNodes.Clear();

            for (var i = 0; i < PlayerCount; i++)
            {
                Moves[i] = InitialMoves;
                OnStrikesUpdated?.Invoke(i);
                OnScoreUpdated?.Invoke(i);
                OnMovesUpdated?.Invoke(i);
            }
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

            Timer -= Time.deltaTime;
            OnTimerUpdated?.Invoke();
            if (Timer <= 0)
            {
                AddMoves(currentPlayer, -1);
                if (Moves[currentPlayer] <= 0)
                {
                    DeclareLoser();
                }
                else
                {
                    ResetTimer();
                }
            }
        }
    }
}
