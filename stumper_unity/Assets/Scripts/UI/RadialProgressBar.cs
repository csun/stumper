using UnityEngine;
using UnityEngine.UI;

namespace Stumper
{
    class RadialProgressBar : MonoBehaviour
    {
        public RawImage ProgressImage;
        private Material material;

        void Start()
        {
            material = new Material(ProgressImage.material);
            ProgressImage.material = material;
        }

        public void SetProgress(float progress)
        {
            material.SetFloat("_Progress", progress);
        }
    }
}