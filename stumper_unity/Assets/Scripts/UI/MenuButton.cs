using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class MenuButton : MonoBehaviour
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
            if (Manager.CurrentMenuState == GameManager.MenuState.GameSummary)
            {
                return;
            }

            Manager.ToggleMenu();
        }

        public void OnMenuStateChanged()
        {
            if (Manager.CurrentMenuState == GameManager.MenuState.Menu)
            {
                // Show the X button
                Text.text = "b";
            }
            else if (Manager.CurrentMenuState == GameManager.MenuState.Gameplay)
            {
                // Show the menu button
                Text.text = "a";
            }
            else
            {
                Text.text = "";
            }
        }
    }
}
