using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace Dorado.Wcf.MiniEncoder
{
    #region MiniEncodingElement

    /// <summary>
    /// �Զ�������ѹ�����ý�
    /// </summary>
    public class MiniEncodingElement : BindingElementExtensionElement
    {
        /// <summary>
        /// �Զ������ýڵ����
        /// </summary>
        protected ConfigurationPropertyCollection _properties;

        public override Type BindingElementType
        {
            get
            {
                return typeof(MiniEncodingElement);
            }
        }

        protected override BindingElement CreateBindingElement()
        {
            MiniEncodingBindingElement bindingElement = new MiniEncodingBindingElement();
            ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        /// <summary>
        /// ��ʼ��������Ϣ
        /// </summary>
        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            MiniEncodingBindingElement element = (MiniEncodingBindingElement)bindingElement;

            // ��ȡ�����Զ���������Ϣ
            PropertyInformationCollection propertyInfo = ElementInformation.Properties;

            // ����ֻ������һ��������
            if (TextMessageEncodingElement.ElementInformation.IsPresent &&
                BinaryMessageEncodingElement.ElementInformation.IsPresent)
            {
                throw new ConfigurationErrorsException("������ 'textMessageEncoding' �� 'binaryMessageEncoding' ֻ������һ����");
            }

            // ȷ�������˱�����
            if (!TextMessageEncodingElement.ElementInformation.IsPresent &&
                !BinaryMessageEncodingElement.ElementInformation.IsPresent)
            {
                throw new ConfigurationErrorsException("û���ƶ��������������� 'textMessageEncoding' �� 'binaryMessageEncoding' ֻ������һ����");
            }

            if (TextMessageEncodingElement.ElementInformation.IsPresent)
            {
                element.InnerMessageEncodingBindingElement = new TextMessageEncodingBindingElement();
                TextMessageEncodingElement.ApplyConfiguration(element.InnerMessageEncodingBindingElement);
            }
            else if (BinaryMessageEncodingElement.ElementInformation.IsPresent)
            {
                element.InnerMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement();
                BinaryMessageEncodingElement.ApplyConfiguration(element.InnerMessageEncodingBindingElement);
            }

            //����ѹ���㷨
            if (CompressAlgorithm != null)
                element.CompressAlgorithm = CompressAlgorithm;
        }

        /// <summary>
        /// ��ȡ�����Զ������ü���
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("textMessageEncoding", typeof(TextMessageEncodingElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("binaryMessageEncoding", typeof(BinaryMessageEncodingElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("algorithm", typeof(CompressAlgorithm), null, null, null, ConfigurationPropertyOptions.None));

                    _properties = properties;
                }
                return _properties;
            }
        }

        [ConfigurationProperty("textMessageEncoding")]
        public TextMessageEncodingElement TextMessageEncodingElement
        {
            get
            {
                return (TextMessageEncodingElement)base["textMessageEncoding"];
            }
        }

        [ConfigurationProperty("binaryMessageEncoding")]
        public BinaryMessageEncodingElement BinaryMessageEncodingElement
        {
            get
            {
                return (BinaryMessageEncodingElement)base["binaryMessageEncoding"];
            }
        }

        /// <summary>
        /// ѹ���㷨����
        /// </summary>
        [ConfigurationProperty("algorithm", DefaultValue = "GZip")]
        public CompressAlgorithm CompressAlgorithm
        {
            get
            {
                return (CompressAlgorithm)base["algorithm"];
            }
        }
    }

    #endregion MiniEncodingElement
}