using System;
using System.Collections.Generic;

namespace Dorado.Core.Cache.StorageStrategys
{
    /// <summary>
    /// ���洢
    /// </summary>
    /// <typeparam name="TKey">������</typeparam>
    /// <typeparam name="TValue">ֵ����</typeparam>
    public class NullStorageStrategy<TKey, TValue> : IStorageStrategyEx<TKey, TValue>
        where TValue : class
    {
        #region IStorageStrategy<TKey,TValue> Members

        /// <summary>
        /// ���һ��������
        /// </summary>
        /// <param name="cacheItem">������</param>
        public void Add(CacheItem<TKey, TValue> cacheItem)
        {
        }

        /// <summary>
        /// ������ӻ�����
        /// </summary>
        /// <param name="cacheItems">������</param>
        public void AddRange(IEnumerable<CacheItem<TKey, TValue>> cacheItems)
        {
        }

        /// <summary>
        /// ��ȡһ��������
        /// </summary>
        /// <param name="key">�����</param>
        /// <returns></returns>
        public CacheItem<TKey, TValue> Get(TKey key)
        {
            return null;
        }

        public CacheItem<TKey, TValue>[] GetRange(IEnumerable<TKey> keys)
        {
            return null;
        }

        /// <summary>
        /// ��ȡȫ��������
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TKey> GetAllKeys()
        {
            return null;
        }

        public void Clear()
        {
        }

        /// <summary>
        /// ɾ��һ��������
        /// </summary>
        /// <param name="key">�����</param>
        public void Remove(TKey key)
        {
        }

        /// <summary>
        /// ����ɾ��������
        /// </summary>
        /// <param name="keys">�����</param>
        public void RemoveRange(IEnumerable<TKey> keys)
        {
        }

        #endregion IStorageStrategy<TKey,TValue> Members

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        ~NullStorageStrategy()
        {
            Dispose();
        }

        #endregion IDisposable Members

        #region ICacheExpireCheckSupported Members

        /// <summary>
        /// ��黺�����
        /// </summary>
        private void _CheckExpired()
        {
        }

        #endregion ICacheExpireCheckSupported Members
    }
}