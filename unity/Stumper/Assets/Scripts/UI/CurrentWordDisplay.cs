using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class CurrentWordDisplay : MonoBehaviour
    {
        public TMP_Text Text;

        public GameManager Manager;

        void Start()
        {
            Manager.OnCurrentNodeChanged += OnCurrentNodeChanged;
        }

        void OnCurrentNodeChanged()
        {
            Text.text = Manager.CurrentNode.Word;
        }
    }
}
