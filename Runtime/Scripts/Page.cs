/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

using UnityEngine;

namespace UGUIPageNavigator.Runtime
{
    [DisallowMultipleComponent]
    public class Page : MonoBehaviour, IPageLifecycleEvent
    {
        [SerializeField]
        private PageTransitionContainer m_TransitionContainer;

        private string m_Path;

        private int m_SortingOrder;

        [SerializeField]
        private bool m_EnableBackdrop;

        [SerializeField]
        private PageBackdrop m_OverrideBackdrop;

        [SerializeField]
        private bool m_DontDestroyAfterPop;

        public string Path
        {
            get => m_Path;
            internal set => m_Path = value;
        }

        public int SortingOrder
        {
            get => m_SortingOrder;
            internal set
            {
                m_SortingOrder = value;
                UpdateSortingOrder();
            }
        }

        public bool DontDestroyAfterPop
        {
            get => m_DontDestroyAfterPop;
            set => m_DontDestroyAfterPop = value;
        }

        public PageTransitionContainer TransitionContainer => m_TransitionContainer;
        public bool EnableBackdrop => m_EnableBackdrop;
        public PageBackdrop OverrideBackdrop => m_OverrideBackdrop;


        public virtual void PageWillAppear()
        {
        }

        public virtual void PageDidAppear()
        {
        }

        public virtual void PageWillDisAppear()
        {
        }

        public virtual void PageDidDisAppear()
        {
        }

        public virtual void PageEnterCache()
        {
        }

        public virtual void PageExitCache()
        {
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

        private void UpdateSortingOrder()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = m_SortingOrder;
        }
    }
}