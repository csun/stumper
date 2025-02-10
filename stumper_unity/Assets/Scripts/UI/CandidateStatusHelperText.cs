using TMPro;
using UnityEngine;

namespace Stumper
{
    internal class CandidateStatusHelperText : MonoBehaviour
    {
        public TMP_Text Text;

        public string AwaitingInputMessage;
        public string InvalidMessage;

        public GameManager Manager;

        void Start()
        {
            Manager.OnCandidateWordChanged += UpdateStatusText;
        }

        void UpdateStatusText()
        {
            if (Manager.CandidateWord.Length == 0)
            {
                Text.text = AwaitingInputMessage;
            }
            else if (Manager.CandidateStatus == GameManager.CandidateWordStatus.Invalid)
            {
                // TODO Add invalid reason to validity computation in manager as well
                Text.text = InvalidMessage;
            }
            else
            {
                Text.text = "";
            }
        }
    }
}
