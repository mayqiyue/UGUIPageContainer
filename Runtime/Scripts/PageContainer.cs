/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUIPageNavigator.Runtime
{
    public class PageContainer : MonoBehaviour
    {
        public static List<PageContainer> Instances { get; internal set; } = new List<PageContainer>();

        [SerializeField]
        private string m_uniqueName = "Default";

        [SerializeField]
        private PageTransitionContainer m_TransitionContainer;

        [SerializeField]
        private List<PageContainerEventListener> m_eventListeners = new List<PageContainerEventListener>();

        [SerializeField] [Tooltip("If the page does not come with its own canvas, this canvas will be used.")]
        private GameObject m_UnderlyingCanvas;

        [SerializeField] [Tooltip("If your canvas mode is ScreenSpace - Camera, you can set the camera here.")]
        private Camera m_CanvasCamera;

        [SerializeField]
        private bool m_AutoSoringOrder = true;

        [SerializeField]
        private int m_BaseSortingOrder = 10;

        [SerializeField]
        private int m_SortingOrderStep = 10;

        private readonly PageAssetLoader m_PageAssetLoader = new PageAssetLoader();

        private readonly List<Page> m_Pages = new List<Page>();

        private readonly List<Page> m_DontDestroyPages = new List<Page>();

        private readonly List<string> m_PushingPaths = new List<string>();

        public List<Page> Pages => m_Pages;

        private void Awake()
        {
            if (m_TransitionContainer.EnterAnimation == null)
            {
                m_TransitionContainer.EnterAnimation = Resources.Load<AnimationClip>("PageEnter");
            }

            if (m_TransitionContainer.ExitAnimation == null)
            {
                m_TransitionContainer.ExitAnimation = Resources.Load<AnimationClip>("PageExit");
            }

            Instances.Add(this);
        }

        private void OnDestroy()
        {
            m_PageAssetLoader.ReleaseAll();
            Instances.Remove(this);
        }

        public static PageContainer Get(string name)
        {
            return Instances.Find(container => container.m_uniqueName == name);
        }

        public static PageContainer Get(Transform transform)
        {
            return transform.GetComponentInParent<PageContainer>();
        }

        public Page TopPage => m_Pages.Count > 0 ? m_Pages[^1] : null;

        #region Push

        public async UniTask Push(string path, bool animated = true, Action<Page> onLoad = null)
        {
            await Push<Page>(path, animated, onLoad);
        }

        public async UniTask Push<T>(string path, bool animated = true, Action<T> onLoad = null) where T : Page
        {
            if (m_PushingPaths.Contains(path)) return;
            m_PushingPaths.Add(path);

            T page = null;
            var cachePage = m_DontDestroyPages.Find(p => p.Path == path) as T;
            if (cachePage != null)
            {
                page = cachePage;
                page.ExitCache();
                m_DontDestroyPages.Remove(cachePage);
            }
            else
            {
                var prefab = await m_PageAssetLoader.LoadAsync<GameObject>(path);
                var pageObj = Instantiate(prefab, transform);
                page = pageObj.GetComponent<T>();
                if (page == null)
                {
                    m_PushingPaths.RemoveAll(x => x == path);
                    throw new Exception($"Page {path} must have a component of type {typeof(T).Name}");
                }

                pageObj.name = pageObj.name.Replace("(Clone)", "");
            }

            page.IsInTransition = true;

            int? sortingOrder = null;
            if (m_AutoSoringOrder)
            {
                sortingOrder = m_Pages.Count > 0 ? (TopPage.SortingOrder + m_SortingOrderStep) : m_BaseSortingOrder;
            }

            page.Config(path, sortingOrder);
            page.Load(m_UnderlyingCanvas, m_CanvasCamera);
            onLoad?.Invoke(page);

            m_Pages.Add(page);

            // setup backdrop
            PageBackdrop backdrop = null;
            if (page.EnableBackdrop && page.PageObject.transform.Find("Backdrop") == null)
            {
                var backdropObj = page.OverrideBackdrop != null
                    ? Instantiate(page.OverrideBackdrop.gameObject, page.PageObject.transform)
                    : Instantiate(Resources.Load<GameObject>("PageBackdrop"), page.PageObject.transform);
                backdropObj.name = "Backdrop";
                backdropObj.transform.SetSiblingIndex(0);
                backdrop = backdropObj.GetComponent<PageBackdrop>();
                if (backdrop == null)
                {
                    backdrop = backdropObj.AddComponent<PageBackdrop>();
                }

                backdrop.Setup(page.PageObject.transform as RectTransform);
            }

            // start animation
            page.PageWillAppear();

            foreach (var t in m_eventListeners)
            {
                t.Will(PageOperation.Push, m_Pages.Count >= 2 ? m_Pages[^2] : null, page);
            }

            backdrop?.Enter(animated);
            if (animated)
            {
                await HandlePageTransition(page, PageOperation.Push).SuppressCancellationThrow();
            }

            page.PageDidAppear();

            foreach (var t in m_eventListeners)
            {
                t.Did(PageOperation.Push, m_Pages.Count >= 2 ? m_Pages[^2] : null, page);
            }

            page.IsInTransition = false;
            m_PushingPaths.RemoveAll(x => x == path);
        }

        #endregion

        #region Pop

        public async UniTask PopToRoot(bool animated = true)
        {
            await Pop(m_Pages.Count, animated);
        }

        public async UniTask Pop(int count = 1, bool animated = true)
        {
            if (count > m_Pages.Count) return;

            var pages = m_Pages.GetRange(m_Pages.Count - count, count);
            var to = m_Pages.Count > 0 ? m_Pages[^1] : null;

            await UniTask.WhenAll(pages.Select(page => Pop(page, to, animated))).SuppressCancellationThrow();
        }

        public async UniTask PopTo(string path, bool animated = true)
        {
            var index = m_Pages.FindIndex(page => page.Path == path);
            if (index == -1) return;

            var count = m_Pages.Count - index - 1;

            await Pop(count, animated);
        }

        public async UniTask PopTo<T>(T page, bool animated = true) where T : Page
        {
            await PopTo(page.Path, animated);
        }

        private async UniTask Pop(Page page, Page to, bool animated = true)
        {
            if (page.IsInTransition) return;
            page.IsInTransition = true;

            foreach (var t in m_eventListeners)
            {
                t.Will(PageOperation.Pop, page, to);
            }

            page.PageWillDisappear();

            if (page.EnableBackdrop)
            {
                var backdrop = page.transform.Find("Backdrop")?.GetComponent<PageBackdrop>();
                if (backdrop != null)
                {
                    backdrop.Exit(animated);
                }
            }

            if (animated)
            {
                await HandlePageTransition(page, PageOperation.Pop);
            }

            page.PageDidDisappear();

            foreach (var t in m_eventListeners)
            {
                t.Did(PageOperation.Pop, page, to);
            }

            page.IsInTransition = false;

            m_Pages.Remove(page);

            if (page.DontDestroyAfterPop)
            {
                page.EnterCache();
            }
            else
            {
                m_PageAssetLoader.Release(page.Path);
                Destroy(page.PageObject);
            }
        }

        #endregion

        #region Transition

        private async UniTask HandlePageTransition(Page page, PageOperation operation)
        {
            AnimationClip clip = null;
            if (operation == PageOperation.Push)
            {
                clip = page.TransitionContainer.EnterAnimation;
                if (clip == null)
                {
                    clip = m_TransitionContainer.EnterAnimation;
                }
            }
            else
            {
                clip = page.TransitionContainer.ExitAnimation;
                if (clip == null)
                {
                    clip = m_TransitionContainer.ExitAnimation;
                }
            }

            if (clip == null)
            {
                throw new Exception($"AnimationClip for {operation} is not set");
            }

            clip = FixClip(clip, page.transform as RectTransform);

            var animator = page.PageObject.GetComponent<Animator>();
            if (animator == null) animator = page.PageObject.AddComponent<Animator>();
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            var runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Page");

            var overrideController = new AnimatorOverrideController
            {
                runtimeAnimatorController = runtimeAnimatorController
            };

            var name = operation == PageOperation.Push ? "Enter" : "Exit";
            foreach (var animationClip in runtimeAnimatorController.animationClips)
            {
                overrideController[animationClip.name] = clip;
            }

            animator.runtimeAnimatorController = overrideController;
            animator.Play(name);
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length), delayType: DelayType.UnscaledDeltaTime, cancellationToken: this.destroyCancellationToken);
        }

        private AnimationClip FixClip(AnimationClip clip, RectTransform rectTransform)
        {
            // TODO: Implement this method
            return clip;
        }

        #endregion
    }
}