/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2011/11/24 10:19:52
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2011/11/24 10:19:52               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System;
using System.Data;

namespace Dorado.VWS.Model
{
    public class DomainPermissionEntity : EntityBase<DomainPermissionEntity>, IConvertToEntity<DomainPermissionEntity>
    {
        /*�ֶ�
         [ID]
      ,[DomainID]
      ,[UserName]
      ,[PermissionType]
      ,[DeleteFlag]
      ,[AddTime]
      ,[AddUserName]
      ,[UpdateTime]
      ,[UpdateUserName]
         */

        public int ID { get; set; }

        /// <summary>
        /// ����id
        /// </summary>
        public int DomainID { get; set; }

        /// <summary>
        /// ����Ȩ�����˺�
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Ȩ������ Dorado.VWS.Model.Enum. EnumManageType
        /// </summary>
        public int PermissionType { get; set; }

        /// <summary>
        /// ɾ�����
        /// </summary>
        public bool DeleteFlag { get; set; }

        /// <summary>
        /// ���ʱ��
        /// </summary>
        public DateTime AddTime { get; set; }

        /// <summary>
        /// ������Ȩ���û�
        /// </summary>
        public string AddUserName { get; set; }

        /// <summary>
        /// ������Ȩ��ʱ��
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// ������Ȩ���û�
        /// </summary>
        public string UpdateUserName { get; set; }

        #region IConvertToEntity<DomainPermissionEntity> Members

        public DomainPermissionEntity ConvertToEntity(DataRow row)
        {
            if (row != null)
            {
                var domainPermissionEntity = new DomainPermissionEntity
                {
                    ID = Convert.ToInt32(row["ID"]),
                    DomainID = Convert.ToInt32(row["DomainID"]),
                    UserName = row["UserName"].ToString(),
                    PermissionType = Convert.ToInt32(row["PermissionType"]),
                    DeleteFlag = Convert.ToBoolean(row["DeleteFlag"]),
                    AddTime = Convert.ToDateTime(row["AddTime"]),
                    AddUserName = row["AddUserName"].ToString(),
                    UpdateTime = Convert.ToDateTime(row["UpdateTime"]),
                    UpdateUserName = row["UpdateUserName"].ToString(),
                };

                return domainPermissionEntity;
            }
            return null;
        }

        #endregion IConvertToEntity<DomainPermissionEntity> Members
    }
}