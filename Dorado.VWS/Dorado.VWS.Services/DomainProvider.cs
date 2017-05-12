/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2011/10/25 14:43:04
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2011/10/25 14:43:04               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System.Collections.Generic;
using Dorado.VWS.Model;
using Dorado.VWS.Services.Persistence;

namespace Dorado.VWS.Services
{
    public class DomainProvider
    {
        /// <summary>
        ///     �����������ݷ��ʲ�������
        /// </summary>
        private readonly DomainDao _domainDao = new DomainDao();

        /// <summary>
        ///     ��ȡ������Ϣ
        /// </summary>
        /// <param name = "domainId">����Id</param>
        /// <returns>����ʵ��</returns>
        public IList<DomainEntity> GetAllDomains()
        {
            return _domainDao.GetAllDomains();
        }

        /// <summary>
        ///     ��ȡ������Ϣ
        /// </summary>
        /// <param name = "domainId">����Id</param>
        /// <returns>����ʵ��</returns>
        public DomainEntity GetDomainById(int domainId)
        {
            return _domainDao.GetDomainById(domainId);
        }

        /// <summary>
        /// ����������ȡ��Ϣ
        /// </summary>
        /// <param name="domainName"></param>
        public DomainEntity GetDomainByName(string domainName)
        {
            return _domainDao.GetDomainByName(domainName);
        }

        #region ��ȫ����

        /// <summary>
        ///     ��ȡָ��ip���������б�
        /// </summary>
        /// <returns>����ʵ���б�</returns>
        public IList<DomainEntity> GetDomains(string ip)
        {
            return _domainDao.GetDomains(ip);
        }

        #endregion ��ȫ����
    }
}