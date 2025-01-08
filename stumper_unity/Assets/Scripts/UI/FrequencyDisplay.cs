using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Stumper
{
    internal class FrequencyDisplay : MonoBehaviour
    {
        public List<TMP_Text> Labels;
        public List<float> LowerBounds;
        public TMP_Text StumpersLabel;
        public TMP_Text CurrentWordDifficultyLabel;

        public GameManager Manager;

        void Start()
        {
            Assert.AreEqual(Labels.Count, LowerBounds.Count + 1);
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        int GetFrequencyLevel(Node node)
        {
            for (var i = 0; i < LowerBounds.Count; i++)
            {
                if (node.LogFrequency > LowerBounds[i])
                {
                    return i;
                }
            }
            return LowerBounds.Count;
        }

        void OnCurrentNodeChanged()
        {
            var valid = Manager.ValidMoves();
            var counts = new int[Labels.Count];
            var stumpers = 0;

            CurrentWordDifficultyLabel.text = $"Current word is {GetFrequencyLevel(Manager.CurrentNode)}";
            foreach (var node in valid)
            {
                if (Manager.ValidMoves(node).Count() == 0)
                {
                    stumpers++;
                }
                counts[GetFrequencyLevel(node)]++;
            }

            for (var i = 0; i < Labels.Count; i++)
            {
                Labels[i].text = counts[i].ToString();
            }
            
            if (stumpers == 1)
            {
                StumpersLabel.enabled = true;
                StumpersLabel.text = $"1 Possible Stumper!";
            }
            else if (stumpers > 1)
            {
                StumpersLabel.enabled = true;
                StumpersLabel.text = $"{stumpers} Possible Stumper!";
            }
            else
            {
                StumpersLabel.enabled = false;
            }
        }
    }
}
