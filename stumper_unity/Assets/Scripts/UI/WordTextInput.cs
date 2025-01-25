using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Stumper
{
    internal class WordTextInput : MonoBehaviour
    {
        public InputSystemUIInputModule InputModule;
        public GameManager Manager;
        public TMP_Text Display;

        void Start()
        {
            var kbd = Keyboard.current;
            kbd.onTextInput += OnTextInput;

            // Unity doesn't handle these keys gracefully in webgl builds so go through the input system instead.
            InputModule.actionsAsset["Stumper/SubmitWord"].performed += _ => Manager.SubmitWord();
            InputModule.actionsAsset["Stumper/SubmitWord"].Enable();

            InputModule.actionsAsset["Stumper/Backspace"].performed += _ => Manager.HandleBackspacePressed();
            InputModule.actionsAsset["Stumper/Backspace"].Enable();

            Manager.OnCandidateWordChanged += OnCandidateWordChanged;
        }

        private void OnCandidateWordChanged()
        {
            Display.text = Manager.CandidateWord;
        }

        private void OnTextInput(char c)
        {
            if (char.IsLetter(c))
            {
                Manager.HandleLetterPressed(c);
            }
        }
    }
}
