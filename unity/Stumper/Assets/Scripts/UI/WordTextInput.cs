using System;
using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class WordTextInput : MonoBehaviour
    {
        public GameManager Manager;
        public TMP_InputField Input;

        void Start()
        {
            Input.onSubmit.AddListener(OnTextSubmit);
        }

        void OnTextSubmit(string word)
        {
            Manager.PlayWord(word);
            Input.text = "";
            Input.ActivateInputField();
        }
    }
}
