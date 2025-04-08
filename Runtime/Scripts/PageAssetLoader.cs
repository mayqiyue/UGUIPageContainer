using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UGUIPageNavigator.Runtime
{
    public class PageAssetLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle> _addressableHandles = new();

        /// <summary>
        /// Loads an asset by key. Tries Addressable first, then falls back to Resources.
        /// </summary>
        /// <typeparam name="T">Type of the asset to load.</typeparam>
        /// <param name="key">The key to load the asset.</param>
        /// <returns>The loaded asset or null if not found.</returns>
        internal async Task<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            // Check if the asset is already loaded
            if (_addressableHandles.TryGetValue(key, out var existingHandle) && existingHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return (T)existingHandle.Result;
            }

            // Try loading from Addressable
            try
            {
                var handle = Addressables.LoadAssetAsync<T>(key);
                _addressableHandles[key] = handle;
                return await handle.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to load asset from Addressables with key '{key}': {ex.Message}");
            }

            // Fallback to Resources
            var resource = Resources.Load<T>(key);
            if (resource == null)
            {
                Debug.LogError($"Failed to load asset from Resources with key '{key}'.");
            }

            return resource;
        }

        /// <summary>
        /// Releases an asset loaded from Addressables.
        /// </summary>
        /// <param name="key">The key of the asset to release.</param>
        internal void Release(string key)
        {
            if (_addressableHandles.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                _addressableHandles.Remove(key);
            }
            else
            {
                Debug.LogWarning($"No Addressable handle found for key '{key}' to release.");
            }
        }

        /// <summary>
        /// Releases all Addressable handles managed by this loader.
        /// </summary>
        internal void ReleaseAll()
        {
            foreach (var handle in _addressableHandles.Values)
            {
                Addressables.Release(handle);
            }

            _addressableHandles.Clear();
        }
    }
}