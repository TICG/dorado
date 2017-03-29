using System.Collections.Generic;

namespace Dorado.Wcf.DynamicProxy
{
    public class ServiceProxyFactory
    {
        private static object objLock = new object();

        private static Dictionary<string, DynamicProxyFactory> proxyFactoryList = new Dictionary<string, DynamicProxyFactory>();

        private static Dictionary<string, DynamicProxy> proxyList = new Dictionary<string, DynamicProxy>();

        static ServiceProxyFactory()
        {
        }

        /// <summary>
        ///��������������
        /// </summary>
        /// <param name="wsdlUri"></param>
        /// <returns></returns>
        public static DynamicProxyFactory CreateSingletonProxyFactory(string wsdlUri)
        {
            lock (objLock)
            {
                DynamicProxyFactory proxyFactory;
                if (!proxyFactoryList.ContainsKey(wsdlUri))
                {
                    proxyFactory = new DynamicProxyFactory(wsdlUri);
                    proxyFactoryList.Add(wsdlUri, proxyFactory);
                }
                else
                    proxyFactory = proxyFactoryList[wsdlUri];

                return proxyFactory;
            }
        }

        /// <summary>
        ///ˢ��ָ���Ĵ�����
        /// </summary>
        /// <param name="wsdlUri"></param>
        /// <param name="serviceContract"></param>
        public static void RefreshProxyFactory(string wsdlUri)
        {
            lock (objLock)
            {
                if (proxyFactoryList.ContainsKey(wsdlUri))
                {
                    proxyFactoryList.Remove(wsdlUri);
                    CreateSingletonProxyFactory(wsdlUri);
                }
            }
        }

        /// <summary>
        ///ˢ�����еĴ�����
        /// </summary>
        public static void RefreshProxyFactory()
        {
            lock (objLock)
            {
                foreach (var proxyFactory in proxyFactoryList)
                {
                    RefreshProxyFactory(proxyFactory.Key);
                }
            }
        }

        /// <summary>
        ///ȡ�ô���������
        /// </summary>
        public static int GetProxyFactoryCount()
        {
            return proxyFactoryList.Count;
        }

        /// <summary>
        ///������������
        /// </summary>
        /// <param name="wsdlUri"></param>
        /// <returns></returns>
        public static DynamicProxy CreateSingletonProxy(string wsdlUri, string contractName)
        {
            lock (objLock)
            {
                DynamicProxy proxy;
                string key = string.Format("{0}|{1}", wsdlUri, contractName);
                if (!proxyList.ContainsKey(key))
                {
                    DynamicProxyFactory proxyFactory = CreateSingletonProxyFactory(wsdlUri);
                    proxy = proxyFactory.CreateProxy(contractName);
                    proxy.CacheAllMethods();
                    proxyList.Add(key, proxy);
                }
                else
                    proxy = proxyList[key];

                return proxy;
            }
        }

        /// <summary>
        ///ˢ��ָ���Ĵ���
        /// </summary>
        /// <param name="wsdlUri"></param>
        /// <param name="serviceContract"></param>
        public static void RefreshProxy(string wsdlUri, string contractName)
        {
            lock (objLock)
            {
                string key = string.Format("{0}|{1}", wsdlUri, contractName);
                if (proxyList.ContainsKey(key))
                {
                    proxyList[key].Dispose();
                    proxyList.Remove(key);
                    CreateSingletonProxy(wsdlUri, contractName);
                }
            }
        }

        /// <summary>
        ///ˢ�����еĴ���
        /// </summary>
        public static void RefreshProxy()
        {
            lock (objLock)
            {
                foreach (var proxy in proxyList)
                {
                    string[] temp = proxy.Key.Split(new char[] { '|' });
                    RefreshProxy(temp[0], temp[1]);
                }
            }
        }

        /// <summary>
        ///ȡ�ô�������
        /// </summary>
        public static int GetProxyCount()
        {
            return proxyList.Count;
        }
    }
}