using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;

namespace Stumper
{
    public class RollInText : MonoBehaviour
    {
        public RectMask2D Mask;
        public TMP_Text MainText;
        public float RollDuration = 1.0f;
        public float RollDurationVariation = 0;

        public bool RollInFromBottom;
        [Tooltip("If set, text will roll out automatically even if there's nothing else queued")]
        public bool RollOutToEmpty;

        [Tooltip("If set, text will alternate roll directions")]
        public bool FlipFlop;

        public float RollOutDelay;
        public float RollOutDelayVariation = 0;
        public AnimationCurve Curve;

        private Vector3 mainStartPosition;
        private TMP_Text nextText;
        private Queue<string> displayQueue = new();
        public bool IsAnimating => displayQueue.Count > 0;

        void Start()
        {
            mainStartPosition = MainText.rectTransform.localPosition;

            nextText = Instantiate(MainText, MainText.transform.parent);
            nextText.enabled = false;
        }

        IEnumerator RollInNext()
        {
            nextText.text = displayQueue.Peek();
            nextText.enabled = true;

            var currentProgress = 0.0f;
            var offset = Mask.rectTransform.rect.height;
            var nextStartPosition = mainStartPosition + Vector3.up * offset;
            var mainEndPosition = mainStartPosition - Vector3.up * offset;
            var chosenDuration = RollDuration + UnityEngine.Random.Range(-RollDurationVariation, RollDurationVariation);

            if (RollInFromBottom)
            {
                var tmp = nextStartPosition;
                nextStartPosition = mainEndPosition;
                mainEndPosition = tmp;
            }

            while (currentProgress < 1)
            {
                currentProgress = Mathf.Min(currentProgress + (Time.deltaTime / chosenDuration), 1);
                var curvedProgress = Curve.Evaluate(currentProgress);

                nextText.rectTransform.localPosition = Vector3.Lerp(
                    nextStartPosition, mainStartPosition, curvedProgress);
                MainText.rectTransform.localPosition = Vector3.Lerp(
                    mainStartPosition, mainEndPosition, curvedProgress);

                yield return null;
            }

            var nextNext = MainText;
            MainText = nextText;
            nextText = nextNext;
            nextText.enabled = false;

            // Ignore roll out delay if empty
            if (MainText.text != "")
            {
                yield return new WaitForSeconds(
                    RollOutDelay + UnityEngine.Random.Range(-RollOutDelayVariation, RollOutDelayVariation));
            }

            // Only dequeue when done animating so that a call to ChangeText will not kick off a coroutine while
            // one is in progress.
            displayQueue.Dequeue();
            if (FlipFlop)
            {
                RollInFromBottom = !RollInFromBottom;
            }

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

        public void ChangeNumericalValue(int newVal)
        {
            var currVal = 0;
            Int32.TryParse(MainText.text, out currVal);

            var diff = newVal - currVal;
            if (diff == 0)
            {
                return;
            }

            var diffString = $"{(diff > 0 ? "+" : "")}{diff}";
            ChangeText(diffString);
            ChangeText(newVal.ToString());
        }
    }
}