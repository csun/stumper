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
        public RollInText PossibilitiesLabel;
        public RollInText PossibilitiesNumber;

        public GameManager Manager;

        void Start()
        {
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        void OnCurrentNodeChanged()
        {
            var valid = Manager.ValidMoves();
            var stumpers = 0;

            if (valid.Count() == 1)
            {
                PossibilitiesNumber.ChangeText("1");
                PossibilitiesLabel.ChangeText("Possibility");
            }
            else
            {
                PossibilitiesNumber.ChangeText(valid.Count().ToString());
                PossibilitiesLabel.ChangeText("Possibilities");
            }

            foreach (var node in valid)
            {
                if (Manager.ValidMoves(node).Count() == 0)
                {
                    stumpers++;
                }
            }
        }
    }
}
