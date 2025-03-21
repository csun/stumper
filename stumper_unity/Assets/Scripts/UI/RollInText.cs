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
        [Tooltip("If set, cancels existing queue and forces immediate animation start when new thing is queued")]
        public bool ForceStart;

        public bool RollInFromBottom;
        [Tooltip("If set, text will roll out automatically even if there's nothing else queued")]
        public bool RollOutToEmpty;

        [Tooltip("If set, text will alternate roll directions")]
        public bool FlipFlop;
        [Tooltip("If set, keeps requeueing elements after they're shown. You can use overrideForce on your ChangeText call to interrupt this.")]
        public bool Loop;

        public float RollOutDelay;
        public float RollOutDelayVariation = 0;
        public AnimationCurve Curve;

        private Vector3 mainStartPosition;
        private TMP_Text nextText;
        private Queue<string> displayQueue = new();
        private int nextAnimId = 0;
        private int currentAnim = 0;
        private int skipToAnim = 0;

        void Awake()
        {
            mainStartPosition = MainText.rectTransform.localPosition;

            nextText = Instantiate(MainText, MainText.transform.parent);
            nextText.enabled = false;
        }

        IEnumerator RollInNext(int id)
        {
            while (id != currentAnim)
            {
                yield return null;
            }

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
                if (skipToAnim > id)
                {
                    break;
                }

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
                var endTime = Time.time + RollOutDelay + UnityEngine.Random.Range(-RollOutDelayVariation, RollOutDelayVariation);
                while (Time.time < endTime)
                {
                    if (skipToAnim > id)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            if (FlipFlop)
            {
                RollInFromBottom = !RollInFromBottom;
            }

            var text = displayQueue.Dequeue();
            // Don't loop if there's only one entry. It looks dumb.
            if (Loop && displayQueue.Count > 0)
            {
                ChangeText(text);
            }

            currentAnim++;

            // If we don't have anything else coming in but are set to roll out to empty, queue.
            // We also need to check that we aren't already empty.
            if (displayQueue.Count == 0 && RollOutToEmpty && MainText.text != "")
            {
                ChangeText("");
            }
        }

        public void ChangeText(string text)
        {
            displayQueue.Enqueue(text);
            if (ForceStart)
            {
                skipToAnim = nextAnimId;
            }
            StartCoroutine(RollInNext(nextAnimId++));
        }

        public void ChangeNumericalValue(int newVal, int oldVal)
        {
            var diff = newVal - oldVal;
            if (diff == 0)
            {
                return;
            }

            var diffString = $"{(diff > 0 ? "+" : "")}{diff}";

            if (ForceStart)
            {
                skipToAnim = nextAnimId;
            }
            displayQueue.Enqueue(diffString);
            displayQueue.Enqueue(newVal.ToString());
            StartCoroutine(RollInNext(nextAnimId++));
            StartCoroutine(RollInNext(nextAnimId++));
        }
    }
}