using System;
using System.Xml.Serialization;

namespace Dorado.Web.Fileset
{
    [Flags]
    [Serializable]
    public enum StaticFileFlag
    {
        /// <summary>
        /// δָ��(��ָ�����ο�)
        /// </summary>
        [XmlEnum("none")]
        None = 0,

        /// <summary>
        /// �ο����ⲿ���ã�
        /// </summary>
        [XmlEnum("ref")]
        Reference = 1 << 1,

        /// <summary>
        /// Ƕ��
        /// </summary>
        [XmlEnum("embed")]
        Embed = 1 << 2,

        /// <summary>
        /// �����ļ�(WebӦ���ڿ�ֱ�Ӷ�ȡ���ļ�)
        /// </summary>
        [XmlEnum("local")]
        Local = 1 << 3,

        /// <summary>
        /// Զ���ļ�
        /// </summary>
        [XmlEnum("remote")]
        Remote = 1 << 4,
    }
}