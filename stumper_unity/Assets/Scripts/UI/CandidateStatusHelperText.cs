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
            Manager.OnUpdateInfoMessage += (message) => { if (message != "") { Text.text = message; } };
        }

        void UpdateStatusText()
        {
            if (Manager.CandidateWord.Length == 0)
            {
                Text.text = AwaitingInputMessage;
            }
            else if (Manager.CandidateStatus == GameManager.CandidateWordStatus.Invalid)
            {
                Text.text = $"{InvalidMessage}\n{Manager.CandidateInvalidReason}";
            }
            else
            {
                Text.text = "";
            }
        }
    }
}
