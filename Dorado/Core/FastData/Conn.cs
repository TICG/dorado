using Dorado.Core.Logger;
using Dorado.Extensions;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Dorado.Core.FastData
{
    /// <summary>
    /// Sql Sever ���ݿ������ࡣ
    /// </summary>
    public class Conn : IConn
    {
        private static string _default;
        private SqlConnection _conn;
        private SqlTransaction _trans;
        private string _name;
        private string _tablename;
        private int _top;
        private int _page = 1;
        private int _pagesize;
        private int _maxcount;
        private ArrayList _field;
        private ArrayList _idx;
        private StringBuilder _join;
        private StringBuilder _where;
        private ArrayList _sort;
        private ArrayList _batchName;
        private ArrayList _batchSql;
        private string _keyid;
        private int _commandTimeOut;

        public Conn(string connectionString, int commandTimeOut = 60)
        {
            if (!connectionString.HasValue())
                throw new CoreException("���ݿ����Ӵ�����Ϊ��");

            _commandTimeOut = commandTimeOut;
            _default = connectionString;
            _conn = new SqlConnection(connectionString);
        }

        /// <summary>
        /// �ĳ����ӵ����ݿ�
        /// </summary>
        /// <param name="databaseName">���ݿ���</param>
        public Conn ChangeDatabase(string databaseName)
        {
            Open();
            _conn.ChangeDatabase(databaseName);
            return this;
        }

        //��������

        /// <summary>
        /// ��ǰ��ѯ������
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null) throw new CoreException("�Բ���Conn�������Ʋ���Ϊ�գ�");
                _name = value;
            }
        }

        public int CommandTimeOut { get { return _commandTimeOut; } set { _commandTimeOut = value; } }

        public Conn Add()
        {
            Add(Name, Sql());
            return this;
        }

        public Conn Add(string name)
        {
            Add(name, Sql());
            return this;
        }

        public Conn Add(string name, string sql)
        {
            if (name == null) throw new CoreException("�Բ���������ݼ�ʱ�����Ʋ���Ϊ�գ�");
            if (_batchName == null)
            {
                _batchName = new ArrayList();
                _batchSql = new ArrayList();
            }
            _batchName.Add(name);
            _batchSql.Add(sql);

            Clear();
            return this;
        }

        public Conn Add(string name, string sql, params object[] para)
        {
            Add(name, String.Format(sql, para));
            return this;
        }

        public int Top
        {
            get { return _top; }
            set
            {
                if (value <= 0) throw new CoreException("Top����Ϊ��ֵ����!");
                _top = value;
            }
        }

        public int Page
        {
            get { return _page; }
            set
            {
                _page = value <= 0 ? 1 : value;
            }
        }

        public int PageSize
        {
            get { return _pagesize; }
            set
            {
                if (value >= 0)
                    _pagesize = value;
                else
                    throw new CoreException("ÿҳ��ʾ��������Ϊ��ֵ!");
            }
        }

        public int MaxCount
        {
            get { return _maxcount; }
            set
            {
                if (value >= 0)
                    _maxcount = value;
                else
                    throw new CoreException("��¼����������Ϊ��ֵ!");
            }
        }

        public string DataBaseName
        {
            get
            {
                Open();
                return _conn.Database;
            }
            set { ChangeDatabase(value); }
        }

        public SqlConnection Connection
        {
            get { return _conn; }
        }

        public SqlTransaction Transaction
        {
            get
            {
                return _trans;
            }
            set
            {
                _trans = value;
            }
        }

        public Conn Open()
        {
            if (_conn == null) _conn = new SqlConnection(_default);
            if (_conn.State != ConnectionState.Open) _conn.Open();
            return this;
        }

        public Conn BeginTrans(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            if (_trans != null) return this;
            if (_conn == null) _conn = new SqlConnection(_default);
            if (_conn.State != ConnectionState.Open) _conn.Open();
            _trans = _conn.BeginTransaction(isolationLevel);
            return this;
        }

        public Conn Rollback()
        {
            if (_conn == null || _conn.State == ConnectionState.Closed) return this;
            if (_trans == null) return this;
            _trans.Rollback();
            _trans.Dispose();
            _trans = null;
            return this;
        }

        public Conn Close()
        {
            if (_conn == null || _conn.State == ConnectionState.Closed) return this;

            if (_trans != null)
            {
                _trans.Commit();
                _trans.Dispose();
                _trans = null;
            }

            _conn.Close();
            _conn.Dispose();
            _conn = null;
            return this;
        }

        public Conn From(string tableName)
        {
            _tablename = tableName;
            return this;
        }

        public Conn Fields(params string[] name)
        {
            if (_field == null) _field = new ArrayList();
            foreach (string t in name)
            {
                string[] tmp = t.Trim().ToLower().Split(' ');
                _field.Add(tmp.Length > 1
                           ? new string[] { tmp[0], tmp[1], null }
                           : new string[] { tmp[0], string.Empty, null });
            }
            return this;
        }

        public Conn KeyId(string keyid)
        {
            _keyid = keyid;
            return this;
        }

        public Conn Field(string func, string name, string alias)
        {
            if (_field == null) _field = new ArrayList();
            _field.Add(new string[] { "dbo." + func + "(" + name + ")", alias.ToLower(), null });
            return this;
        }

        public Conn Field(string name)
        {
            if (_field == null) _field = new ArrayList();
            _field.Add(new string[] { name.ToLower(), string.Empty, null });
            return this;
        }

        public Conn Field(string name, string alias)
        {
            if (_field == null) _field = new ArrayList();
            _field.Add(new string[] { name, alias.ToLower(), null });
            return this;
        }

        public Conn Field(string name, int length)
        {
            if (_field == null) _field = new ArrayList();
            _field.Add(new string[] { name.ToLower(), string.Empty, length.ToString() });
            return this;
        }

        public Conn Field(string name, string alias, int length)
        {
            if (_field == null) _field = new ArrayList();
            _field.Add(new string[] { name, alias.ToLower(), length.ToString() });
            return this;
        }

        public Conn Field(string name, bool isIndex)
        {
            if (isIndex)
            {
                if (_idx == null) _idx = new ArrayList();
                _idx.Add(new string[] { name.ToLower(), string.Empty, null });
            }
            else
            {
                if (_field == null) _field = new ArrayList();
                _field.Add(new string[] { name.ToLower(), string.Empty, null });
            }
            return this;
        }

        public Conn Field(string name, bool isIndex, int length)
        {
            if (isIndex)
            {
                if (_idx == null) _idx = new ArrayList();
                _idx.Add(new string[] { name.ToLower(), string.Empty, length.ToString() });
            }
            else
            {
                if (_field == null) _field = new ArrayList();
                _field.Add(new string[] { name.ToLower(), string.Empty, length.ToString() });
            }
            return this;
        }

        public Conn Field(string name, string alias, bool isIndex)
        {
            if (isIndex)
            {
                if (_idx == null) _idx = new ArrayList();
                _idx.Add(new string[] { name.ToLower(), alias, null });
            }
            else
            {
                if (_field == null) _field = new ArrayList();
                _field.Add(new string[] { name.ToLower(), alias, null });
            }
            return this;
        }

        public Conn Field(string name, string alias, bool isIndex, int length)
        {
            if (isIndex)
            {
                if (_idx == null) _idx = new ArrayList();

                _idx.Add(new string[] { name.ToLower(), alias, length.ToString() });
            }
            else
            {
                if (_field == null) _field = new ArrayList();
                _field.Add(new string[] { name.ToLower(), alias, length.ToString() });
            }
            return this;
        }

        public Conn Join(string tableName, string source, string dest)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" inner join " + tableName + " (nolock) on " + source + " = " + dest);
            return this;
        }

        public Conn Join(string tableName, string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" inner join " + tableName + " (nolock) on " + sql);
            return this;
        }

        public Conn Join(string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" inner join " + sql);
            return this;
        }

        public Conn LeftJoin(string tableName, string source, string dest)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" left join " + tableName + " (nolock) on " + source + " = " + dest);
            return this;
        }

        public Conn LeftJoin(string tableName, string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" left join " + tableName + " (nolock) on " + sql);
            return this;
        }

        public Conn LeftJoin(string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" left join " + sql);
            return this;
        }

        public Conn RightJoin(string tableName, string source, string dest)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" right join " + tableName + " (nolock) on " + source + " = " + dest);
            return this;
        }

        public Conn RightJoin(string tableName, string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" right join " + tableName + " (nolock) on " + sql);
            return this;
        }

        public Conn RightJoin(string sql)
        {
            if (_join == null) _join = new StringBuilder();
            _join.Append(" right join " + sql);
            return this;
        }

        public Conn Clear()
        {
            _tablename = null;

            _top = 0;
            _page = 1;
            _pagesize = 20;
            _maxcount = 0;

            if (_idx != null) _idx.Clear();
            if (_field != null) _field.Clear();
            if (_join != null) _join.Remove(0, _join.Length);
            if (_where != null) _where.Remove(0, _where.Length);
            if (_sort != null) _sort.Clear();
            return this;
        }

        public Conn ClearField()
        {
            if (_idx != null) _idx.Clear();
            if (_field != null) _field.Clear();
            return this;
        }

        public Conn ClearWhere()
        {
            if (_where != null) _where.Remove(0, _where.Length);
            return this;
        }

        public Conn ClearJoin()
        {
            if (_join != null) _join.Remove(0, _join.Length);
            return this;
        }

        public Conn ClearOrder()
        {
            if (_order != null) _sort.Clear();
            return this;
        }

        public Conn Where(string sql)
        {
            if (_where == null) _where = new StringBuilder();
            _where.Append(_where.Length > 0 ? " and " : " where ");
            _where.Append("(" + sql + ")");
            return this;
        }

        public Conn Where(string[] fields, object[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < fields.Length; i++)
            {
                if (sb.Length > 0) sb.Append(" and ");
                sb.Append(fields[i] + "=" + DataTypeExtensions.ToSql(data[i]));
            }
            Where(sb.ToString());
            return this;
        }

        public Conn Where(string sql, params object[] para)
        {
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] is string) para[i] = para[i].ToString();
            }
            Where(string.Format(sql, para));
            return this;
        }

        public Conn WhereIn(string sql, params object[] para)
        {
            if (_where == null) _where = new StringBuilder();
            _where.Append(_where.Length > 0 ? " and " : " where ");
            _where.Append("(" + String.Format(sql, para) + ")");
            return this;
        }

        public Conn WhereOr(string sql, params object[] para)
        {
            if (_where == null) _where = new StringBuilder();
            _where.Append(_where.Length > 0 ? " or " : " where ");
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] is string) para[i] = para[i].ToString();
            }
            _where.Append("(" + String.Format(sql, para) + ")");
            return this;
        }

        public Conn Order(string field)
        {
            if (_sort == null) _sort = new ArrayList(3);
            field = field.Trim().ToLower();
            string[] tmp = field.Split(',');
            foreach (string t in tmp)
            {
                string[] s = new string[3];
                if (t.IndexOf(" desc") >= 0)
                    s[1] = "desc";
                else
                    s[1] = "asc";
                s[0] = t.Replace(" desc", string.Empty).Replace(" asc", string.Empty).Trim();
                s[2] = null;
                _sort.Add(s);
            }
            return this;
        }

        //��������Sql�ַ���
        private string _order
        {
            get
            {
                if (_sort == null) return string.Empty;
                StringBuilder sort = new StringBuilder();
                foreach (object t1 in _sort)
                {
                    string[] t = (string[])t1;
                    if (sort.Length > 0) sort.Append(','); else sort.Append(" order by ");
                    sort.Append(t[0] + " " + t[1]);
                }
                return sort.ToString();
            }
        }

        //���ɵ�����Sql�ַ���
        private string Orderback
        {
            get
            {
                if (_sort == null) return string.Empty;
                StringBuilder sort = new StringBuilder();
                foreach (string[] t in _sort.Cast<string[]>())
                {
                    if (sort.Length > 0) sort.Append(','); else sort.Append(" order by ");
                    sort.Append(t[0] + " " + (t[1] == "desc" ? "asc" : "desc"));
                }
                return sort.ToString();
            }
        }

        //���ɷ�ҳ����
        private static string _gensort(ArrayList arr, object[] vv, int i, bool back)
        {
            if (arr == null || arr.Count == 0) throw new CoreException("�Բ�����ͨ��ҳ����Ҫָ������������");
            StringBuilder sb = new StringBuilder();

            sb.Append("(");
            string[] s = (string[])arr[i];
            string op;
            if (s[1].Equals("asc")) op = (back ? "<" : ">");
            else op = (back ? ">" : "<");

            string tmp;
            if (vv == null) tmp = "@para" + i;
            else tmp = DataTypeExtensions.ToSql(vv[i]);

            if (i >= arr.Count - 1)
                sb.Append(s[0] + op + tmp);
            else
            {
                sb.Append(s[0] + "=" + tmp);
                sb.Append(" and ");
                sb.Append(_gensort(arr, vv, i + 1, back));
                sb.Append(" or ");
                sb.Append(s[0] + op + tmp);
            }
            sb.Append(")");
            return sb.ToString();
        }

        public int ExecuteNonQuery(string sql)
        {
            Open();
            using (SqlCommand cmd = new SqlCommand(sql, _conn))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecuteNonQuery(string sql, params object[] para)
        {
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] is string) para[i] = para[i].ToString();
            }
            Open();
            using (SqlCommand cmd = new SqlCommand(String.Format(sql, para), _conn))
            {
                if (Transaction != null) cmd.Transaction = Transaction;
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string sql)
        {
            Open();
            using (SqlCommand cmd = new SqlCommand(sql, _conn))
            {
                if (Transaction != null) cmd.Transaction = Transaction;
                return cmd.ExecuteScalar();
            }
        }

        public object ExecuteScalar(string sql, params object[] para)
        {
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] is string) para[i] = para[i].ToString();
            }
            Open();
            using (SqlCommand cmd = new SqlCommand(String.Format(sql, para), _conn))
            {
                if (Transaction != null) cmd.Transaction = Transaction;
                return cmd.ExecuteScalar();
            }
        }

        public DataArrayList SelectBatch()
        {
            return SelectBatch(null);
        }

        public DataArrayList SelectBatch(DataArrayList list)
        {
            if (_batchName == null) throw new CoreException("����û��ָ���κβ�ѯ��䣡");
            if (list == null) list = new DataArrayList();
            StringBuilder sb = new StringBuilder();
            foreach (object t in _batchSql)
            {
                sb.Append(t + ";\n");
            }
            Open();
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = new SqlCommand(sb.ToString(), Connection) { CommandTimeout = _commandTimeOut };
                reader = cmd.ExecuteReader();

                int index = 0;
                do
                {
                    string name = _batchName[index].ToString();
                    DataArrayColumn[] columns = new DataArrayColumn[reader.FieldCount];
                    var data = new DataArray(name);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns[i] = data.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                    }

                    while (reader.Read())
                    {
                        data.AddRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columns[i].Set(reader.GetValue(i), data.Count - 1);
                        }
                    }

                    list.Add(data);

                    index++;
                } while (reader.NextResult());
                reader.Close();
            }
            catch (SqlException ex)
            {
                LoggerWrapper.Logger.Error("Conn.SelectBacth��������", ex);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (cmd != null) cmd.Dispose();
            }
            _batchName.Clear();
            _batchSql.Clear();
            return list;
        }

        public DataArrayList SelectAll()
        {
            return SelectAll(null);
        }

        public DataArrayList SelectAll(DataArrayList list)
        {
            if (_batchName == null) throw new CoreException("����û��ָ���κβ�ѯ��䣡");
            if (list == null) list = new DataArrayList();
            Open();
            StringBuilder error = new StringBuilder();
            for (int i = 0; i < _batchSql.Count; i++)
            {
                SqlCommand cmd = null;
                SqlDataReader reader = null;
                try
                {
                    cmd = new SqlCommand(_batchSql.ToString(), Connection) { CommandTimeout = _commandTimeOut };
                    reader = cmd.ExecuteReader();

                    list.Add(_batchName[i].ToString(), reader.Exec(null, false));
                    reader.Close();
                }
                catch (SqlException ex)
                {
                    error.Append("����" + ex.Message + "\n" + _batchSql[i].ToString() + "\n");
                }
                finally
                {
                    if (reader != null) reader.Close();
                    if (cmd != null) cmd.Dispose();
                }
            }
            if (error.Length > 0)
            {
                LoggerWrapper.Logger.Error("Conn.SelectAll��������\r\n" + error.ToString());
            }
            _batchName.Clear();
            _batchSql.Clear();
            return list;
        }

        private string Fld()
        {
            StringBuilder ret = new StringBuilder();
            if (_idx != null)
            {
                foreach (string[] tmp in from object t in _idx select t as string[])
                {
                    if (ret.Length > 0) ret.Append(",");
                    if (tmp[2] == null)
                        ret.Append(tmp[0] + " " + tmp[1]);
                    else
                        ret.Append("cast(" + tmp[0] + " as nvarchar(" + tmp[2] + "))+'..' " + (tmp[1] == string.Empty ? tmp[0] : tmp[1]));
                }
            }
            if (_field == null) return ret.ToString();
            foreach (string[] tmp in from object t in _field select t as string[])
            {
                if (ret.Length > 0) ret.Append(",");
                if (tmp[2] == null)
                    ret.Append(tmp[0] + " " + tmp[1]);
                else
                    ret.Append("cast(" + tmp[0] + " as nvarchar(" + tmp[2] + "))+'..' " + (tmp[1] == string.Empty ? tmp[0] : tmp[1]));
            }
            return ret.ToString();
        }

        #region �򵥲�ѯ Select

        public string Sql()
        {
            if (_tablename == null) throw new CoreException("�Բ�����û��ָ��������");
            return "select"
                + (_top > 0 ? " top " + _top.ToString() : string.Empty) + " "
                + Fld()
                + " from "
                + _tablename + " (nolock) "
                + (_join == null ? string.Empty : _join.ToString()) + " "
                + (_where == null ? string.Empty : _where.ToString()) + " "
                + _order;
        }

        public DataArray Select()
        {
            return Select(Sql());
        }

        public DataArray Select(string sql, params object[] para)
        {
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] is string) para[i] = para[i].ToString();
            }
            return Select(String.Format(sql, para));
        }

        public DataArray Select(string sql)
        {
            DataArray data = new DataArray(_name);
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                Open();
                cmd = new SqlCommand(sql, Connection) { CommandTimeout = _commandTimeOut };
                reader = cmd.ExecuteReader();
                reader.Exec(data, false);
            }
            catch (SqlException ex)
            {
                LoggerWrapper.Logger.Error("Conn.Select��������" + sql, ex);
                throw ex;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (cmd != null) cmd.Dispose();
            }
            data.PageSize = PageSize;
            return data;
        }

        #endregion �򵥲�ѯ Select

        #region ��ҳ��ѯ SelectPage

        private string SqlPage(ref bool back, bool isLimitPageIndex = true)
        {
            if (_pagesize <= 0) _pagesize = 20;
            if (_page <= 0) _page = 1;
            StringBuilder sb = new StringBuilder();
            if (_maxcount <= 0)
            {
                if (_join == null && _where == null && _tablename.IndexOf("(") == -1)
                {
                    sb.Append("declare @cc int;\n");
                    sb.Append("select @cc=rowcnt from sysindexes (nolock) where id=object_id('" + _tablename + "') and indid=1;\n");
                    sb.Append("if @cc>0 select @cc;\n");
                    sb.Append("else	select count(*) from " + _tablename + " (nolock) ;\n");
                }
                else
                {
                    sb.Append("select count(" + (_keyid == null ? "*" : "distinct " + _keyid) + ") from " + _tablename + " (nolock) ");
                    if (_join != null) sb.Append(_join.ToString());
                    if (_where != null) sb.Append(_where.ToString());
                }
                SqlCommand cmd = null;
                try
                {
                    Open();
                    cmd = new SqlCommand(sb.ToString(), _conn);
                    _maxcount = System.Convert.ToInt32(cmd.ExecuteScalar());
                }
                finally
                {
                    if (cmd != null) cmd.Dispose();
                }
                sb.Remove(0, sb.Length);
            }
            int max = (_maxcount - 1) / _pagesize + 1;
            if (_page > max && isLimitPageIndex) _page = max;
            if (_page == 1)
            {
                back = false;
                sb.Append("select distinct top " + _pagesize.ToString() + " " + Fld() + " from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString() + "\n");
                sb.Append(_order);
            }
            else if (_page == max)
            {
                back = true;
                sb.Append("select distinct top " + (_maxcount - (max - 1) * _pagesize) + " " + Fld() + " from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString() + "\n");
                sb.Append(Orderback + "\n");
            }
            else if (_page > max / 2 + 20)
            {
                back = true;
                int len = _sort.Count;
                for (int i = 0; i < len; i++)
                    sb.Append("declare @para" + i.ToString() + " nvarchar(255);\n");
                sb.Append("select distinct top " + (_maxcount - (_page) * _pagesize).ToString() + " ");
                for (int i = 0; i < len; i++)
                {
                    if (i < len - 1)
                        sb.Append("@para" + i.ToString() + "=" + ((string[])_sort[i])[0] + ",");
                    else
                        sb.Append("@para" + i.ToString() + "=" + ((string[])_sort[i])[0] + " ");
                }
                sb.Append(" from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString() + "\n");
                sb.Append(Orderback + ";\n");
                sb.Append("select distinct top " + _pagesize.ToString() + " " + Fld() + " from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString());
                if (_where != null && _where.Length > 0) sb.Append(" and "); else sb.Append(" where ");
                sb.Append(_gensort(_sort, null, 0, back));
                sb.Append(Orderback);
            }
            else
            {
                back = false;
                int len = _sort.Count;
                for (int i = 0; i < len; i++)
                    sb.Append("declare @para" + i.ToString() + " nvarchar(255);\n");
                sb.Append("select distinct top " + ((_page - 1) * _pagesize).ToString() + " ");
                for (int i = 0; i < len; i++)
                {
                    if (i < len - 1)
                        sb.Append("@para" + i.ToString() + "=" + ((string[])_sort[i])[0] + ",");
                    else
                        sb.Append("@para" + i.ToString() + "=" + ((string[])_sort[i])[0] + " ");
                }
                sb.Append(" from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString() + "\n");
                sb.Append(_order + ";\n");
                sb.Append("select distinct top " + _pagesize.ToString() + " " + Fld() + " from " + _tablename + " (nolock) \n");
                if (_join != null) sb.Append(_join.ToString() + "\n");
                if (_where != null) sb.Append(_where.ToString());
                if (_where != null && _where.Length > 0) sb.Append(" and "); else sb.Append(" where ");
                sb.Append(_gensort(_sort, null, 0, back));
                sb.Append(_order);
            }
            return sb.ToString();
        }

        public DataArray SelectPage(bool isLimitPageIndex = true)
        {
            if (_pagesize == 0) _pagesize = 20;

            SqlCommand cmd = null;
            SqlDataReader reader = null;

            DataArray data;

            data = new DataArray((_name), _pagesize);
            bool back = false;
            var sql = SqlPage(ref back, isLimitPageIndex);

            data.Page = _page;
            data.PageSize = _pagesize;
            data.MaxCount = _maxcount;

            try
            {
                Open();
                cmd = new SqlCommand(sql, _conn) { CommandTimeout = _commandTimeOut };
                reader = cmd.ExecuteReader();
                reader.Exec(data, back);
            }
            catch (SqlException ex)
            {
                LoggerWrapper.Logger.Error("Conn.SelectPage��������" + sql, ex);
                throw ex;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (cmd != null) cmd.Dispose();
            }
            return data;
        }

        #endregion ��ҳ��ѯ SelectPage

        #region �α��ҳ��ѯ SelectCursor

        public string SqlCursor(string sql, bool isLimitPageIndex)
        {
            if (_pagesize <= 0) _pagesize = 20;
            if (_page <= 0) _page = 1;
            StringBuilder sb = new StringBuilder();
            sb.Append("declare @P1 int, @rowcount int, @page int, @pagesize int,@sqlstr nvarchar(4000);\n");
            sb.Append("set @page=" + _page.ToString() + ";\n");
            sb.Append("set @pagesize=" + _pagesize.ToString() + ";\n");
            sb.Append("set @sqlstr=N'" + sql.Replace("'", "''") + "';\n");
            sb.Append("exec sp_cursoropen @P1 output,@sqlstr,@scrollopt=1,@ccopt=1, @rowcount=@rowcount output;\n");
            if (isLimitPageIndex)
            {
                sb.Append("if @page*" + _pagesize.ToString() + "> @rowcount + " + _pagesize.ToString() + " ");
                sb.Append("set @page=(@rowcount-1)/" + _pagesize.ToString() + "+1;\n");
            }
            sb.Append("select @page,@rowcount,ceiling(1.0*@rowcount/@pagesize);\n");
            sb.Append("set @page=(@page-1)*@pagesize+1;\n");
            sb.Append("exec sp_cursorfetch @P1,16,@page," + _pagesize.ToString() + ";\n");
            sb.Append("exec sp_cursorclose @P1;\n");
            return sb.ToString();
        }

        public DataArray SelectCursor(bool isLimitPageIndex = true)
        {
            return SelectCursor(null, isLimitPageIndex);
        }

        public DataArray SelectCursor(string sql, bool isLimitPageIndex = true)
        {
            sql = SqlCursor(sql ?? Sql(), isLimitPageIndex);

            if (_pagesize == 0) _pagesize = 20;
            DataArray data = new DataArray(_name, _pagesize) { PageSize = _pagesize };

            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                Open();
                cmd = new SqlCommand(sql, _conn) { CommandTimeout = _commandTimeOut };

                reader = cmd.ExecuteReader();

                reader.NextResult();
                reader.Read();
                _page = reader.GetInt32(0);
                _maxcount = reader.GetInt32(1);
                data.Page = _page;
                data.MaxCount = _maxcount;

                reader.NextResult();

                reader.Exec(data, false);
            }
            catch (SqlException ex)
            {
                LoggerWrapper.Logger.Error("Conn.SelectCursor��������" + sql, ex);
                throw ex;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (cmd != null) cmd.Dispose();
            }
            return data;
        }

        #endregion �α��ҳ��ѯ SelectCursor

        #region RowNumber��ҳ��ѯ SelectRowNumber

        public string SqlRowNumber()
        {
            if (_pagesize <= 0) _pagesize = 20;
            if (_page <= 0) _page = 1;
            string fld = Fld().Replace("'", "''");
            string join = _join == null ? string.Empty : _join.ToString().Replace("'", "''");
            string where = _where == null ? string.Empty : _where.ToString().Replace("'", "''");
            StringBuilder sb = new StringBuilder();
            sb.Append("declare @select varchar(8000);\n");
            sb.Append("set @select ='select count(1)  from " + _tablename + " " + (join) + " " + (where) + ";");
            sb.Append("select * from (select " + fld + ",ROW_NUMBER() over(" + _order + ") as rowNumber  from " + _tablename + " " + (join) + " " + (where) + " )  " + _tablename + " where  rowNumber between (((" + _page + " - 1) * " + _pagesize + ")+1) and (" + _page + "*" + _pagesize + ")';\n");
            sb.Append("exec(@select);\n");
            return sb.ToString();
        }

        public DataArray SelectRowNumber()
        {
            return SelectRowNumber(null);
        }

        public DataArray SelectRowNumber(string sql)
        {
            sql = sql ?? SqlRowNumber();

            if (_pagesize == 0) _pagesize = 20;
            DataArray data = new DataArray(_name, _pagesize) { PageSize = _pagesize };

            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                Open();
                cmd = new SqlCommand(sql, _conn) { CommandTimeout = _commandTimeOut };

                reader = cmd.ExecuteReader();
                reader.Read();
                _maxcount = reader.GetInt32(0);
                data.Page = Convert.ToInt32(Math.Ceiling((double)_maxcount / _pagesize));
                data.MaxCount = _maxcount;
                reader.NextResult();

                reader.Exec(data, false);
            }
            catch (SqlException ex)
            {
                LoggerWrapper.Logger.Error("Conn.SelectRowNumber��������" + sql, ex);
                throw ex;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (cmd != null) cmd.Dispose();
            }
            return data;
        }

        #endregion RowNumber��ҳ��ѯ SelectRowNumber

        public void Dispose()
        {
            Close();
        }
    }
}