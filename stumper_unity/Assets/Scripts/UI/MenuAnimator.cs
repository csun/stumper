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
        public float GameEndDelay;
        private IEnumerator currentCoroutine;
        private float currentMenuOpenProgress;
        private bool shouldOpenMenu;

        public GameObject MenuContent;
        public GameObject SummaryContent;
        public GameObject HelpContent;

        public GameSummary SummaryManager;

        private float gameLocalY;

        void Start()
        {
            gameLocalY = ContentTransform.localPosition.y;
        }

        public void OpenSummary(bool shouldDelay)
        {
            StartCoroutine(SummaryDelay(shouldDelay));
        }

        IEnumerator SummaryDelay(bool shouldDelay)
        {
            if (shouldDelay)
            {
                yield return new WaitForSeconds(GameEndDelay);
            }
            ShowContent(SummaryContent);
            SummaryManager.RefreshStats();
        }

        public void OpenHelp()
        {
            ShowContent(HelpContent);
        }

        public void OpenMenu()
        {
            ShowContent(MenuContent);
        }

        public void CloseMenu()
        {
            shouldOpenMenu = false;
            StartAnim();
        }

        public void LinkToWebsite()
        {
            Application.OpenURL("https://www.csun.io/");
        }

        private void ShowContent(GameObject content)
        {
            void ChangeActiveState(GameObject obj)
            {
                obj.SetActive(obj == content);
            }

            ChangeActiveState(MenuContent);
            ChangeActiveState(SummaryContent);
            ChangeActiveState(HelpContent);

            shouldOpenMenu = true;
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
