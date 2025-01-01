using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Stumper
{
    internal class GameManager : MonoBehaviour
    {
        public event Action OnCurrentNodeChanged;

        public Graph WordGraph;

        public int MaxStrikes;

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
            Debug.Log($"Valid choies were {ValidMoves()}");
            Application.Quit();
        }

        IEnumerable<Node> ValidMoves()
        {
            return CurrentNode.Children.Except(usedNodes);
        }

        void ResetGameState()
        {
            CurrentNode = WordGraph.GetRandomStartNode();
            WordGraph.ShuffleStartNodes();
            currentPlayer = 0;
            strikes[0] = 0;
            strikes[1] = 0;
            usedNodes.Clear();
        }

        void Start()
        {
            ResetGameState();
        }
    }
}
