using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stumper
{
    internal class GameManager : MonoBehaviour
    {
        public enum CandidateWordStatus
        {
            PotentiallyValid,
            Valid,
            Invalid
        }

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
                UpdateCandidateValidity();
                OnCandidateWordChanged();
            }
        }
        string _candidateWord = "";
        public CandidateWordStatus CandidateStatus { get; private set; }
        public string CandidateInvalidReason { get; private set; }
        List<bool> candidateUsesCharacter = new();

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
                UpdateCandidateValidity();
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
            if (CandidateStatus == CandidateWordStatus.Invalid)
            {
                return;
            }

            var node = WordGraph.Query(CandidateWord);
            CandidateWord = "";
            ResetTimer();

            if (node is null || usedNodes.Contains(node) || !CurrentNode.Children.Contains(node))
            {
                HandleInvalidMove();
            }
            else
            {
                HandleValidMove(node);
            }


            if (MoveLimitEnabled && Moves[currentPlayer] <= 0)
            {
                DeclareLoser();
            }
        }

        private void HandleInvalidMove()
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

            AddMoves(currentPlayer, -1);

        }

        private void HandleValidMove(Node nextNode)
        {
            Scores[currentPlayer] += CurrentNode.Word.Length;
            OnScoreUpdated?.Invoke(currentPlayer);

            if (CurrentNode.Word.Length < nextNode.Word.Length && MoveLimitEnabled)
            {
                AddMoves(currentPlayer, WordLengthAddedMoves[Math.Min(CurrentNode.Word.Length - WordGraph.StartingWordLength, WordLengthAddedMoves.Count - 1)]);
            }
            else
            {
                AddMoves(currentPlayer, -1);
            }

            usedNodes.Add(nextNode);

            // NOTE - Need to do this after adding this node as used so that it is included
            // in stumper calculations
            CurrentNode = nextNode;
            currentPlayer = nextPlayer;

            var valid = ValidMoves();
            if (valid.Count() == 0)
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

            candidateUsesCharacter.Clear();
            usedNodes.Clear();
            currentPlayer = 0;
            ResetTimer();

            for (var i = 0; i < PlayerCount; i++)
            {
                Moves[i] = InitialMoves;
                OnStrikesUpdated?.Invoke(i);
                OnScoreUpdated?.Invoke(i);
                OnMovesUpdated?.Invoke(i);
            }

            CurrentNode = WordGraph.GetRandomStartNode();
        }

        void UpdateCandidateValidity()
        {
            var candidateLetterCounts = new Dictionary<char, int>();
            foreach (var c in CandidateWord)
            {
                candidateLetterCounts.TryAdd(c, 0);
                candidateLetterCounts[c]++;
            }

            var charactersUnaccountedFor = CandidateWord.Length;
            candidateUsesCharacter.Clear();
            foreach (var c in CurrentNode.Word)
            {
                var isUsed = false;

                if (candidateLetterCounts.ContainsKey(c))
                {
                    candidateLetterCounts[c]--;
                    charactersUnaccountedFor--;
                    if (candidateLetterCounts[c] == 0)
                    {
                        candidateLetterCounts.Remove(c);
                    }
                    isUsed = true;
                }

                candidateUsesCharacter.Add(isUsed);
            }

            // Potentially valid - shorter length, max one candidate letter not accounted for
            if (CandidateWord.Length < CurrentNode.Word.Length && charactersUnaccountedFor <= 1)
            {
                CandidateStatus = CandidateWordStatus.PotentiallyValid;
            }
            // Edit - same length, one candidate letter not accounted for
            // Anagram - same length, no candidate letters unaccounted for, not same word
            else if (CandidateWord.Length == CurrentNode.Word.Length &&
                    charactersUnaccountedFor <= 1 &&
                    CandidateWord != CurrentNode.Word)
            {
                CandidateStatus = CandidateWordStatus.Valid;
            }
            // Extension - length + 1, one candidate letter not accounted for
            else if (CandidateWord.Length == CurrentNode.Word.Length + 1 &&
                    charactersUnaccountedFor == 1)
            {
                CandidateStatus = CandidateWordStatus.Valid;
            }
            else
            {
                CandidateStatus = CandidateWordStatus.Invalid;
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
