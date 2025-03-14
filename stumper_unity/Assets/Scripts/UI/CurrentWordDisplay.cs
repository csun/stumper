using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class CurrentWordDisplay : MonoBehaviour
    {
        enum Operation
        {
            Insert,
            Delete,
            Substitute
        }

        public GameManager Manager;
        public GameObject LetterPrefab;
        public Transform LetterParent;

        public int LetterSpacing;

        string currentWord;
        List<RollInText> letters = new();
        int nextAnimationIdx;
        int readyAnimationIdx;

        void Start()
        {
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        void OnCurrentNodeChanged()
        {
            StartCoroutine(AnimateChanges(Manager.CurrentNode.Word, nextAnimationIdx));
            nextAnimationIdx++;
        }

        IEnumerator AnimateChanges(string newWord, int index)
        {
            // Wait until it's our turn to animate in case we queue up a bunch of them
            while (index != readyAnimationIdx)
            {
                yield return null;
            }

            var targetPositions = new List<Vector3>();
            var letterOffset = -(newWord.Length / 2.0f) * LetterSpacing;
            for (var i = 0; i < newWord.Length; i++)
            {
                targetPositions.Add(new Vector3(i * LetterSpacing + letterOffset, 0, 0));
            }

            var newLetterObjects = new List<RollInText>();
            var targetLetters = new List<string>();
            var operations = FindEditOperations(currentWord, newWord);
            while (operations.Count > 0)
            {
                var op = operations.Pop();
                var idx = targetLetters.Count;
                switch (op)
                {
                    case Operation.Substitute:
                        newLetterObjects.Add(letters[idx]);
                        targetLetters.Add(newWord[idx].ToString());
                        break;
                    case Operation.Insert:
                        var go = Instantiate(LetterPrefab, LetterParent);
                        go.transform.localPosition = targetPositions[idx];
                        newLetterObjects.Add(go.GetComponent<RollInText>());
                        targetLetters.Add(newWord[idx].ToString());
                        break;
                    case Operation.Delete:
                        targetLetters.Add("");
                        break;
                }
            }

            if (newWord.Length < currentWord.Length)
            {
                // Animate letter roll outs first and then handle slides
                for (var i = 0; i < newLetterObjects.Count; i++)
                {
                    letters[i].ChangeText(targetLetters[i].ToString());
                }

                // TODO wait for text change to finish
                // TODO lerp to position
            }
            else if (newWord.Length > currentWord.Length)
            {
                // Animate slides first and then handle letter roll ins
            }
            else
            {
                // Should only need to do letter roll ins
            }

            // TODO delete empty letter objects
            currentWord = newWord;
            letters = newLetterObjects;
            readyAnimationIdx = index + 1;
        }

        // Copilot-generated translation of this python code https://github.com/athlohangade/minimum-edit-distance/blob/main/wagner_fischer.py
        // Wagner-fischer algorithm
        Stack<Operation> FindEditOperations(string sourceString, string targetString)
        {
            const int INS_COST = 2;
            const int DEL_COST = 3;
            const int SUB_COST = 1;

            int[,] dp = new int[targetString.Length + 1, sourceString.Length + 1];

            for (int i = 1; i <= targetString.Length; i++)
            {
                dp[i, 0] = dp[i - 1, 0] + INS_COST;
            }
            for (int j = 1; j <= sourceString.Length; j++)
            {
                dp[0, j] = dp[0, j - 1] + DEL_COST;
            }

            var operations = new Stack<Operation>();

            for (int i = 1; i <= targetString.Length; i++)
            {
                for (int j = 1; j <= sourceString.Length; j++)
                {
                    if (sourceString[j - 1] == targetString[i - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1];
                    }
                    else
                    {
                        dp[i, j] = Mathf.Min(dp[i - 1, j] + INS_COST,
                                             dp[i - 1, j - 1] + SUB_COST,
                                             dp[i, j - 1] + DEL_COST);
                    }
                }
            }

            int x = targetString.Length;
            int y = sourceString.Length;

            while (x != 0 && y != 0)
            {
                if (targetString[x - 1] == sourceString[y - 1])
                {
                    // Substitution with same letter is same as no change
                    operations.Push(Operation.Substitute);
                    x--;
                    y--;
                }
                else if (dp[x, y] == dp[x - 1, y - 1] + SUB_COST)
                {
                    operations.Push(Operation.Substitute);
                    x--;
                    y--;
                }
                else if (dp[x, y] == dp[x - 1, y] + INS_COST)
                {
                    operations.Push(Operation.Insert);
                    x--;
                }
                else
                {
                    operations.Push(Operation.Delete);
                    y--;
                }
            }

            while (y != 0)
            {
                operations.Push(Operation.Delete);
                y--;
            }

            while (x != 0)
            {
                operations.Push(Operation.Insert);
                x--;
            }

            return operations;
        }
    }
}
