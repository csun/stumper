using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stumper
{
    internal class GameManager : MonoBehaviour
    {
        public event Action OnCurrentNodeChanged;
        public event Action<int> OnTimerUpdated;

        public Graph WordGraph;

        public int MaxStrikes;

        public float StartingTimer;
        public float PerMoveAddedTime;
        public float MaxTimer;

        int currentPlayer;
        int nextPlayer => (currentPlayer + 1) % 2;

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

        public float[] Timers = new float[2];

        int[] strikes = new int[2];
        HashSet<Node> usedNodes = new();

        public void PlayWord(string word)
        {
            var node = WordGraph.Query(word);

            if (node is null)
            {
                AddStrike("Invalid word.");
                return;
            }
            if (usedNodes.Contains(node))
            {
                AddStrike("Word has already been played.");
                return;
            }
            if (!CurrentNode.Children.Contains(node))
            {
                AddStrike("Word is not reachable from current word.");
                return;
            }

            CurrentNode = node;

            usedNodes.Add(node);
            Timers[currentPlayer] = Math.Min(MaxTimer, Timers[currentPlayer] + PerMoveAddedTime);
            OnTimerUpdated.Invoke(currentPlayer);
            currentPlayer = nextPlayer;

            var valid = ValidMoves();
            if (valid.Count() == 0)
            {
                DeclareLoser();
            }
        }

        void AddStrike(string message)
        {
            Debug.Log(message);
            strikes[currentPlayer]++;

            if (strikes[currentPlayer] >= MaxStrikes)
            {
                DeclareLoser();
            }
            else
            {
                Debug.Log($"\tYou have used {strikes[currentPlayer]} / {MaxStrikes} strikes.");
            }
        }

        void DeclareLoser()
        {
            Debug.Log($"Player {currentPlayer} has been Stumped.");
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
            CurrentNode = WordGraph.GetRandomStartNode();
            Timers[1] = StartingTimer;
            currentPlayer = 0;
            usedNodes.Clear();

            for (var i = 0; i < 2; i++)
            {
                Timers[i] = StartingTimer;
                strikes[i] = 0;
                OnTimerUpdated(i);
            }
        }

        void Start()
        {
            WordGraph.RegenerateValidStartNodes();
            ResetGameState();
        }
    
        void Update()
        {
            Timers[currentPlayer] -= Time.deltaTime;
            OnTimerUpdated.Invoke(currentPlayer);
            if (Timers[currentPlayer] <= 0)
            {
                DeclareLoser();
            }
        }
    }
}
