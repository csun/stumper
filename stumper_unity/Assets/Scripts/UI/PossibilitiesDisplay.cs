using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Stumper
{
    internal class PossibilitiesDisplay : MonoBehaviour
    {
        public CountsIndicator Edits;
        public CountsIndicator Extensions;
        public CountsIndicator Anagrams;
        public CountsIndicator Stumpers;

        public GameManager Manager;

        void Start()
        {
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        void OnCurrentNodeChanged()
        {
            var current = Manager.CurrentNode;
            var valid = Manager.ValidMoves();
            var edits = 0;
            var anagrams = 0;
            var extensions = 0;
            var stumpers = 0;

            foreach (var node in valid)
            {
                if (current.AnagramKey == node.AnagramKey)
                {
                    anagrams++;
                }
                else if (current.Word.Length == node.Word.Length)
                {
                    edits++;
                }
                else
                {
                    extensions++;
                }

                if (Manager.ValidMoves(node).Count() == 0)
                {
                    stumpers++;
                }
            }

            Anagrams.UpdateCount(anagrams);
            Edits.UpdateCount(edits);
            Extensions.UpdateCount(extensions);
            Stumpers.UpdateCount(stumpers);
        }
    }
}
