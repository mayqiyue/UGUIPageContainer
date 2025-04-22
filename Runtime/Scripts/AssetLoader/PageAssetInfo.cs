using System;

namespace UGUIPageNavigator.Runtime
{
    public sealed class PageAssetInfo
    {
        /// <summary>
        /// 必选，对于Addressable/YooAsset，资源的Key或者路径；对于Resources，资源的路径
        /// </summary>
        public readonly string addressKey;

        /// <summary>
        /// 可选，package名，YooAsset需要
        /// </summary>
        public readonly string package;

        public string Path => addressKey;


        public PageAssetInfo(string addressKey, string package = null)
        {
            this.addressKey = addressKey ?? throw new ArgumentNullException(nameof(addressKey));
            this.package = package;
        }

        public override bool Equals(object obj)
        {
            if (obj is PageAssetInfo other)
            {
                return addressKey == other.addressKey && package == other.package;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(addressKey, package);
        }

        public static bool operator ==(PageAssetInfo lhs, PageAssetInfo rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return true;
            if (lhs is null || rhs is null) return false;
            return lhs.addressKey == rhs.addressKey && lhs.package == rhs.package;
        }

        public static bool operator !=(PageAssetInfo lhs, PageAssetInfo rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return false;
            if (lhs is null || rhs is null) return true;
            return lhs.addressKey != rhs.addressKey || lhs.package != rhs.package;
        }

        public override string ToString()
        {
            return $"{package}:{addressKey}";
        }
    }
}