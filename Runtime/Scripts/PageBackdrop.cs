/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIPageNavigator.Runtime
{
    public class PageBackdrop : MonoBehaviour
    {
        [SerializeField]
        private bool m_closeWhenClicked;

        private CanvasGroup m_CanvasGroup;
        private RectTransform m_ParentTransform;
        private RectTransform m_RectTransform;

        private void Awake()
        {
            m_RectTransform = (RectTransform)transform;
            m_CanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (m_closeWhenClicked)
            {
                if (!TryGetComponent<Image>(out var image))
                {
                    image = gameObject.AddComponent<Image>();
                    image.color = Color.clear;
                }

                if (!TryGetComponent<Button>(out var button))
                {
                    button = gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                }

                button.onClick.AddListener(() =>
                {
                    var modalContainer = PageContainer.Get(transform);
                    modalContainer.Pop().Forget();
                });
            }
        }

        public void Setup(RectTransform parentTransform)
        {
            m_ParentTransform = parentTransform;
            m_RectTransform.FillParent(m_ParentTransform);
            m_RectTransform.localEulerAngles = Vector3.zero;
            m_CanvasGroup.interactable = m_closeWhenClicked;
            gameObject.SetActive(false);
        }

        public virtual void Enter(bool animated)
        {
            gameObject.SetActive(true);

            if (animated)
            {
                StartCoroutine(FadeCanvasGroup(m_CanvasGroup, 0, 1, 0.3f));
            }
            else
            {
                m_CanvasGroup.alpha = 1;
            }
        }

        public virtual void Exit(bool animated)
        {
            if (animated)
            {
                StartCoroutine(FadeCanvasGroup(m_CanvasGroup, 1, 0, 0.3f, () => { gameObject.SetActive(false); }));
            }
            else
            {
                m_CanvasGroup.alpha = 0;
                gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration, Action onComplete = null)
        {
            var elapsed = 0f;
            cg.alpha = start;
            cg.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            cg.alpha = end;
            onComplete?.Invoke();
        }
    }
}