/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

namespace UGUIPageNavigator.Runtime
{
    public interface IPageLifecycleEvent
    {
        /// <summary>
        /// 
        /// </summary>
        void PageDidLoad();

        /// <summary>
        /// You can do some initialization operations here
        /// </summary>
        void PageWillAppear();


        /// <summary>
        /// You can do some animations that users can see here
        /// </summary>
        void PageDidAppear();

        /// <summary>
        /// You can do some exit animations here
        /// </summary>
        void PageWillDisappear();

        /// <summary>
        /// You can do some resource destruction operations
        /// </summary>
        void PageDidDisappear();
    }
}