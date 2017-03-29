namespace Dorado.Utils
{
    using Dorado.Core;
    using Dorado.Core.Logger;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// ���ݷ��ʳ��������
    /// </summary>
    public class SQLUtility
    {
        private string connString;

        public SQLUtility(string conn)
        {
            connString = conn;
        }

        #region ���÷���

        /// <summary>
        /// �ж��Ƿ����ĳ���ĳ���ֶ�
        /// </summary>
        /// <param name="tableName">������</param>
        /// <param name="columnName">������</param>
        /// <returns>�Ƿ����</returns>
        public bool IsExistsColumn(string tableName, string columnName)
        {
            string sql = "select count(1) from syscolumns where [id]=object_id('" + tableName + "') and [name]='" + columnName + "'";
            object res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return (int)res > 0;
        }

        public int GetMaxID(string fieldName, string tableName)
        {
            string strsql = "select max(" + fieldName + ")+1 from " + tableName;
            object obj = GetSingle(strsql);
            return obj == null ? 1 : (int)obj;
        }

        public bool IsExists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = (int)obj;
            }
            return cmdresult == 0 ? false : true;
        }

        /// <summary>
        /// ���Ƿ����
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public bool IsExistsTable(string tableName)
        {
            string strsql = "select count(1) from sysobjects where id = object_id(N'[" + tableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";

            //string strsql = "SELECT count(1) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U')";
            object obj = GetSingle(strsql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = (int)obj;
            }
            return cmdresult == 0 ? false : true;
        }

        public bool IsExists(string strSql, params SqlParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = (int)obj;
            }
            return cmdresult == 0 ? false : true;
        }

        public static string GetSafeSql(string rawString)
        {
            if (string.IsNullOrEmpty(rawString))
                return string.Empty;
            string returnValue = rawString;
            Dictionary<string, string> convertDict = new Dictionary<string, string>();
            convertDict.Add("'", "''");
            convertDict.Add("%", "[%]");
            convertDict.Add("[", "[[]");
            convertDict.Add("_", "[_]");
            convertDict.Add("^", "[^]");
            convertDict.Add("<", "[<]");
            convertDict.Add(">", "[>]");
            foreach (var v in convertDict)
            {
                returnValue = returnValue.Replace(v.Key, v.Value);
            }
            return returnValue;
        }

        public bool Test()
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        #endregion ���÷���

        #region ִ�м�SQL���

        /// <summary>
        /// ִ��SQL��䣬����Ӱ��ļ�¼��
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public int ExecuteSql(string sqlString)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return 0;
                    }
                }
            }
        }

        public int ExecuteSqlByTime(string sqlString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">����SQL���</param>
        public int ExecuteSqlTran(ICollection<String> sqlList)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                connection.Open();
                using (SqlTransaction tx = connection.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.Transaction = tx;
                        try
                        {
                            int count = 0;
                            foreach (string var in sqlList)
                            {
                                if (var.Trim().Length > 1)
                                {
                                    cmd.CommandText = var;
                                    count += cmd.ExecuteNonQuery();
                                }
                            }
                            tx.Commit();
                            return count;
                        }
                        catch (Exception ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                            tx.Rollback();
                            return 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ִ�д�һ���洢���̲����ĵ�SQL��䡣
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <param name="content">��������,����һ���ֶ��Ǹ�ʽ���ӵ����£���������ţ�����ͨ�������ʽ���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public int ExecuteSql(string sqlString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                    myParameter.Value = content;
                    cmd.Parameters.Add(myParameter);
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�д�һ���洢���̲����ĵ�SQL��䡣
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <param name="content">��������,����һ���ֶ��Ǹ�ʽ���ӵ����£���������ţ�����ͨ�������ʽ���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public object ExecuteSqlGet(string sqlString, string content)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    SqlParameter myParameter = new SqlParameter("@content", SqlDbType.NText);
                    myParameter.Value = content;
                    cmd.Parameters.Add(myParameter);
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            return null;
                        else
                            return obj;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// �����ݿ������ͼ���ʽ���ֶ�(������������Ƶ���һ��ʵ��)
        /// </summary>
        /// <param name="strSQL">SQL���</param>
        /// <param name="fs">ͼ���ֽ�,���ݿ���ֶ�����Ϊimage�����</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, connection))
                {
                    SqlParameter myParameter = new SqlParameter("@fs", SqlDbType.Image);
                    myParameter.Value = fs;
                    cmd.Parameters.Add(myParameter);
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// ִ��һ�������ѯ�����䣬���ز�ѯ�����object����
        /// </summary>
        /// <param name="SQLString">�����ѯ������</param>
        /// <returns>��ѯ�����object��</returns>
        public object GetSingle(string sqlString)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            return null;
                        else
                            return obj;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        public object GetSingle(string sqlString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(sqlString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            return null;
                        else
                            return obj;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�в�ѯ��䣬����SqlDataReader ( ע�⣺���ø÷�����һ��Ҫ��SqlDataReader����Close )
        /// </summary>
        /// <param name="strSQL">��ѯ���</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string strSQL)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand(strSQL, connection))
                {
                    try
                    {
                        connection.Open();
                        SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        return myReader;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�в�ѯ��䣬����DataSet
        /// </summary>
        /// <param name="SQLString">��ѯ���</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlDataAdapter command = new SqlDataAdapter(sqlString, connection))
                {
                    using (DataSet ds = new DataSet())
                    {
                        try
                        {
                            connection.Open();
                            command.Fill(ds, "ds");
                        }
                        catch (SqlException ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                        }
                        return ds;
                    }
                }
            }
        }

        public DataSet Query(string sqlString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlDataAdapter command = new SqlDataAdapter(sqlString, connection))
                {
                    using (DataSet ds = new DataSet())
                    {
                        try
                        {
                            connection.Open();
                            command.SelectCommand.CommandTimeout = Times;
                            command.Fill(ds, "ds");
                        }
                        catch (SqlException ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                        }
                        return ds;
                    }
                }
            }
        }

        #endregion ִ�м�SQL���

        #region ִ�д�������SQL���

        /// <summary>
        /// ִ��SQL��䣬����Ӱ��ļ�¼��
        /// </summary>
        /// <param name="SQLString">SQL���</param>
        /// <returns>Ӱ��ļ�¼��</returns>
        public int ExecuteSql(string sqlString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���Ĺ�ϣ��keyΪsql��䣬value�Ǹ�����SqlParameter[]��</param>
        public void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            //ѭ��
                            foreach (DictionaryEntry myDE in SQLStringList)
                            {
                                string cmdText = myDE.Key.ToString();
                                SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                int val = cmd.ExecuteNonQuery();
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���Ĺ�ϣ��keyΪsql��䣬value�Ǹ�����SqlParameter[]��</param>
        public int ExecuteSqlTran(ICollection<CommandInfo> cmdList)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            int count = 0;

                            //ѭ��
                            foreach (CommandInfo myDE in cmdList)
                            {
                                string cmdText = myDE.CommandText;
                                SqlParameter[] cmdParms = (SqlParameter[])myDE.Parameters;
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);

                                if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                                {
                                    if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    object obj = cmd.ExecuteScalar();
                                    bool isHave = false;
                                    if (obj == null && obj == DBNull.Value)
                                    {
                                        isHave = false;
                                    }
                                    isHave = Convert.ToInt32(obj) > 0;

                                    if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                                    {
                                        trans.Rollback();
                                        return 0;
                                    }
                                    continue;
                                }
                                int val = cmd.ExecuteNonQuery();
                                count += val;
                                if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                            return count;
                        }
                        catch (Exception ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                            trans.Rollback();
                            return 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���Ĺ�ϣ��keyΪsql��䣬value�Ǹ�����SqlParameter[]��</param>
        public void ExecuteSqlTranWithIndentity(ICollection<CommandInfo> SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            int indentity = 0;

                            //ѭ��
                            foreach (CommandInfo myDE in SQLStringList)
                            {
                                string cmdText = myDE.CommandText;
                                SqlParameter[] cmdParms = (SqlParameter[])myDE.Parameters;
                                foreach (SqlParameter q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.InputOutput)
                                    {
                                        q.Value = indentity;
                                    }
                                }
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                int val = cmd.ExecuteNonQuery();
                                foreach (SqlParameter q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.Output)
                                    {
                                        indentity = Convert.ToInt32(q.Value);
                                    }
                                }
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                            trans.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ִ�ж���SQL��䣬ʵ�����ݿ�����
        /// </summary>
        /// <param name="SQLStringList">SQL���Ĺ�ϣ��keyΪsql��䣬value�Ǹ�����SqlParameter[]��</param>
        public void ExecuteSqlTranWithIndentity(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        try
                        {
                            int indentity = 0;

                            //ѭ��
                            foreach (DictionaryEntry myDE in SQLStringList)
                            {
                                string cmdText = myDE.Key.ToString();
                                SqlParameter[] cmdParms = (SqlParameter[])myDE.Value;
                                foreach (SqlParameter q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.InputOutput)
                                    {
                                        q.Value = indentity;
                                    }
                                }
                                PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                                int val = cmd.ExecuteNonQuery();
                                foreach (SqlParameter q in cmdParms)
                                {
                                    if (q.Direction == ParameterDirection.Output)
                                    {
                                        indentity = Convert.ToInt32(q.Value);
                                    }
                                }
                                cmd.Parameters.Clear();
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            LoggerWrapper.Logger.Error(ex);
                            trans.Rollback();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ִ��һ�������ѯ�����䣬���ز�ѯ�����object����
        /// </summary>
        /// <param name="SQLString">�����ѯ������</param>
        /// <returns>��ѯ�����object��</returns>
        public object GetSingle(string sqlString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                            return null;
                        else
                            return obj;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�в�ѯ��䣬����SqlDataReader ( ע�⣺���ø÷�����һ��Ҫ��SqlDataReader����Close )
        /// </summary>
        /// <param name="strSQL">��ѯ���</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string sqlString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                        SqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        cmd.Parameters.Clear();
                        return myReader;
                    }
                    catch (SqlException ex)
                    {
                        LoggerWrapper.Logger.Error(ex);
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// ִ�в�ѯ��䣬����DataSet
        /// </summary>
        /// <param name="SQLString">��ѯ���</param>
        /// <returns>DataSet</returns>
        public DataSet Query(string sqlString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    PrepareCommand(cmd, connection, null, sqlString, cmdParms);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            try
                            {
                                da.Fill(ds, "ds");
                                cmd.Parameters.Clear();
                            }
                            catch (SqlException ex)
                            {
                                LoggerWrapper.Logger.Error(ex);
                            }
                            return ds;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepares the command.
        /// </summary>
        /// <param name="cmd">The CMD.</param>
        /// <param name="conn">The conn.</param>
        /// <param name="trans">The trans.</param>
        /// <param name="cmdText">The CMD text.</param>
        /// <param name="cmdParms">The CMD parms.</param>
        /// <Author>�Ž���(lietou1986@gmail.com) 2009-2-19</Author>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion ִ�д�������SQL���

        #region �洢���̲���

        /// <summary>
        /// ִ�д洢���̣�����SqlDataReader ( ע�⣺���ø÷�����һ��Ҫ��SqlDataReader����Close )
        /// </summary>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader RunProcedure(IDataParameter[] parameters, string storedProcName)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                SqlDataReader returnReader;
                connection.Open();
                SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
                returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return returnReader;
            }
        }

        /// <summary>
        /// ִ�д洢����
        /// </summary>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <param name="tableName">DataSet����еı���</param>
        /// <returns>DataSet</returns>
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlDataAdapter sqlDA = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        connection.Open();
                        sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                        sqlDA.Fill(ds, tableName);
                        return ds;
                    }
                }
            }
        }

        /// <summary>
        /// Runs the procedure.
        /// </summary>
        /// <param name="storedProcName">Name of the stored proc.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="Times">The times.</param>
        /// <returns></returns>
        public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                using (SqlDataAdapter sqlDA = new SqlDataAdapter())
                {
                    using (DataSet ds = new DataSet())
                    {
                        connection.Open();
                        sqlDA.SelectCommand = BuildQueryCommand(connection, storedProcName, parameters);
                        sqlDA.SelectCommand.CommandTimeout = Times;
                        sqlDA.Fill(ds, tableName);
                        return ds;
                    }
                }
            }
        }

        /// <summary>
        /// ���� SqlCommand ����(��������һ���������������һ������ֵ)
        /// </summary>
        /// <param name="connection">���ݿ�����</param>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <returns>SqlCommand</returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(storedProcName, connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter != null)
                    {
                        // ���δ����ֵ���������,���������DBNull.Value.
                        if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                            (parameter.Value == null))
                        {
                            parameter.Value = DBNull.Value;
                        }
                        command.Parameters.Add(parameter);
                    }
                }
                return command;
            }
        }

        /// <summary>
        /// ִ�д洢���̣�����Ӱ�������
        /// </summary>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <param name="rowsAffected">Ӱ�������</param>
        /// <returns></returns>
        public int RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            {
                try
                {
                    int result;
                    connection.Open();
                    SqlCommand command = BuildIntCommand(connection, storedProcName, parameters);
                    result = Convert.ToInt32(command.ExecuteScalar());
                    return result;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// ���� SqlCommand ����ʵ��(��������һ������ֵ)
        /// </summary>
        /// <param name="storedProcName">�洢������</param>
        /// <param name="parameters">�洢���̲���</param>
        /// <returns>SqlCommand ����ʵ��</returns>
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            SqlCommand command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        #endregion �洢���̲���
    }

    public enum EffentNextType
    {
        /// <summary>
        /// ������������κ�Ӱ��
        /// </summary>
        None,

        /// <summary>
        /// ��ǰ������Ϊ"select count(1) from .."��ʽ��������������ִ�У������ڻع�����
        /// </summary>
        WhenHaveContine,

        /// <summary>
        /// ��ǰ������Ϊ"select count(1) from .."��ʽ����������������ִ�У����ڻع�����
        /// </summary>
        WhenNoHaveContine,

        /// <summary>
        /// ��ǰ���Ӱ�쵽�������������0������ع�����
        /// </summary>
        ExcuteEffectRows,

        /// <summary>
        /// �����¼�-��ǰ������Ϊ"select count(1) from .."��ʽ����������������ִ�У����ڻع�����
        /// </summary>
        SolicitationEvent
    }

    public class CommandInfo
    {
        public object ShareObject = null;
        public object OriginalData = null;

        private event EventHandler _solicitationEvent;

        public event EventHandler SolicitationEvent
        {
            add
            {
                _solicitationEvent += value;
            }
            remove
            {
                _solicitationEvent -= value;
            }
        }

        public void OnSolicitationEvent()
        {
            if (_solicitationEvent != null)
            {
                _solicitationEvent(this, new EventArgs());
            }
        }

        public string CommandText;
        public System.Data.Common.DbParameter[] Parameters;
        public EffentNextType EffentNextType = EffentNextType.None;

        public CommandInfo()
        {
        }

        public CommandInfo(string sqlText, SqlParameter[] para)
        {
            this.CommandText = sqlText;
            this.Parameters = para;
        }

        public CommandInfo(string sqlText, SqlParameter[] para, EffentNextType type)
        {
            this.CommandText = sqlText;
            this.Parameters = para;
            this.EffentNextType = type;
        }
    }
}