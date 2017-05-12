/*-------------------------------------------------------------------------
 * ��Ȩ���У�Dorado
 * ʱ�䣺 2012/1/4 16:21:32
 * ���ߣ�
 * �汾            ʱ��                  ����                 ����
 * v 1.0    2012/1/4 16:21:32               ����
 * ������Ҫ��;������
 *  -------------------------------------------------------------------------*/

using System;
using System.Text.RegularExpressions;
using Dorado.Core;
using Dorado.Core.Logger;

namespace Dorado.VWS.Services
{
    public class SecurityExt
    {
        public static int RemoveAllCache()
        {
            try
            {
                Regex reg = new Regex("allowIPs_.*|allowservice_.*", RegexOptions.IgnoreCase);
                return WebCache.ClearAll(reg);
            }
            catch (Exception ex)
            {
                LoggerWrapper.Logger.Error("VWS.Site", ex.ToString());
            }
            return 0;
        }
    }
}