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

            // This may not be necessary because OnTextInput gets keycodes for enter, but I
            // was reading that some keyboards send two keycodes (CR and LF) for enter, so I
            // don't want it to trigger twice.
            InputModule.actionsAsset["Stumper/SubmitWord"].performed += _ => Manager.SubmitWord();
            InputModule.actionsAsset["Stumper/SubmitWord"].Enable();

            Manager.OnCandidateWordChanged += OnCandidateWordChanged;
        }

        private void OnCandidateWordChanged()
        {
            Display.text = Manager.CandidateWord;
        }

        private void OnTextInput(char c)
        {
            if (c == '\b')
            {
                Manager.HandleBackspacePressed();
            }
            else if (char.IsLetter(c))
            {
                Manager.HandleLetterPressed(c);
            }
        }
    }
}
