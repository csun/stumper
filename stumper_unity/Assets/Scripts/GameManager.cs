using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

        public enum MenuState
        {
            Gameplay,
            Menu,
            GameSummary
        }

        public event Action OnCurrentPlayerChanged;
        public event Action OnCurrentNodeChanged;
        public event Action OnCandidateWordChanged;
        public event Action<int> OnStrikesUpdated;
        public event Action<int, int> OnScoreUpdated;
        public event Action<int, int> OnMovesUpdated;
        public event Action OnTimerUpdated;
        public event Action OnTimerPauseChanged;
        public event Action OnMenuStateChanged;

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

        public MenuState CurrentMenuState
        {
            get => _currentMenuState;

            private set
            {
                _currentMenuState = value;
                OnMenuStateChanged?.Invoke();
            }
        }
        private MenuState _currentMenuState;
        public MenuAnimator MenuAnimator;

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

        public int CurrentPlayer
        {
            get => _currentPlayer;
            private set
            {
                _currentPlayer = value;
                OnCurrentPlayerChanged?.Invoke();
            }
        }
        int _currentPlayer;
        int nextPlayer => (CurrentPlayer + 1) % PlayerCount;

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
        Node roundStartingNode;

        [HideInInspector]
        public float Timer;
        [HideInInspector]
        public bool TimerPaused;
        [HideInInspector]
        public int[] Strikes;
        [HideInInspector]
        public int[] Moves;
        [HideInInspector]
        public int[] Scores;

        HashSet<Node> usedNodes = new();

        public void ToggleMenu()
        {
            if (CurrentMenuState == MenuState.Gameplay)
            {
                CurrentMenuState = MenuState.Menu;
                MenuAnimator.OpenMenu();
            }
            else
            {
                CurrentMenuState = MenuState.Gameplay;
                MenuAnimator.CloseMenu();
            }
        }

        public void OpenHelp()
        {
            CurrentMenuState = MenuState.Menu;
            MenuAnimator.OpenHelp();
        }

        public void ToggleTimerPause()
        {
            TimerPaused = !TimerPaused;
            OnTimerPauseChanged?.Invoke();
        }

        public void HandleBackspacePressed()
        {
            if (CurrentMenuState != MenuState.Gameplay) { return; }
            if (CandidateWord.Length == 0)
            {
                return;
            }

            CandidateWord = CandidateWord.Substring(0, CandidateWord.Length - 1);
        }

        public void HandleLetterPressed(char letter)
        {
            if (CurrentMenuState != MenuState.Gameplay) { return; }
            CandidateWord += char.ToUpper(letter);
        }

        public void SubmitWord()
        {
            if (CurrentMenuState != MenuState.Gameplay) { return; }
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


            if (MoveLimitEnabled && Moves[CurrentPlayer] <= 0)
            {
                DeclareLoser();
            }
        }

        public void NewGame()
        {
            ResetGameState();
        }

        public void Restart()
        {
            ResetGameState(false);
        }

        private void HandleInvalidMove()
        {
            if (strikesEnabled)
            {
                Strikes[CurrentPlayer]++;
                OnStrikesUpdated?.Invoke(CurrentPlayer);

                if (Strikes[CurrentPlayer] >= MaxStrikes)
                {
                    DeclareLoser();
                }
                else
                {
                    Debug.Log($"\tYou have used {Strikes[CurrentPlayer]} / {MaxStrikes} strikes.");
                }
            }

            AddMoves(CurrentPlayer, -1);

        }

        private void HandleValidMove(Node nextNode)
        {
            var oldScore = Scores[CurrentPlayer];
            Scores[CurrentPlayer] += CurrentNode.Word.Length;
            OnScoreUpdated?.Invoke(CurrentPlayer, oldScore);

            if (CurrentNode.Word.Length < nextNode.Word.Length && MoveLimitEnabled)
            {
                AddMoves(CurrentPlayer, WordLengthAddedMoves[Math.Min(CurrentNode.Word.Length - WordGraph.StartingWordLength, WordLengthAddedMoves.Count - 1)]);
            }
            else
            {
                AddMoves(CurrentPlayer, -1);
            }

            usedNodes.Add(nextNode);

            // NOTE - Need to do this after adding this node as used so that it is included
            // in stumper calculations
            CurrentNode = nextNode;
            CurrentPlayer = nextPlayer;

            var valid = ValidMoves();
            if (valid.Count() == 0)
            {
                DeclareLoser();
            }
        }

        void AddMoves(int player, int amount)
        {
            var oldMoves = Moves[player];
            Moves[player] += amount;
            OnMovesUpdated?.Invoke(player, oldMoves);
        }

        public void DeclareLoser()
        {
            var valid = ValidMoves();
            var validStr = "Valid choices were: ";
            foreach (var node in valid)
            {
                validStr += node.Word + ", ";
            }

            CurrentMenuState = MenuState.GameSummary;
            MenuAnimator.OpenSummary();
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

        void ResetGameState(bool newWord = true)
        {
            candidateUsesCharacter.Clear();
            usedNodes.Clear();
            CurrentPlayer = 0;
            ResetTimer();

            for (var i = 0; i < PlayerCount; i++)
            {
                var oldMoves = Moves[i];
                var oldScore = Scores[i];
                Moves[i] = InitialMoves;
                Scores[i] = 0;
                Strikes[i] = 0;

                OnStrikesUpdated?.Invoke(i);
                OnScoreUpdated?.Invoke(i, oldScore);
                OnMovesUpdated?.Invoke(i, oldMoves);
            }

            if (newWord)
            {
                CurrentNode = WordGraph.GetRandomStartNode();
                roundStartingNode = CurrentNode;
            }
            else
            {
                CurrentNode = roundStartingNode;
            }

            CurrentMenuState = MenuState.Gameplay;
            MenuAnimator.CloseMenu();
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
            Scores = new int[PlayerCount];
            Moves = new int[PlayerCount];
            Strikes = new int[PlayerCount];
            NewGame();
        }

        void Update()
        {
            if (CurrentMenuState != MenuState.Gameplay) { return; }
            UpdateCurrentTimer();
        }

        void UpdateCurrentTimer()
        {
            if (!timerEnabled || TimerPaused || CurrentMenuState != MenuState.Gameplay)
            {
                return;
            }

            Timer -= Time.deltaTime;
            OnTimerUpdated?.Invoke();
            if (Timer <= 0)
            {
                AddMoves(CurrentPlayer, -1);
                if (Moves[CurrentPlayer] <= 0)
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
