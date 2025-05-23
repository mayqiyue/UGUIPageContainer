/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

using System;
using UnityEngine;

namespace UGUIPageNavigator.Runtime
{
    /// <summary>
    /// 一个page的正确结构是这样的：Canvas -> Root -> Content -> 其他UI元素   翻译成英文
    /// </summary>
    [DisallowMultipleComponent]
    public class Page : MonoBehaviour, IPageLifecycleEvent
    {
        private PageAssetInfo m_Path;

        private int m_SortingOrder;

        [SerializeField]
        private PageTransitionContainer m_TransitionContainer;

        [SerializeField]
        private bool m_EnableBackdrop;

        [SerializeField]
        private PageBackdrop m_OverrideBackdrop;

        [SerializeField]
        private bool m_DontDestroyAfterPop;

        private GameObject m_PageObject;

        private RectTransform m_Root;

        private Canvas m_PageCanvas;

        internal bool IsInTransition = false;

        public bool DontDestroyAfterPop
        {
            get => m_DontDestroyAfterPop;
            set => m_DontDestroyAfterPop = value;
        }

        public bool EnableBackdrop
        {
            get => m_EnableBackdrop;
            set => m_EnableBackdrop = value;
        }

        public PageBackdrop OverrideBackdrop
        {
            get => m_OverrideBackdrop;
            set => m_OverrideBackdrop = value;
        }

        public PageAssetInfo Path => m_Path;
        public int SortingOrder => m_SortingOrder;
        public PageTransitionContainer TransitionContainer => m_TransitionContainer;

        public RectTransform Root => m_Root;
        public GameObject PageObject => m_PageObject;
        public Canvas PageCanvas => m_PageCanvas;

        internal void Config(PageAssetInfo path, int? sortingOrder)
        {
            m_Path = path;
            if (sortingOrder.HasValue)
            {
                m_SortingOrder = sortingOrder.Value;
            }
        }

        internal void Load(GameObject underlyingCanvas, Camera canvasCamera)
        {
            CheckCanvas(underlyingCanvas, canvasCamera);
            CheckRoot();
            CheckSortingOrder();
            PageDidLoad();
        }

        internal void UpdateCanvasCamera(Camera camera)
        {
            if (m_PageCanvas == null) return;
            CheckCamera(m_PageCanvas, camera);
        }

        internal void EnterCache()
        {
            gameObject.SetActive(false);
            PageEnterCache();
        }

        internal void ExitCache()
        {
            gameObject.SetActive(true);
            PageExitCache();
        }

        private void CheckCanvas(GameObject underlyingCanvas, Camera canvasCamera)
        {
            var rootCanvas = gameObject.GetComponent<Canvas>();
            if (rootCanvas != null)
            {
                m_PageObject = gameObject;
                m_PageCanvas = rootCanvas;
                CheckCamera(rootCanvas, canvasCamera);
                return;
            }

            rootCanvas = gameObject.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                m_PageObject = rootCanvas.gameObject;
                m_PageCanvas = rootCanvas;
                CheckCamera(rootCanvas, canvasCamera);
                return;
            }

            if (underlyingCanvas == null)
            {
                throw new Exception("Page must be under a canvas");
            }

            var pageObj = Instantiate(underlyingCanvas, transform.parent);
            pageObj.name = gameObject.name;
            m_PageObject = pageObj;

            rootCanvas = pageObj.GetComponent<Canvas>();
            m_PageCanvas = rootCanvas;
            CheckCamera(rootCanvas, canvasCamera);

            transform.SetParent(pageObj.transform);
            ((RectTransform)transform).FillParent(pageObj.transform as RectTransform);
            ((RectTransform)transform).localEulerAngles = Vector3.zero;
            gameObject.name = RootName;
        }

        private void CheckRoot()
        {
            if (transform.name == RootName) return;

            var root = transform.Find(RootName);
            if (root == null)
            {
                var rootObj = new GameObject("Root");
                var rootRect = rootObj.AddComponent<RectTransform>();
                rootRect.SetParent(transform);
                rootRect.FillParent(transform as RectTransform);
                rootRect.localEulerAngles = Vector3.zero;
                root = rootRect;

                foreach (Transform child in transform)
                {
                    if (child == root) continue;
                    child.SetParent(root);
                    ((RectTransform)child).localEulerAngles = Vector3.zero;
                }
            }
            else
            {
                ((RectTransform)root).FillParent(transform as RectTransform);
                ((RectTransform)root).localEulerAngles = Vector3.zero;
            }


            m_Root = root as RectTransform;
        }

        private void CheckCamera(Canvas canvas, Camera canvasCamera)
        {
            if (canvasCamera == null) return;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = canvasCamera;
        }

        private void CheckSortingOrder()
        {
            var canvas = m_PageObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = m_SortingOrder;
        }


        private string RootName => "Root";

        #region IPageLifecycleEvent

        public virtual void PageDidLoad()
        {
        }

        public virtual void PageWillAppear()
        {
        }

        public virtual void PageDidAppear()
        {
        }

        public virtual void PageWillDisappear()
        {
        }

        public virtual void PageDidDisappear()
        {
        }

        public virtual void PageEnterCache()
        {
        }

        public virtual void PageExitCache()
        {
        }

        #endregion
    }
}