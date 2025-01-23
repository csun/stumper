using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

namespace Stumper
{
    public class RollInText : MonoBehaviour
    {
        public RectMask2D Mask;
        public TMP_Text MainText;
        public float RollDuration = 1.0f;

        [Tooltip("If set, text will roll out automatically even if there's nothing else queued")]
        public bool RollOutToEmpty;

        public float RollOutDelay;
        public AnimationCurve Curve;

        private Vector3 mainStartPosition;
        private TMP_Text NextText;
        private Queue<string> displayQueue = new();

        void Start()
        {
            mainStartPosition = MainText.rectTransform.position;

            NextText = Instantiate(MainText, MainText.transform.parent);
            NextText.enabled = false;
        }

        IEnumerator RollInNext()
        {
            NextText.text = displayQueue.Peek();
            NextText.enabled = true;

            var currentProgress = 0.0f;
            var offset = Mask.rectTransform.rect.height;
            var nextStartPosition = mainStartPosition + Vector3.up * offset;
            var mainEndPosition = mainStartPosition - Vector3.up * offset;

            while (currentProgress < 1)
            {
                currentProgress = Mathf.Min(currentProgress + (Time.deltaTime / RollDuration), 1);
                var curvedProgress = Curve.Evaluate(currentProgress);

                NextText.rectTransform.position = Vector3.Lerp(
                    nextStartPosition, mainStartPosition, curvedProgress);
                MainText.rectTransform.position = Vector3.Lerp(
                    mainStartPosition, mainEndPosition, curvedProgress);

                yield return null;
            }

            var nextNext = MainText;
            MainText = NextText;
            NextText = nextNext;
            NextText.enabled = false;

            // Ignore roll out delay if empty
            if (MainText.text != "")
            {
                yield return new WaitForSeconds(RollOutDelay);
            }

            // Only dequeue when done animating so that a call to ChangeText will not kick off a coroutine while
            // one is in progress.
            displayQueue.Dequeue();

            if (displayQueue.Count > 0)
            {
                StartCoroutine(RollInNext());
            }
            // If we don't have anything else coming in but are set to roll out to empty, queue.
            // We also need to check that we aren't already empty.
            else if (RollOutToEmpty && MainText.text != "")
            {
                ChangeText("");
            }
        }

        public void ChangeText(string text)
        {
            displayQueue.Enqueue(text);

            // Only start the coroutine if there is only one item in the queue - if there are
            // multiple, the coroutine will dispatch the next call itself
            if (displayQueue.Count == 1)
            {
                StartCoroutine(RollInNext());
            }
        }
    }
}