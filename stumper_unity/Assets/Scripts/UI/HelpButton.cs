using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class HelpButton : MonoBehaviour
    {
        public GameManager Manager;
        public TMP_Text Text;

        void Start()
        {
            Manager.OnMenuStateChanged += OnMenuStateChanged;
        }

        public void OnClick()
        {
            // Don't want to be able to close game summary using this button
            if (Manager.CurrentMenuState != GameManager.MenuState.Gameplay)
            {
                return;
            }

            Manager.OpenHelp();
        }

        public void OnMenuStateChanged()
        {
            if (Manager.CurrentMenuState == GameManager.MenuState.Gameplay)
            {
                Text.text = "?";
            }
            else
            {
                Text.text = "";
            }
        }
    }
}
