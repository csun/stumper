using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Stumper
{
    public class SimpleButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        public UnityEvent Action;

        public TMPro.TMP_Text Text;
        public Image Background;

        public Color ActiveHighlightColor;
        public Color ActiveUnHighlightColor;
        public Color BackgroundHighlightColor;
        public Color BackgroundUnHighlightColor;

        private bool mouseInside;

        public void OnPointerClick(PointerEventData eventData)
        {
            Action.Invoke();
            mouseInside = false;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseInside = true;
            Refresh();
        }

        public virtual void Refresh()
        {
            if (mouseInside)
            {
                Highlight();
            }
            else
            {
                UnHighlight();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseInside = false;
            Refresh();
        }

        public void Highlight()
        {
            Text.color = ActiveHighlightColor;
            Background.color = BackgroundHighlightColor;
        }

        public void UnHighlight()
        {
            Text.color = ActiveUnHighlightColor;
            Background.color = BackgroundUnHighlightColor;
        }
    }
}
