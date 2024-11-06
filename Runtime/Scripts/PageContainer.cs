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
        private static readonly List<PageContainer> Instances = new List<PageContainer>();

        [SerializeField]
        private string m_uniqueName = "Default";

        [SerializeField]
        private PageTransitionContainer m_TransitionContainer;

        [SerializeField]
        private List<PageContainerEventListener> m_eventListeners = new List<PageContainerEventListener>();

        [SerializeField]
        private bool m_AutoSoringOrder = true;

        [SerializeField]
        private int m_BaseSortingOrder = 10;

        [SerializeField]
        private int m_SortingOrderStep = 10;

        private readonly List<Page> m_Pages = new List<Page>();

        private readonly List<Page> m_DontDestroyPages = new List<Page>();

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

        public static PageContainer Get(string name)
        {
            return Instances.Find(container => container.m_uniqueName == name);
        }

        public static PageContainer Get(Transform transform)
        {
            return transform.GetComponentInParent<PageContainer>();
        }

        public async UniTask Push(string path, bool animated = true, Action<Page> onload = null)
        {
            await Push<Page>(path, animated, onload);
        }

        public async UniTask Push<T>(string path, bool animated = true, Action<T> onload = null) where T : Page
        {
            T page = null;
            GameObject pageObj = null;

            var cachePage = m_DontDestroyPages.Find(p => p.Path == path) as T;
            if (cachePage != null)
            {
                page = cachePage;
                pageObj = page.gameObject;
                page.ExitCache();
                m_DontDestroyPages.Remove(cachePage);
            }
            else
            {
                var prefab = Resources.Load<GameObject>(path);
                pageObj = Instantiate(prefab, transform);
                page = pageObj.GetComponent<T>();
            }

            if (page == null)
            {
                throw new Exception($"Page {path} must have a component of type {typeof(T).Name}");
            }

            var root = pageObj.transform.Find("Root");
            if (root == null)
            {
                throw new Exception($"Page {path} must have a child named Root");
            }

            (root as RectTransform).FillParent(pageObj.transform as RectTransform);

            pageObj.name = pageObj.name.Replace("(Clone)", "");

            PageBackdrop backdrop = null;
            if (page.EnableBackdrop)
            {
                var backdropObj = page.OverrideBackdrop != null
                    ? Instantiate(page.OverrideBackdrop.gameObject, pageObj.transform)
                    : Instantiate(Resources.Load<GameObject>("PageBackdrop"), pageObj.transform);
                backdropObj.name = "Backdrop";
                backdropObj.transform.SetSiblingIndex(0);
                backdrop = backdropObj.GetComponent<PageBackdrop>();
                if (backdrop == null)
                {
                    backdrop = backdropObj.AddComponent<PageBackdrop>();
                }

                backdrop.Setup(pageObj.transform as RectTransform);
            }

            page.Path = path;
            if (m_AutoSoringOrder)
            {
                var max = m_Pages.Count > 0 ? m_Pages[^1].SortingOrder : m_BaseSortingOrder;
                page.SortingOrder = max + m_SortingOrderStep;
            }

            m_Pages.Add(page);
            onload?.Invoke(page);

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
        }

        public async UniTask Pop(int count = 1, bool animated = true)
        {
            if (count > m_Pages.Count) return;

            var pages = m_Pages.GetRange(m_Pages.Count - count, count);
            m_Pages.RemoveRange(m_Pages.Count - count, count);

            var to = m_Pages.Count > 0 ? m_Pages[^1] : null;

            pages.Reverse();

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
            foreach (var t in m_eventListeners)
            {
                t.Will(PageOperation.Pop, page, to);
            }

            page.PageWillDisAppear();

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

            page.PageDidDisAppear();

            foreach (var t in m_eventListeners)
            {
                t.Did(PageOperation.Pop, page, to);
            }

            if (page.DontDestroyAfterPop)
            {
                page.EnterCache();
            }
            else
            {
                Destroy(page.gameObject);
            }
        }

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

            var animator = page.gameObject.GetComponent<Animator>();
            if (animator == null) animator = page.gameObject.AddComponent<Animator>();

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
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length), cancellationToken: this.destroyCancellationToken);
        }

        private AnimationClip FixClip(AnimationClip clip, RectTransform rectTransform)
        {
            // TODO: Implement this method
            return clip;
        }
    }
}