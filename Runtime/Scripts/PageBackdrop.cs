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

        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
            _parentTransform = parentTransform;
            _rectTransform.FillParent(_parentTransform);
            _canvasGroup.interactable = m_closeWhenClicked;
            gameObject.SetActive(false);
        }

        public virtual void Enter(bool animated)
        {
            gameObject.SetActive(true);

            if (animated)
            {
                StartCoroutine(FadeCanvasGroup(_canvasGroup, 0, 1, 0.3f));
            }
            else
            {
                _canvasGroup.alpha = 1;
            }
        }

        public virtual void Exit(bool animated)
        {
            if (animated)
            {
                StartCoroutine(FadeCanvasGroup(_canvasGroup, 1, 0, 0.3f, () => { gameObject.SetActive(false); }));
            }
            else
            {
                _canvasGroup.alpha = 0;
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
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
                yield return null;
            }

            cg.alpha = end;
            onComplete?.Invoke();
        }
    }
}