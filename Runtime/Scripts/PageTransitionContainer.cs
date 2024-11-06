/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIPageNavigator.Runtime
{
    [Serializable]
    public class PageTransitionContainer
    {
        [SerializeField]
        private AnimationClip m_EnterAnimation;

        [SerializeField]
        private AnimationClip m_ExitAnimation;

        public AnimationClip EnterAnimation
        {
            get => m_EnterAnimation;
            set => m_EnterAnimation = value;
        }

        public AnimationClip ExitAnimation
        {
            get => m_ExitAnimation;
            set => m_ExitAnimation = value;
        }
    }
}