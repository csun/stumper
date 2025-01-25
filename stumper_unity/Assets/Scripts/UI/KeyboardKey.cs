using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Stumper
{
    internal class KeyboardKey : MonoBehaviour
    {
        public enum KeyMode
        {
            Letter,
            Backspace,
            Enter,
        }

        public KeyMode Mode;
        public TMP_Text Text;
        public GameManager Manager;

        public void OnClick()
        {
            switch (Mode)
            {
                case KeyMode.Letter:
                    Manager.HandleLetterPressed(Text.text[0]);
                    break;
                case KeyMode.Backspace:
                    Manager.HandleBackspacePressed();
                    break;
                case KeyMode.Enter:
                    Manager.SubmitWord();
                    break;
            }
        }
    }
}