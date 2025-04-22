using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace UGUIPageNavigator.Runtime
{
    public interface IPageAssetLoader
    {
        /// <summary>
        /// Loads an asset by key.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="key">The key to load the asset.</param>
        /// <returns>The loaded asset or null if not found.</returns>
        UniTask<T> LoadAsync<T>(PageAssetInfo key) where T : UnityEngine.Object;

        /// <summary>
        /// Releases an asset by key.
        /// </summary>
        /// <param name="key">The key of the asset to release.</param>
        void Release(PageAssetInfo key);

        /// <summary>
        /// Releases all loaded assets.
        /// </summary>
        void ReleaseAll();
    }
}