using UnityEngine;

namespace Stumper
{
    public class CountsIndicator : MonoBehaviour
    {
        public string Singular;
        public string Plural;

        public RollInText NumberText;
        public RollInText LabelText;

        public void UpdateCount(int count)
        {
            LabelText.ChangeText(count == 1 ? Singular : Plural);
            NumberText.ChangeText(count.ToString());
        }
    }
}
