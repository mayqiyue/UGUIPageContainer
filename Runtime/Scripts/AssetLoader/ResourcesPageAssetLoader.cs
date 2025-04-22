using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UGUIPageNavigator.Runtime
{
    public class ResourcesPageAssetLoader : IPageAssetLoader
    {
        /// <summary>
        /// Loads an asset by key using the Resources API.
        /// </summary>
        public async UniTask<T> LoadAsync<T>(PageAssetInfo key) where T : UnityEngine.Object
        {
            var asset = Resources.Load<T>(key.addressKey);
            return asset;
        }

        /// <summary>
        /// Releases an asset by key. Resources API does not require explicit release.
        /// </summary>
        public void Release(PageAssetInfo key)
        {
        }

        /// <summary>
        /// Releases all loaded assets. Resources API does not require explicit release.
        /// </summary>
        public void ReleaseAll()
        {
        }
    }
}