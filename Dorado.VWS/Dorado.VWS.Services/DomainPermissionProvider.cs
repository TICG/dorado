/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2011/11/24 15:31:11
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2011/11/24 15:31:11               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System.Collections.Generic;
using Dorado.VWS.Model;
using Dorado.VWS.Model.Enum;
using Dorado.VWS.Services.Persistence;

namespace Dorado.VWS.Services
{
    public class DomainPermissionProvider
    {
        /// <summary>
        ///     �����������ݷ��ʲ�������
        /// </summary>
        private readonly DomainPermissionDao _domainPermissionDao = new DomainPermissionDao();

        /// <summary>
        /// ��ȡ��������Ա�б�
        /// </summary>
        /// <param name="domainName">����</param>
        /// <returns></returns>
        public static List<string> GetManageUserListByDomainName(string domainName)
        {
            List<string> list = new List<string>();

            try
            {
                DomainProvider domainProvider = new DomainProvider();
                DomainEntity de = domainProvider.GetDomainByName(domainName);
                if (de == null)
                {
                    return list;
                }
                DomainPermissionDao dpd = new DomainPermissionDao();
                IList<DomainPermissionEntity> dpeList = dpd.GetUsersByDomainAndPermissionType(de.DomainId, (int)Model.Enum.EnumManageType.DailyManage);

                if (dpeList != null)
                {
                    int i = 0;
                    foreach (DomainPermissionEntity dpe in dpeList)
                    {
                        list.Add(dpe.UserName);
                        i++;
                        if (i >= 3)
                        {
                            break;
                        }
                    }
                }
            }
            catch
            {
            }

            return list;
        }

        /// <summary>
        /// ��ȡ����Ȩ�û��б�
        /// </summary>
        /// <param name="domianID"></param>
        /// <param name="permissionType"></param>
        /// <returns></returns>
        public IList<DomainPermissionEntity> GetUsersByDomainAndPermissionType(int domianID, int permissionType)
        {
            return _domainPermissionDao.GetUsersByDomainAndPermissionType(domianID, permissionType);
        }

        /// <summary>
        /// ��ȡ�û�����Ȩ�б�
        /// </summary>
        /// <param name="domianID"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IList<DomainPermissionEntity> GetPermission(int domianID, string userName)
        {
            return _domainPermissionDao.GetPermission(domianID, userName);
        }

        /// <summary>
        /// ��ȡ�û�����Ȩ�б�
        /// </summary>
        /// <param name="domianID"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public DomainPermissionEntity GetPermission(int domianID, string userName, int permissionType)
        {
            return _domainPermissionDao.GetPermission(domianID, userName, permissionType);
        }

        /// <summary>
        /// �ж��û��Ƿ���Ȩ��
        /// </summary>
        /// <param name="domianID"></param>
        /// <param name="userName"></param>
        /// <param name="permissionType"></param>
        /// <returns></returns>
        public bool HasPermission(int domianID, string userName, int permissionType)
        {
            return (_domainPermissionDao.GetPermission(domianID, userName, permissionType) != null);
        }

        /// <summary>
        /// �ж��û��Ƿ���Ȩ��
        /// </summary>
        /// <param name="domianID"></param>
        /// <param name="userName"></param>
        /// <param name="permissionType"></param>
        /// <returns></returns>
        public bool HasPermission(int domianID, string userName, EnumManageType manageType)
        {
            return HasPermission(domianID, userName, (int)manageType);
        }

        /// <summary>
        /// ��ȡĳ�û�����������
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public IList<DomainPermissionEntity> GetDomainsByUser(string username)
        {
            return _domainPermissionDao.GetDomainsByUser(username);
        }

        /// <summary>
        /// ����û�
        /// </summary>
        /// <param name="domainPermissionEntity"></param>
        /// <returns></returns>
        public int Add(DomainPermissionEntity domainPermissionEntity)
        {
            return _domainPermissionDao.Add(domainPermissionEntity);
        }

        /// <summary>
        /// ɾ����Ȩ
        /// </summary>
        /// <param name="domainID"></param>
        /// <param name="userName"></param>
        /// <param name="permissionType"></param>
        /// <returns></returns>
        //public int Delete(int domainID, string userName, int permissionType)
        //{
        //    return _domainPermissionDao.Delete(domainID,userName,permissionType);
        //}

        /// <summary>
        /// ɾ����Ȩ
        /// </summary>
        /// <param name="domainPermissionEntity"></param>
        /// <returns></returns>
        public int Delete(DomainPermissionEntity domainPermissionEntity)
        {
            return _domainPermissionDao.Delete(domainPermissionEntity);
        }
    }
}