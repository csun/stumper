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

        public enum LossReason
        {
            Conceded,
            OutOfMoves,
            OutOfTime,
            Stumper
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
        public event Action<string> OnUpdateInfoMessage;

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

        public LossReason LastLossReason;
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
        HashSet<string> usedWords = new();

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
            if (CandidateStatus != CandidateWordStatus.Valid)
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

            var valid = ValidMoves();
            if (valid.Count() == 0)
            {
                DeclareLoser(LossReason.Stumper);
            }
            else if (MoveLimitEnabled && Moves[CurrentPlayer] <= 0)
            {
                DeclareLoser(LossReason.OutOfMoves);
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
                    DeclareLoser(LossReason.OutOfMoves);
                }
                else
                {
                    Debug.Log($"\tYou have used {Strikes[CurrentPlayer]} / {MaxStrikes} strikes.");
                }
            }

            AddMoves(CurrentPlayer, -1);
            OnUpdateInfoMessage?.Invoke("That's not a real word!");
        }

        private void HandleValidMove(Node nextNode)
        {
            var oldScore = Scores[CurrentPlayer];
            Scores[CurrentPlayer] += CurrentNode.Word.Length;
            OnScoreUpdated?.Invoke(CurrentPlayer, oldScore);

            if (CurrentNode.Word.Length < nextNode.Word.Length && MoveLimitEnabled)
            {
                AddMoves(CurrentPlayer, WordLengthAddedMoves[Math.Min(CurrentNode.Word.Length - WordGraph.StartingWordLength, WordLengthAddedMoves.Count - 1)]);
                OnUpdateInfoMessage?.Invoke("Word extended! Gained extra moves.");
            }
            else
            {
                AddMoves(CurrentPlayer, -1);
            }

            usedNodes.Add(nextNode);
            usedWords.Add(nextNode.Word);

            // NOTE - Need to do this after adding this node as used so that it is included
            // in stumper calculations
            CurrentNode = nextNode;
            CurrentPlayer = nextPlayer;
        }

        void AddMoves(int player, int amount)
        {
            var oldMoves = Moves[player];
            Moves[player] += amount;
            OnMovesUpdated?.Invoke(player, oldMoves);
        }

        public void Concede()
        {
            DeclareLoser(LossReason.Conceded);
        }

        void DeclareLoser(LossReason lossReason)
        {
            LastLossReason = lossReason;
            CurrentMenuState = MenuState.GameSummary;
            // Don't delay if we've conceded as that means we're already in the menu
            MenuAnimator.OpenSummary(lossReason != LossReason.Conceded);
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
            usedNodes.Clear();
            usedWords.Clear();
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

            usedNodes.Add(CurrentNode);
            usedWords.Add(CurrentNode.Word);
            CurrentMenuState = MenuState.Gameplay;
            CandidateWord = "";
            OnUpdateInfoMessage?.Invoke("");
            MenuAnimator.CloseMenu();
        }

        void UpdateCandidateValidity()
        {
            const string MULTI_CHANGE_REASON = "Altered / added more than 1 letter.";

            var lengthDiff = CandidateWord.Length - CurrentNode.Word.Length;
            // Remove these two cases as possibilities immediately to simplify later logic
            if (usedWords.Contains(CandidateWord))
            {
                CandidateStatus = CandidateWordStatus.Invalid;
                CandidateInvalidReason = "Re-used a previously played word.";
                return;
            }
            else if (lengthDiff > 1)
            {
                CandidateStatus = CandidateWordStatus.Invalid;
                CandidateInvalidReason = MULTI_CHANGE_REASON;
                return;
            }

            // First check if might be anagram
            var anagramLetterCounts = new Dictionary<char, int>();
            foreach (var c in CandidateWord)
            {
                anagramLetterCounts.TryAdd(c, 0);
                anagramLetterCounts[c]++;
            }
            foreach (var c in CurrentNode.Word)
            {
                anagramLetterCounts.TryAdd(c, 0);
                anagramLetterCounts[c]--;
            }

            var maybeAnagram = true;
            foreach (var count in anagramLetterCounts.Values)
            {
                // There is at least one letter in the candidate which is not part of the current word
                if (count > 0)
                {
                    maybeAnagram = false;
                    break;
                }
            }

            // Valid case 1: full anagram. Exit early.
            // Note that we've already handled the case of the same exact word with the used word check
            // at the start of this function.
            if (maybeAnagram && lengthDiff == 0)
            {
                CandidateStatus = CandidateWordStatus.Valid;
                return;
            }
            // Potentially valid case 1: on the way to becoming an anagram / no changes yet. Exit early
            else if (maybeAnagram)
            {
                CandidateStatus = CandidateWordStatus.PotentiallyValid;
                return;
            }

            // If impossible for it to be an anagram, we should only be able to find one edit / insertion
            bool prefixMatches(int head, int candidateOffset)
            {
                while (head + candidateOffset < CandidateWord.Length && head < CurrentNode.Word.Length)
                {
                    if (CandidateWord[head + candidateOffset] != CurrentNode.Word[head])
                    {
                        return false;
                    }
                    head++;
                }

                return true;
            }

            for (var head = 0; head < CandidateWord.Length && head < CurrentNode.Word.Length; head++)
            {
                // Because we know this is not an anagram, we can only find a max of one change
                // Entering this block triggers the search for any more changes that would
                // invalidate this word.
                if (CandidateWord[head] != CurrentNode.Word[head])
                {
                    // Indicates that this character was an in-place alteration
                    if (prefixMatches(head + 1, 0) && lengthDiff <= 0)
                    {
                        CandidateStatus = lengthDiff == 0 ? CandidateWordStatus.Valid : CandidateWordStatus.PotentiallyValid;
                    }
                    // Indicates that this character was an insertion
                    else if (prefixMatches(head, 1))
                    {
                        CandidateStatus = lengthDiff == 1 ? CandidateWordStatus.Valid : CandidateWordStatus.PotentiallyValid;
                    }
                    else
                    {
                        CandidateStatus = CandidateWordStatus.Invalid;
                        CandidateInvalidReason = MULTI_CHANGE_REASON;
                    }
                    return;
                }
            }

            // To get to here, we must be in the case where we've added exactly one character to the end of the current word.
            CandidateStatus = CandidateWordStatus.Valid;
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
                    DeclareLoser(LossReason.OutOfTime);
                }
                else
                {
                    ResetTimer();
                }
            }
        }

    }
}
