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
        public RollInText StumpersLabel;
        public RollInText StumpersNumber;

        public GameManager Manager;

        void Start()
        {
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        void OnCurrentNodeChanged()
        {
            var valid = Manager.ValidMoves();
            var stumpers = 0;

            foreach (var node in valid)
            {
                if (Manager.ValidMoves(node).Count() == 0)
                {
                    stumpers++;
                }
            }

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

            if (stumpers == 0)
            {
                StumpersNumber.ChangeText("");
                StumpersLabel.ChangeText("");
            }
            else if (stumpers == 1)
            {
                StumpersNumber.ChangeText("1");
                StumpersLabel.ChangeText("Possible Stumper");
            }
            else
            {
                StumpersNumber.ChangeText(stumpers.ToString());
                StumpersLabel.ChangeText("Possible Stumpers");
            }
        }
    }
}
