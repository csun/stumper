using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace Stumper
{
    // Most logic copied from Unity UI AspectRatioFitter
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class WidthConstriner : UIBehaviour, ILayoutSelfController
    {
        public Vector2 WidthRange;

        [System.NonSerialized]
        private RectTransform m_Rect;

        // This "delayed" mechanism is required for case 1014834.
        private bool m_DelayedSetDirty = false;

        //Does the gameobject has a parent for reference to enable FitToParent/EnvelopeParent modes.
        private bool m_DoesParentExist = false;

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        // field is never assigned warning
#pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
#pragma warning restore 649

        protected override void OnEnable()
        {
            base.OnEnable();
            m_DoesParentExist = rectTransform.parent ? true : false;
            SetDirty();
        }

        protected override void Start()
        {
            base.Start();
            //Disable the component if the aspect mode is not valid or the object state/setup is not supported with AspectRatio setup.
            if (!IsComponentValidOnObject())
                this.enabled = false;
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            m_DoesParentExist = rectTransform.parent ? true : false;
            SetDirty();
        }

        /// <summary>
        /// Update the rect based on the delayed dirty.
        /// Got around issue of calling onValidate from OnEnable function.
        /// </summary>
        protected virtual void Update()
        {
            if (m_DelayedSetDirty)
            {
                m_DelayedSetDirty = false;
                SetDirty();
            }
        }

        /// <summary>
        /// Function called when this RectTransform or parent RectTransform has changed dimensions.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        private void UpdateRect()
        {
            if (!IsActive() || !IsComponentValidOnObject())
                return;

            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);

            var targetWidth = Mathf.Clamp(GetParentSize().x, WidthRange.x, WidthRange.y);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }

        private Vector2 GetParentSize()
        {
            RectTransform parent = rectTransform.parent as RectTransform;
            return !parent ? Vector2.zero : parent.rect.size;
        }

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutHorizontal() { }

        /// <summary>
        /// Method called by the layout system. Has no effect
        /// </summary>
        public virtual void SetLayoutVertical() { }

        /// <summary>
        /// Mark the AspectRatioFitter as dirty.
        /// </summary>
        protected void SetDirty()
        {
            UpdateRect();
        }

        public bool IsComponentValidOnObject()
        {
            Canvas canvas = gameObject.GetComponent<Canvas>();
            if (canvas && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace)
            {
                return false;
            }
            return true;
        }

        private bool DoesParentExists()
        {
            return m_DoesParentExist;
        }
    }
}
