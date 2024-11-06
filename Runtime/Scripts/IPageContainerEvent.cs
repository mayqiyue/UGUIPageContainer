/*
 * Author: float
 * Date: 2024-11-05
 * Unity Version: 2022.3.13f1
 * Description:
 *
 */

namespace UGUIPageNavigator.Runtime
{
    public enum PageOperation
    {
        Push,
        Pop
    }

    public interface IPageContainerEvent
    {
        public void Will(PageOperation operation, Page from, Page to);
        public void Did(PageOperation operation, Page from, Page to);
    }
}