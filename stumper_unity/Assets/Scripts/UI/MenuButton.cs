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
            if (Manager.CurrentMenuState == GameManager.MenuState.Gameplay)
            {
                // Show the menu button
                Text.text = "a";
            }
            else if (Manager.CurrentMenuState == GameManager.MenuState.GameSummary)
            {
                // Want to force using other menu buttons to leave summary screen
                Text.text = "";
            }
            else
            {
                // Show the X button for all other menus except for the game summary
                Text.text = "b";
            }
        }
    }
}
