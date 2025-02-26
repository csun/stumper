using System.Collections;
using UnityEngine;

namespace Stumper
{
    internal class MenuAnimator : MonoBehaviour
    {
        public RectTransform ContentTransform;
        public float MenuContentYOffset;
        public AnimationCurve Curve;
        public float Duration;
        private IEnumerator currentCoroutine;
        private float currentMenuOpenProgress;
        private bool shouldOpenMenu;

        private float gameLocalY;

        void Start()
        {
            gameLocalY = ContentTransform.localPosition.y;
        }

        public void OpenMenu()
        {
            shouldOpenMenu = true;
            StartAnim();
        }

        public void CloseMenu()
        {
            shouldOpenMenu = false;
            StartAnim();
        }

        private void StartAnim()
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = AnimateMenuContent();
            StartCoroutine(currentCoroutine);
        }

        IEnumerator AnimateMenuContent()
        {
            while ((shouldOpenMenu && currentMenuOpenProgress < 1) || (!shouldOpenMenu && currentMenuOpenProgress > 0))
            {
                var deltaProgress = (Time.deltaTime / Duration) * (shouldOpenMenu ? 1 : -1);
                currentMenuOpenProgress = Mathf.Clamp(currentMenuOpenProgress + deltaProgress, 0, 1);
                ContentTransform.localPosition = new Vector2(
                    ContentTransform.localPosition.x,
                    Mathf.Lerp(gameLocalY, gameLocalY + MenuContentYOffset, Curve.Evaluate(currentMenuOpenProgress))
                );

                yield return null;
            }

            currentCoroutine = null;
        }
    }
}
