using System;
using System.Collections.Generic;
using System.Linq;
using KaimiraGames;
using UnityEngine;

namespace Stumper
{
    class Node
    {
        public string Word;
        public string AnagramKey;
        public float LogFrequency;
        public HashSet<Node> Children = new();

        public Node(string word, float logFrequency)
        {
            Word = word;
            AnagramKey = String.Concat(word.OrderBy(c => c));
            LogFrequency = logFrequency;
        }

        public override string ToString()
        {
            var childString = "";
            foreach (var child in Children)
            {
                childString = childString + child.Word + " ";
            }
            return $"{Word}: [{childString}]";
        }
    }

    [CreateAssetMenu(fileName = "Graph", menuName = "Stumper/Graph", order = 0)]
    internal class Graph : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        struct SerializableNode
        {
            public string Word;
            public float LogFrequency;
            public List<int> Children;

            public SerializableNode(Node node, Dictionary<string, int> wordToIndex)
            {
                Word = node.Word;
                LogFrequency = node.LogFrequency;
                Children = new();

                foreach (var child in node.Children)
                {
                    Children.Add(wordToIndex[child.Word]);
                }
            }

            public Node ToNode()
            {
                return new(Word, LogFrequency);
            }
        }

        const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public TextAsset WordList;
        public bool AllowDeletions;
        public bool AllowAnagrams;
        public int StartingWordLength;
        public float StartingWordMinimumLogFrequency;

        Dictionary<string, Node> wordToNode;
        WeightedList<Node> validStartNodes;

        [SerializeField]
        List<SerializableNode> serializableNodes;

        public Node Query(string word)
        {
            return wordToNode.GetValueOrDefault(word.ToUpper(), null);
        }

        public Node GetRandomStartNode()
        {
            return validStartNodes.Next();
        }

        public void OnBeforeSerialize()
        {
            // I don't know if we need this - unsure if order is preserved when iterating over
            // dictionary values multiple times. If so don't need. I'm on a plane so can't check.
            var ordered = new List<Node>();
            var wordToIndex = new Dictionary<string, int>();
            serializableNodes = new();

            foreach (var node in wordToNode.Values)
            {
                var idx = ordered.Count;
                wordToIndex[node.Word] = idx;
                ordered.Add(node);
            }

            serializableNodes = new();
            foreach (var node in ordered)
            {
                serializableNodes.Add(new(node, wordToIndex));
            }
        }

        public void OnAfterDeserialize()
        {
            wordToNode = new();
            var newNodes = new List<Node>();

            foreach (var node in serializableNodes)
            {
                newNodes.Add(node.ToNode());
            }

            for (var i = 0; i < newNodes.Count; i++)
            {
                wordToNode[newNodes[i].Word] = newNodes[i];
                foreach (var childIdx in serializableNodes[i].Children)
                {
                    newNodes[i].Children.Add(newNodes[childIdx]);
                }
            }

            RegenerateValidStartNodes();
        }

        [ContextMenu("Debug Info")]
        void DebugInfo()
        {
            Debug.Log(Query("STAR"));

            var longWords = "";
            var startingWords = "";
            var maxLen = 0;
            foreach (var node in wordToNode.Values)
            {
                maxLen = Math.Max(maxLen, node.Word.Length);
                if (node.Word.Length >= 10)
                {
                    longWords += $"{node.Word}, ";
                }
            }
            foreach (var node in validStartNodes)
            {
                startingWords += $"{node.Word}({node.LogFrequency}), ";
            }

            Debug.Log($"Longest word is {maxLen} long");
            Debug.Log($"Long words {longWords}");
            Debug.Log($"Starting words: {startingWords}");
        }

        [ContextMenu("Regenerate Graph")]
        public void Regenerate()
        {
            wordToNode = new();

            var exploreQueue = new Queue<Node>();
            var alreadyExplored = new HashSet<string>();
            var shouldPrune = new HashSet<string>();
            var anagramToNodes = new Dictionary<string, List<Node>>();

            Node addWord(string line)
            {
                var splitLine = line.Split(' ');
                var word = splitLine[0];
                var logFrequency = float.Parse(splitLine[1]);

                var node = new Node(word, logFrequency);
                wordToNode[word] = node;
                shouldPrune.Add(word);
                if (AllowAnagrams)
                {
                    if (!anagramToNodes.ContainsKey(node.AnagramKey))
                    {
                        anagramToNodes[node.AnagramKey] = new();
                    }

                    anagramToNodes[node.AnagramKey].Add(node);
                }

                return node;
            }

            void enqueueNodeIfNeeded(Node node)
            {
                if (!alreadyExplored.Contains(node.Word))
                {
                    wordToNode[node.Word] = node;
                    alreadyExplored.Add(node.Word);
                    shouldPrune.Remove(node.Word);
                    exploreQueue.Enqueue(node);
                }
            }

            void tryConnect(Node node, string word)
            {
                var connectNode = Query(word);
                if (connectNode is null || connectNode == node)
                {
                    return;
                }

                node.Children.Add(connectNode);
                enqueueNodeIfNeeded(connectNode);
            }

            foreach (var line in WordList.text.Split('\n'))
            {
                var node = addWord(line);
                var word = node.Word;
                if (word.Length == StartingWordLength)
                {
                    enqueueNodeIfNeeded(node);
                }
            }

            Debug.Log($"Found {exploreQueue.Count} starting nodes.");

            while (exploreQueue.Count > 0)
            {
                var node = exploreQueue.Dequeue();
                var word = node.Word;

                // Additions
                for (var i = 0; i <= word.Length; i++)
                {
                    foreach (var letter in ALPHABET)
                    {
                        tryConnect(node, word[..i] + letter + word[i..]);
                    }
                }
                // Edits
                for (var i = 0; i < word.Length; i++)
                {
                    foreach (var letter in ALPHABET)
                    {
                        tryConnect(node, word[..i] + letter + word[(i + 1)..]);
                    }
                }
                if (AllowDeletions)
                {
                    for (var i = 0; i < word.Length; i++)
                    {
                        tryConnect(node, word[..i] + word[(i + 1)..]);
                    }
                }
                if (AllowAnagrams)
                {
                    foreach (var anagramNode in anagramToNodes[node.AnagramKey])
                    {
                        tryConnect(node, anagramNode.Word);
                    }
                }
            }

            Debug.Log(
                $"{alreadyExplored.Count} / {wordToNode.Count} nodes reachable. Pruning unreachable."
            );

            foreach (var word in shouldPrune)
            {
                wordToNode.Remove(word);
            }

            RegenerateValidStartNodes();

            Debug.Log($"{validStartNodes.Count} valid start nodes");
        }

        public void RegenerateValidStartNodes()
        {
            var weightedList = new List<WeightedListItem<Node>>();

            foreach (var node in wordToNode.Values)
            {
                if (node.Word.Length == StartingWordLength &&
                    node.LogFrequency >= StartingWordMinimumLogFrequency &&
                    node.Children.Count > 0)
                {
                    // WARN - you can change how this gets weighted but it may silently overflow
                    // and discard some items internally. Make sure cumulative weights are kept inbounds.

                    // To match actual real life occurrence rates the log frequency would need to be used as an exponent of 10.
                    // However, that causes the overflow mentioned above. Using a lower power lets unusual words occur but hopefully
                    // not too frequently
                    weightedList.Add(new(node, Mathf.RoundToInt(Mathf.Pow(2, node.LogFrequency))));
                }
            }

            validStartNodes = new(weightedList);
        }
    }
}
