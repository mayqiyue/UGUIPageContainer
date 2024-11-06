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
    public abstract class PageContainerEventListener : MonoBehaviour, IPageContainerEvent
    {
        public abstract void Will(PageOperation operation, Page from, Page to);

        public abstract void Did(PageOperation operation, Page from, Page to);
    }
}