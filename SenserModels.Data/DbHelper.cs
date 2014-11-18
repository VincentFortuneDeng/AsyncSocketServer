using System;
using System.Data;
using System.Xml;
using System.Data.Common;
using System.Collections;

using SenserModels.Config;

namespace SenserModels.Data
{

#if NET1 

    #region 数据访问助手类  For NET1.0
	/// <summary>
	/// 数据访问助手类
	/// </summary>
	public class DbHelper
	{
    #region 私有变量
  
		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		protected static string m_connectionstring = null ;

		/// <summary>
		/// DbProviderFactory实例
		/// </summary>
		private static IDbProviderFactory m_factory = null;

		/// <summary>
		/// CnTaxLawyer数据接口
		/// </summary>
		private static IDbProvider m_provider = null;

		/// <summary>
		/// 查询次数统计
		/// </summary>
		private static int m_querycount = 0;

		/// <summary>
		/// Parameters缓存哈希表
		/// </summary>
		private static Hashtable m_paramcache = Hashtable.Synchronized(new Hashtable());
		private static object lockHelper = new object();

    #endregion
       
    #region 属性
		
		/// <summary>
        /// 查询次数统计
        /// </summary>
        public static int QueryCount
        {
            get { return m_querycount; }
            set { m_querycount = value; }
        }

		/// <summary>
		/// 数据库连接字符串
		/// </summary>
		public static string ConnectionString
		{
			get
			{
				if (m_connectionstring == null)
				{
					m_connectionstring = BaseConfigs.GetDBConnectString;
				}
				return m_connectionstring;
			}
			set 
			{
				m_connectionstring = value;
			}
		}

		/// <summary>
		/// IDbProvider接口
		/// </summary>
		public static IDbProvider Provider
		{
			get
			{
				if (m_provider == null)
				{
					lock(lockHelper)
					{
						if (m_provider == null)
						{
							try
							{
								m_provider = (IDbProvider)Activator.CreateInstance(Type.GetType(string.Format("SenserModels.Data.{0}Provider, SenserModels.Data.{0}", BaseConfigs.GetDbType)));
							}
							catch
							{
								throw new Exception("请检查SenserModelsApp.config中Dbtype节点数据库类型是否正确，例如：SqlServer、Access、MySql，注意大小写。");
							}
                            
						}
					}
                    
					//m_provider = new DbProviderFinder().GetDbProvider("accesss");
            
                    
				}
				return m_provider;
			}
		}

		/// <summary>
		/// DbFactory实例
		/// </summary>
		public static IDbProviderFactory Factory
		{
			get
			{
				if (m_factory == null)
				{
					m_factory = Provider.Instance();
				}
				return m_factory;
			}
		}

        /// <summary>
        /// 刷新数据库提供者
        /// </summary>
        public static void ResetDbProvider()
        {
            BaseConfigs.ResetConfig();
            DatabaseProvider.ResetDbProvider();
            m_connectionstring = null;
            m_factory = null;
            m_provider = null;
        }

    #endregion

    #region 私有方法

		/// <summary>
		/// 将IDataParameter参数数组(参数值)分配给IDbCommand命令.
		/// 这个方法将给任何一个参数分配DBNull.Value;
		/// 该操作将阻止默认值的使用.
		/// </summary>
		/// <param name="command">命令名</param>
		/// <param name="commandParameters">IDataParameters数组</param>
		private static void AttachParameters(IDbCommand command, IDataParameter[] commandParameters)
		{
			if (command == null) throw new ArgumentNullException("command");
			if (commandParameters != null)
			{
				foreach (IDataParameter p in commandParameters)
				{
					if (p != null)
					{
						// 检查未分配值的输出参数,将其分配以DBNull.Value.
						if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) &&
							(p.Value == null))
						{
							p.Value = DBNull.Value;
						}
						command.Parameters.Add(p);
					}
				}

			}
		}

		/// <summary>
		/// 将DataRow类型的列值分配到IDataParameter参数数组.
		/// </summary>
		/// <param name="commandParameters">要分配值的IDataParameter参数数组</param>
		/// <param name="dataRow">将要分配给存储过程参数的DataRow</param>
		private static void AssignParameterValues(IDataParameter[] commandParameters, DataRow dataRow)
		{
			if ((commandParameters == null) || (dataRow == null))
			{
				return;
			}

			int i = 0;
			// 设置参数值
			foreach (IDataParameter commandParameter in commandParameters)
			{
				// 创建参数名称,如果不存在,只抛出一个异常.
				if (commandParameter.ParameterName == null ||
					commandParameter.ParameterName.Length <= 1)
					throw new Exception(
						string.Format("请提供参数{0}一个有效的名称{1}.", i, commandParameter.ParameterName));
				// 从dataRow的表中获取为参数数组中数组名称的列的索引.
				// 如果存在和参数名称相同的列,则将列值赋给当前名称的参数.
				if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
					commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
				i++;
			}
		}

		/// <summary>
		/// 将一个对象数组分配给IDataParameter参数数组.
		/// </summary>
		/// <param name="commandParameters">要分配值的IDataParameter参数数组</param>
		/// <param name="parameterValues">将要分配给存储过程参数的对象数组</param>
		private static void AssignParameterValues(IDataParameter[] commandParameters, object[] parameterValues)
		{
			if ((commandParameters == null) || (parameterValues == null))
			{
				return;
			}

			// 确保对象数组个数与参数个数匹配,如果不匹配,抛出一个异常.
			if (commandParameters.Length != parameterValues.Length)
			{
				throw new ArgumentException("参数值个数与参数不匹配.");
			}

			// 给参数赋值
			for (int i = 0, j = commandParameters.Length; i < j; i++)
			{
				// If the current array value derives from IDbDataParameter, then assign its Value property
				if (parameterValues[i] is IDbDataParameter)
				{
					IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
					if (paramInstance.Value == null)
					{
						commandParameters[i].Value = DBNull.Value;
					}
					else
					{
						commandParameters[i].Value = paramInstance.Value;
					}
				}
				else if (parameterValues[i] == null)
				{
					commandParameters[i].Value = DBNull.Value;
				}
				else
				{
					commandParameters[i].Value = parameterValues[i];
				}
			}
		}

		/// <summary>
		/// 预处理用户提供的命令,数据库连接/事务/命令类型/参数
		/// </summary>
		/// <param name="command">要处理的IDbCommand</param>
		/// <param name="connection">数据库连接</param>
		/// <param name="transaction">一个有效的事务或者是null值</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
		/// <param name="commandText">存储过程名或都SQL命令文本</param>
		/// <param name="commandParameters">和命令相关联的IDataParameter参数数组,如果没有参数为'null'</param>
		/// <param name="mustCloseConnection"><c>true</c> 如果连接是打开的,则为true,其它情况下为false.</param>
		private static void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, out bool mustCloseConnection)
		{
			if (command == null) throw new ArgumentNullException("command");
			if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

			// If the provided connection is not open, we will open it
			if (connection.State != ConnectionState.Open)
			{
				mustCloseConnection = true;
				connection.Open();
			}
			else
			{
				mustCloseConnection = false;
			}

			// 给命令分配一个数据库连接.
			command.Connection = connection;

			// 设置命令文本(存储过程名或SQL语句)
			command.CommandText = commandText;

			// 分配事务
			if (transaction != null)
			{
				if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
				command.Transaction = transaction;
			}

			// 设置命令类型.
			command.CommandType = commandType;

			// 分配命令参数
			if (commandParameters != null)
			{
				AttachParameters(command, commandParameters);
			}
			return;
		}

		/// <summary>
		/// 探索运行时的存储过程,返回IDataParameter参数数组.
		/// 初始化参数值为 DBNull.Value.
		/// </summary>
		/// <param name="connection">一个有效的数据库连接</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="includeReturnValueParameter">是否包含返回值参数</param>
		/// <returns>返回IDataParameter参数数组</returns>
		private static IDataParameter[] DiscoverSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			IDbCommand cmd = connection.CreateCommand();
			cmd.CommandText = spName;
			cmd.CommandType = CommandType.StoredProcedure;

			connection.Open();
			// 检索cmd指定的存储过程的参数信息,并填充到cmd的Parameters参数集中.
			Provider.DeriveParameters(cmd);
			connection.Close();
			// 如果不包含返回值参数,将参数集中的每一个参数删除.
			if (!includeReturnValueParameter)
			{
				cmd.Parameters.RemoveAt(0);
			}

			// 创建参数数组
			IDataParameter[] discoveredParameters = new IDataParameter[cmd.Parameters.Count];
			// 将cmd的Parameters参数集复制到discoveredParameters数组.
			cmd.Parameters.CopyTo(discoveredParameters, 0);

			// 初始化参数值为 DBNull.Value.
			foreach (IDataParameter discoveredParameter in discoveredParameters)
			{
				discoveredParameter.Value = DBNull.Value;
			}
			return discoveredParameters;
		}

		/// <summary>
		/// IDataParameter参数数组的深层拷贝.
		/// </summary>
		/// <param name="originalParameters">原始参数数组</param>
		/// <returns>返回一个同样的参数数组</returns>
		private static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
		{
			IDataParameter[] clonedParameters = new IDataParameter[originalParameters.Length];

			for (int i = 0, j = originalParameters.Length; i < j; i++)
			{
				clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
			}

			return clonedParameters;
		}

    #endregion 私有方法结束

    #region ExecuteNonQuery方法

		/// <summary>
		/// 执行指定连接字符串,类型的IDbCommand.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery("SELECT * FROM [table123]");
		/// </remarks>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(string commandText)
		{
			return ExecuteNonQuery(CommandType.Text, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定连接字符串,类型的IDbCommand.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery("SELECT * FROM [table123]");
		/// </remarks>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(out int id, string commandText)
		{
			return ExecuteNonQuery(out id, CommandType.Text, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定连接字符串,类型的IDbCommand.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定连接字符串,并返回刚插入的自增ID
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(out int id, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(out id, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定连接字符串,类型的IDbCommand.如果没有提供参数,不返回结果.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">IDataParameter参数数组</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");

			using (IDbConnection connection =  Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// 执行指定连接字符串并返回刚插入的自增ID,类型的IDbCommand.如果没有提供参数,不返回结果.
		/// </summary>
		/// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">IDataParameter参数数组</param>
		/// <returns>返回命令影响的行数</returns>
		public static int ExecuteNonQuery(out int id, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
       
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");

			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				return ExecuteNonQuery(out id, connection, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令 
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(connection, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令并返回自增ID 
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(out int id, IDbConnection connection, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(out id, connection, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">T存储过程名称或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			// 创建IDbCommand命令,并进行预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

			// Finally, execute the command
			int retval = cmd.ExecuteNonQuery();

			// 清除参数,以便再次使用.
			cmd.Parameters.Clear();
			if (mustCloseConnection)
				connection.Close();
			return retval;
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">T存储过程名称或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(out int id, IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (Provider.GetLastIdSql().Trim() == "") throw new ArgumentNullException("GetLastIdSql is \"\"");

			// 创建IDbCommand命令,并进行预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行命令
			int retval = cmd.ExecuteNonQuery();
			// 清除参数,以便再次使用.
			cmd.Parameters.Clear();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = Provider.GetLastIdSql();
			id = int.Parse(cmd.ExecuteScalar().ToString());

            m_querycount++;
			if (mustCloseConnection)
			{
				connection.Close();
			}
			return retval;
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,将对象数组的值赋给存储过程参数.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
		{
			if (ConnectionString == null) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 给存储过程分配参数值
				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行带事务的IDbCommand.
		/// </summary>
		/// <remarks>
		/// 示例.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="transaction">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回影响的行数/returns>
		public static int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(transaction, commandType, commandText, (IDataParameter[])null);
		}


		/// <summary>
		/// 执行带事务的IDbCommand.
		/// </summary>
		/// <remarks>
		/// 示例.:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
		/// </remarks>
		/// <param name="transaction">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回影响的行数/returns>
		public static int ExecuteNonQuery(out int id, IDbTransaction transaction, CommandType commandType, string commandText)
		{
			return ExecuteNonQuery(out id, transaction, commandType, commandText, (IDataParameter[])null);
		}


		/// <summary>
		/// 执行带事务的IDbCommand(指定参数).
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

			// 预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行
			int retval = cmd.ExecuteNonQuery();

            m_querycount++;

			// 清除参数集,以便再次使用.
			cmd.Parameters.Clear();
			return retval;
		}

		/// <summary>
		/// 执行带事务的IDbCommand(指定参数).
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQuery(out int id, IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

			// 预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行
			int retval = cmd.ExecuteNonQuery();
			m_querycount++;
			
			// 清除参数,以便再次使用.
			cmd.Parameters.Clear();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = Provider.GetLastIdSql();
			id = int.Parse(cmd.ExecuteScalar().ToString());
			return retval;
		}

		/// <summary>
		/// 执行带事务的IDbCommand(指定参数值).
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值
		/// 示例:  
		///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回受影响的行数</returns>
		public static int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 给存储过程参数赋值
				AssignParameterValues(commandParameters, parameterValues);

				// 调用重载方法
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				// 没有参数值
				return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
			}
		}

    #endregion ExecuteNonQuery方法结束

    #region ExecuteCommandWithSplitter方法
		/// <summary>
		/// 运行含有GO命令的多条SQL命令
		/// </summary>
		/// <param name="commandText">SQL命令字符串</param>
		/// <param name="splitter">分割字符串</param>
		public static void ExecuteCommandWithSplitter(string commandText, string splitter)
		{
			int startPos = 0;

			do
			{
				int lastPos = commandText.IndexOf(splitter, startPos);
				int len = (lastPos > startPos ? lastPos : commandText.Length) - startPos;
				string query = commandText.Substring(startPos, len);

				if (query.Trim().Length > 0)
				{
					ExecuteNonQuery(CommandType.Text, query);
				}

				if (lastPos == -1)
					break;
				else
					startPos = lastPos + splitter.Length;
			} while (startPos < commandText.Length);

		}

		/// <summary>
		/// 运行含有GO命令的多条SQL命令
		/// </summary>
		/// <param name="commandText">SQL命令字符串</param>
		public static void ExecuteCommandWithSplitter(string commandText)
		{
			ExecuteCommandWithSplitter(commandText, "\r\nGO\r\n");
		}
    #endregion ExecuteCommandWithSplitter方法结束

    #region ExecuteDataset方法


		/// <summary>
		/// 执行指定数据库连接字符串的命令,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset("SELECT * FROM [table1]");
		/// </remarks>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(string commandText)
		{
			return ExecuteDataset(CommandType.Text, commandText, (IDataParameter[])null);
		}


		/// <summary>
		/// 执行指定数据库连接字符串的命令,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(CommandType commandType, string commandText)
		{
			return ExecuteDataset(commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例: 
		///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">SqlParamters参数数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
          
			// 创建并打开数据库连接对象,操作完成释放对象.
            
			//using (IDbConnection connection = (IDbConnection)new System.Data.SqlClient.SqlConnection(ConnectionString))
			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				// 调用指定数据库连接字符串重载方法.
				return ExecuteDataset(connection, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,直接提供参数值,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值.
		/// 示例: 
		///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(string spName, params object[] parameterValues)
		{
		    if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
		    if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

		    if ((parameterValues != null) && (parameterValues.Length > 0))
		    {
		        // 从缓存中检索存储过程参数
		        IDataParameter[] commandParameters = GetSpParameterSet(spName);

		        // 给存储过程参数分配值
		        AssignParameterValues(commandParameters, parameterValues);

		        return ExecuteDataset(CommandType.StoredProcedure, spName, commandParameters);
		    }
		    else
		    {
		        return ExecuteDataset(CommandType.StoredProcedure, spName);
		    }
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
		{
			return ExecuteDataset(connection, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,指定存储过程参数,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			// 预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

			// 创建IDbDataAdapter和DataSet.
			//using (IDbDataAdapter da = Factory.CreateDataAdapter())
			IDbDataAdapter da = Factory.CreateDataAdapter();
			{
				da.SelectCommand = cmd;
				DataSet ds = new DataSet();

				// 填充DataSet.
				da.Fill(ds);
				
                m_querycount++;

				cmd.Parameters.Clear();

				if (mustCloseConnection)
					connection.Close();

				return ds;
			}
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,指定参数值,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输入参数和返回值.
		/// 示例.:  
		///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 比缓存中加载存储过程参数
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 给存储过程参数分配值
				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定事务的命令,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
		{
			return ExecuteDataset(transaction, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定事务的命令,指定参数,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

			// 预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

			// 创建 DataAdapter & DataSet
			//using (IDbDataAdapter da = Factory.CreateDataAdapter())
			IDbDataAdapter da = Factory.CreateDataAdapter();
			{
				da.SelectCommand = cmd;
				DataSet ds = new DataSet();
				da.Fill(ds);
				
				m_querycount++;
				
				cmd.Parameters.Clear();
				return ds;
			}
		}

		/// <summary>
		/// 执行指定事务的命令,指定参数值,返回DataSet.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输入参数和返回值.
		/// 示例.:  
		///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">事务</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回一个包含结果集的DataSet</returns>
		public static DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 给存储过程参数分配值
				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
			}
		}

    #endregion ExecuteDataset数据集命令结束

    #region ExecuteReader 数据阅读器

		/// <summary>
		/// 枚举,标识数据库连接是由BaseDbHelper提供还是由调用者提供
		/// </summary>
		private enum IDbConnectionOwnership
		{
			/// <summary>由BaseDbHelper提供连接</summary>
			Internal,
			/// <summary>由调用者提供连接</summary>
			External
		}

		/// <summary>
		/// 执行指定数据库连接对象的数据阅读器.
		/// </summary>
		/// <remarks>
		/// 如果是BaseDbHelper打开连接,当连接关闭DataReader也将关闭.
		/// 如果是调用都打开连接,DataReader由调用都管理.
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="transaction">一个有效的事务,或者为 'null'</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <param name="commandParameters">IDataParameters参数数组,如果没有参数则为'null'</param>
		/// <param name="connectionOwnership">标识数据库连接对象是由调用者提供还是由BaseDbHelper提供</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		private static IDataReader ExecuteReader(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, IDbConnectionOwnership connectionOwnership)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			bool mustCloseConnection = false;
			// 创建命令
			IDbCommand cmd = Factory.CreateCommand();
			try
			{
				PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

				// 创建数据阅读器
				IDataReader dataReader;

				if (connectionOwnership == IDbConnectionOwnership.External)
				{
					dataReader = cmd.ExecuteReader();
				}
				else
				{
					dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				}
				
                m_querycount++;

				// 清除参数,以便再次使用..
				// HACK: There is a problem here, the output parameter values are fletched 
				// when the reader is closed, so if the parameters are detached from the command
				// then the SqlReader can磘 set its values. 
				// When this happen, the parameters can磘 be used again in other command.
				bool canClear = true;
				foreach (IDataParameter commandParameter in cmd.Parameters)
				{
					if (commandParameter.Direction != ParameterDirection.Input)
						canClear = false;
				}
				
				
				if (canClear)
				{
					cmd.Dispose();
					cmd.Parameters.Clear();
				}

				return dataReader;
			}
			catch
			{
				if (mustCloseConnection)
					connection.Close();
				throw;
			}
		}

		/// <summary>
		/// 执行指定数据库连接字符串的数据阅读器.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(CommandType commandType, string commandText)
		{
			return ExecuteReader(commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接字符串的数据阅读器,指定参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <param name="commandParameters">SqlParamter参数数组(new IDataParameter("@prodid", 24))</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			IDbConnection connection = null;
			try
			{
				connection = Factory.CreateConnection();
				connection.ConnectionString = ConnectionString;
				connection.Open();

				return ExecuteReader(connection, null, commandType, commandText, commandParameters, IDbConnectionOwnership.Internal);
			}
			catch
			{
				// If we fail to return the SqlDatReader, we need to close the connection ourselves
				if (connection != null) connection.Close();
				throw;
			}

		}

		/// <summary>
		/// 执行指定数据库连接字符串的数据阅读器,指定参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 示例:  
		///  IDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(string spName, params object[] parameterValues)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				IDataParameter[] commandParameters = GetSpParameterSet(spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return ExecuteReader(CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定数据库连接对象的数据阅读器.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
		{
			return ExecuteReader(connection, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandParameters">SqlParamter参数数组</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			return ExecuteReader(connection, (IDbTransaction)null, commandType, commandText, commandParameters, IDbConnectionOwnership.External);
		}

		/// <summary>
		/// [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 示例:  
		///  IDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">T存储过程名</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return ExecuteReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
		{
			return ExecuteReader(transaction, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// [调用者方式]执行指定数据库事务的数据阅读器,指定参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///   IDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

			return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, IDbConnectionOwnership.External);
		}

		/// <summary>
		/// [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  IDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				AssignParameterValues(commandParameters, parameterValues);

				return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				// 没有参数值
				return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
			}
		}

    #endregion ExecuteReader数据阅读器

    #region ExecuteScalar 返回结果集中的第一行第一列

		/// <summary>
		/// 执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(CommandType commandType, string commandText)
		{
			// 执行参数为空的方法
			return ExecuteScalar(commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,指定参数,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			// 创建并打开数据库连接对象,操作完成释放对象.
			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				// 调用指定数据库连接字符串重载方法.
				return ExecuteScalar(connection, commandType, commandText, commandParameters);
			}
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,指定参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(string spName, params object[] parameterValues)
		{
		    if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
		    if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

		    // 如果有参数值
		    if ((parameterValues != null) && (parameterValues.Length > 0))
		    {
		        // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
		        IDataParameter[] commandParameters = GetSpParameterSet(spName);

		        // 给存储过程参数赋值
		        AssignParameterValues(commandParameters, parameterValues);

		        // 调用重载方法
		        return ExecuteScalar(CommandType.StoredProcedure, spName, commandParameters);
		    }
		    else
		    {
		        // 没有参数值
		        return ExecuteScalar(CommandType.StoredProcedure, spName);
		    }
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
		{
			// 执行参数为空的方法
			return ExecuteScalar(connection, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			// 创建IDbCommand命令,并进行预处理
			IDbCommand cmd = Factory.CreateCommand();

			bool mustCloseConnection = false;
			PrepareCommand(cmd, connection, (IDbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行IDbCommand命令,并返回结果.
			object retval = cmd.ExecuteScalar();

            m_querycount++;

			// 清除参数,以便再次使用.
			cmd.Parameters.Clear();

			if (mustCloseConnection)
				connection.Close();

			return retval;
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,指定参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 给存储过程参数赋值
				AssignParameterValues(commandParameters, parameterValues);

				// 调用重载方法
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				// 没有参数值
				return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定数据库事务的命令,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
		{
			// 执行参数为空的方法
			return ExecuteScalar(transaction, commandType, commandText, (IDataParameter[])null);
		}

		/// <summary>
		/// 执行指定数据库事务的命令,指定参数,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

			// 创建IDbCommand命令,并进行预处理
			IDbCommand cmd = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行IDbCommand命令,并返回结果.
			object retval = cmd.ExecuteScalar();
            m_querycount++;

			// 清除参数,以便再次使用.
			cmd.Parameters.Clear();
			return retval;
		}

		/// <summary>
		/// 执行指定数据库事务的命令,指定参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// PPull the parameters for this stored procedure from the parameter cache ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 给存储过程参数赋值
				AssignParameterValues(commandParameters, parameterValues);

				// 调用重载方法
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				// 没有参数值
				return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
			}
		}

    #endregion ExecuteScalar

    #region FillDataset 填充数据集
		/// <summary>
		/// 执行指定数据库连接字符串的命令,映射数据表并填充数据集.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)</param>
		public static void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (dataSet == null) throw new ArgumentNullException("dataSet");

			// 创建并打开数据库连接对象,操作完成释放对象.
			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				// 调用指定数据库连接字符串重载方法.
				FillDataset(connection, commandType, commandText, dataSet, tableNames);
			}
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,映射数据表并填充数据集.指定命令参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		public static void FillDataset(CommandType commandType,
			string commandText, DataSet dataSet, string[] tableNames,
			params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (dataSet == null) throw new ArgumentNullException("dataSet");
			// 创建并打开数据库连接对象,操作完成释放对象.
			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				// 调用指定数据库连接字符串重载方法.
				FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
			}
		}

		/// <summary>
		/// 执行指定数据库连接字符串的命令,映射数据表并填充数据集,指定存储过程参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>    
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		public static void FillDataset(string spName,
			DataSet dataSet, string[] tableNames,
			params object[] parameterValues)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (dataSet == null) throw new ArgumentNullException("dataSet");
			// 创建并打开数据库连接对象,操作完成释放对象.
			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				// 调用指定数据库连接字符串重载方法.
				FillDataset(connection, spName, dataSet, tableNames, parameterValues);
			}
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,映射数据表并填充数据集.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>    
		public static void FillDataset(IDbConnection connection, CommandType commandType,
			string commandText, DataSet dataSet, string[] tableNames)
		{
			FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,映射数据表并填充数据集,指定参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		public static void FillDataset(IDbConnection connection, CommandType commandType,
			string commandText, DataSet dataSet, string[] tableNames,
			params IDataParameter[] commandParameters)
		{
			FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
		}

		/// <summary>
		/// 执行指定数据库连接对象的命令,映射数据表并填充数据集,指定存储过程参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		public static void FillDataset(IDbConnection connection, string spName,
			DataSet dataSet, string[] tableNames,
			params object[] parameterValues)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (dataSet == null) throw new ArgumentNullException("dataSet");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 给存储过程参数赋值
				AssignParameterValues(commandParameters, parameterValues);

				// 调用重载方法
				FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
			}
			else
			{
				// 没有参数值
				FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
			}
		}

		/// <summary>
		/// 执行指定数据库事务的命令,映射数据表并填充数据集.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		public static void FillDataset(IDbTransaction transaction, CommandType commandType,
			string commandText,
			DataSet dataSet, string[] tableNames)
		{
			FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
		}

		/// <summary>
		/// 执行指定数据库事务的命令,映射数据表并填充数据集,指定参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		public static void FillDataset(IDbTransaction transaction, CommandType commandType,
			string commandText, DataSet dataSet, string[] tableNames,
			params IDataParameter[] commandParameters)
		{
			FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
		}

		/// <summary>
		/// 执行指定数据库事务的命令,映射数据表并填充数据集,指定存储过程参数值.
		/// </summary>
		/// <remarks>
		/// 此方法不提供访问存储过程输出参数和返回值参数.
		/// 
		/// 示例:  
		///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
		/// </remarks>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		/// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
		public static void FillDataset(IDbTransaction transaction, string spName,
			DataSet dataSet, string[] tableNames,
			params object[] parameterValues)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (dataSet == null) throw new ArgumentNullException("dataSet");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果有参数值
			if ((parameterValues != null) && (parameterValues.Length > 0))
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 给存储过程参数赋值
				AssignParameterValues(commandParameters, parameterValues);

				// 调用重载方法
				FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
			}
			else
			{
				// 没有参数值
				FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
			}
		}

		/// <summary>
		/// [私有方法][内部调用]执行指定数据库连接对象/事务的命令,映射数据表并填充数据集,DataSet/TableNames/IDataParameters.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new IDataParameter("@prodid", 24));
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="transaction">一个有效的连接事务</param>
		/// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
		/// <param name="commandText">存储过程名称或SQL语句</param>
		/// <param name="dataSet">要填充结果集的DataSet实例</param>
		/// <param name="tableNames">表映射的数据表数组
		/// 用户定义的表名 (可有是实际的表名.)
		/// </param>
		/// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
		private static void FillDataset(IDbConnection connection, IDbTransaction transaction, CommandType commandType,
			string commandText, DataSet dataSet, string[] tableNames,
			params IDataParameter[] commandParameters)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (dataSet == null) throw new ArgumentNullException("dataSet");

			// 创建IDbCommand命令,并进行预处理
			IDbCommand command = Factory.CreateCommand();
			bool mustCloseConnection = false;
			PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

			// 执行命令
			//using (IDbDataAdapter dataAdapter = Factory.CreateDataAdapter())
			IDbDataAdapter dataAdapter = Factory.CreateDataAdapter();
			{
				dataAdapter.SelectCommand = command;
				// 追加表映射
				if (tableNames != null && tableNames.Length > 0)
				{
					string tableName = "Table";
					for (int index = 0; index < tableNames.Length; index++)
					{
						if (tableNames[index] == null || tableNames[index].Length == 0) throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
						dataAdapter.TableMappings.Add(tableName, tableNames[index]);
						tableName = "Table" + (index + 1).ToString();
					}
				}

				// 填充数据集使用默认表名称
				dataAdapter.Fill(dataSet);

                m_querycount++;

				// 清除参数,以便再次使用.
				command.Parameters.Clear();
			}

			if (mustCloseConnection)
				connection.Close();
		}
    #endregion

    #region UpdateDataset 更新数据集
		/// <summary>
		/// 执行数据集更新到数据库,指定inserted, updated, or deleted命令.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
		/// </remarks>
		/// <param name="insertCommand">[追加记录]一个有效的SQL语句或存储过程</param>
		/// <param name="deleteCommand">[删除记录]一个有效的SQL语句或存储过程</param>
		/// <param name="updateCommand">[更新记录]一个有效的SQL语句或存储过程</param>
		/// <param name="dataSet">要更新到数据库的DataSet</param>
		/// <param name="tableName">要更新到数据库的DataTable</param>
		public static void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand, DataSet dataSet, string tableName)
		{
			if (insertCommand == null) throw new ArgumentNullException("insertCommand");
			if (deleteCommand == null) throw new ArgumentNullException("deleteCommand");
			if (updateCommand == null) throw new ArgumentNullException("updateCommand");
			if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");

			// 创建IDbDataAdapter,当操作完成后释放.
			//using (IDbDataAdapter dataAdapter = Factory.CreateDataAdapter())
			IDbDataAdapter dataAdapter = Factory.CreateDataAdapter();
			{
				// 设置数据适配器命令
				dataAdapter.UpdateCommand = updateCommand;
				dataAdapter.InsertCommand = insertCommand;
				dataAdapter.DeleteCommand = deleteCommand;

				// 更新数据集改变到数据库
				dataAdapter.Update(dataSet);

				// 提交所有改变到数据集.
				dataSet.AcceptChanges();
			}
		}
    #endregion

    #region CreateCommand 创建一条IDbCommand命令
		/// <summary>
		/// 创建IDbCommand命令,指定数据库连接对象,存储过程名和参数.
		/// </summary>
		/// <remarks>
		/// 示例:  
		///  IDbCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="sourceColumns">源表的列名称数组</param>
		/// <returns>返回IDbCommand命令</returns>
		public static IDbCommand CreateCommand(IDbConnection connection, string spName, params string[] sourceColumns)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 创建命令
			IDbCommand cmd = Factory.CreateCommand();
			cmd.CommandText = spName;
			cmd.Connection = connection;
			cmd.CommandType = CommandType.StoredProcedure;

			// 如果有参数值
			if ((sourceColumns != null) && (sourceColumns.Length > 0))
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 将源表的列到映射到DataSet命令中.
				for (int index = 0; index < sourceColumns.Length; index++)
					commandParameters[index].SourceColumn = sourceColumns[index];

				// Attach the discovered parameters to the IDbCommand object
				AttachParameters(cmd, commandParameters);
			}

			return cmd;
		}
    #endregion

    #region ExecuteNonQueryTypedParams 类型化参数(DataRow)
		/// <summary>
		/// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回受影响的行数.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQueryTypedParams(String spName, DataRow dataRow)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回受影响的行数.
		/// </summary>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQueryTypedParams(IDbConnection connection, String spName, DataRow dataRow)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回受影响的行数.
		/// </summary>
		/// <param name="transaction">一个有效的连接事务 object</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回影响的行数</returns>
		public static int ExecuteNonQueryTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// Sf the row has values, the store procedure parameters must be initialized
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
			}
		}
    #endregion

    #region ExecuteDatasetTypedParams 类型化参数(DataRow)
		/// <summary>
		/// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataSet.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回一个包含结果集的DataSet.</returns>
		public static DataSet ExecuteDatasetTypedParams(String spName, DataRow dataRow)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			//如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteDataset(CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteDataset(CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataSet.
		/// </summary>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回一个包含结果集的DataSet.</returns>
		/// 
		public static DataSet ExecuteDatasetTypedParams(IDbConnection connection, String spName, DataRow dataRow)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回DataSet.
		/// </summary>
		/// <param name="transaction">一个有效的连接事务 object</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回一个包含结果集的DataSet.</returns>
		public static DataSet ExecuteDatasetTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
			}
		}

    #endregion

    #region ExecuteReaderTypedParams 类型化参数(DataRow)
		/// <summary>
		/// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataReader.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReaderTypedParams(String spName, DataRow dataRow)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, spName);
			}
		}


		/// <summary>
		/// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataReader.
		/// </summary>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReaderTypedParams(IDbConnection connection, String spName, DataRow dataRow)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回DataReader.
		/// </summary>
		/// <param name="transaction">一个有效的连接事务 object</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回包含结果集的IDataReader</returns>
		public static IDataReader ExecuteReaderTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
			}
		}
    #endregion

    #region ExecuteScalarTypedParams 类型化参数(DataRow)
		/// <summary>
		/// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalarTypedParams(String spName, DataRow dataRow)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteScalar(CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteScalar(CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalarTypedParams(IDbConnection connection, String spName, DataRow dataRow)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
			}
		}

		/// <summary>
		/// 执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
		/// </summary>
		/// <param name="transaction">一个有效的连接事务 object</param>
		/// <param name="spName">存储过程名称</param>
		/// <param name="dataRow">使用DataRow作为参数值</param>
		/// <returns>返回结果集中的第一行第一列</returns>
		public static object ExecuteScalarTypedParams(IDbTransaction transaction, String spName, DataRow dataRow)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			// 如果row有值,存储过程必须初始化.
			if (dataRow != null && dataRow.ItemArray.Length > 0)
			{
				// 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
				IDataParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

				// 分配参数值
				AssignParameterValues(commandParameters, dataRow);

				return DbHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
			}
			else
			{
				return DbHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
			}
		}
    #endregion

    #region 缓存方法

		/// <summary>
		/// 追加参数数组到缓存.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符串</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <param name="commandParameters">要缓存的参数数组</param>
		public static void CacheParameterSet(string commandText, params IDataParameter[] commandParameters)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

			string hashKey = ConnectionString + ":" + commandText;

			m_paramcache[hashKey] = commandParameters;
		}

		/// <summary>
		/// 从缓存中获取参数数组.
		/// </summary>
		/// <param name="ConnectionString">一个有效的数据库连接字符</param>
		/// <param name="commandText">存储过程名或SQL语句</param>
		/// <returns>参数数组</returns>
		public static IDataParameter[] GetCachedParameterSet(string commandText)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

			string hashKey = ConnectionString + ":" + commandText;

			IDataParameter[] cachedParameters = m_paramcache[hashKey] as IDataParameter[];
			if (cachedParameters == null)
			{
				return null;
			}
			else
			{
				return CloneParameters(cachedParameters);
			}
		}

    #endregion 缓存方法结束

    #region 检索指定的存储过程的参数集

		/// <summary>
		/// 返回指定的存储过程的参数集
		/// </summary>
		/// <remarks>
		/// 这个方法将查询数据库,并将信息存储到缓存.
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符</param>
		/// <param name="spName">存储过程名</param>
		/// <returns>返回IDataParameter参数数组</returns>
		public static IDataParameter[] GetSpParameterSet(string spName)
		{
			return GetSpParameterSet(spName, false);
		}

		/// <summary>
		/// 返回指定的存储过程的参数集
		/// </summary>
		/// <remarks>
		/// 这个方法将查询数据库,并将信息存储到缓存.
		/// </remarks>
		/// <param name="ConnectionString">一个有效的数据库连接字符.</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="includeReturnValueParameter">是否包含返回值参数</param>
		/// <returns>返回IDataParameter参数数组</returns>
		public static IDataParameter[] GetSpParameterSet(string spName, bool includeReturnValueParameter)
		{
			if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			using (IDbConnection connection = Factory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
			}
		}

		/// <summary>
		/// [内部]返回指定的存储过程的参数集(使用连接对象).
		/// </summary>
		/// <remarks>
		/// 这个方法将查询数据库,并将信息存储到缓存.
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接字符</param>
		/// <param name="spName">存储过程名</param>
		/// <returns>返回IDataParameter参数数组</returns>
		internal static IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName)
		{
			return GetSpParameterSet(connection, spName, false);
		}

		/// <summary>
		/// [内部]返回指定的存储过程的参数集(使用连接对象)
		/// </summary>
		/// <remarks>
		/// 这个方法将查询数据库,并将信息存储到缓存.
		/// </remarks>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="includeReturnValueParameter">
		/// 是否包含返回值参数
		/// </param>
		/// <returns>返回IDataParameter参数数组</returns>
		internal static IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			using (IDbConnection clonedConnection = (IDbConnection)((ICloneable)connection).Clone())
			{
				return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
			}
		}

		/// <summary>
		/// [私有]返回指定的存储过程的参数集(使用连接对象)
		/// </summary>
		/// <param name="connection">一个有效的数据库连接对象</param>
		/// <param name="spName">存储过程名</param>
		/// <param name="includeReturnValueParameter">是否包含返回值参数</param>
		/// <returns>返回IDataParameter参数数组</returns>
		private static IDataParameter[] GetSpParameterSetInternal(IDbConnection connection, string spName, bool includeReturnValueParameter)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

			string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

			IDataParameter[] cachedParameters;

			cachedParameters = m_paramcache[hashKey] as IDataParameter[];
			if (cachedParameters == null)
			{
				IDataParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
				m_paramcache[hashKey] = spParameters;
				cachedParameters = spParameters;
			}

			return CloneParameters(cachedParameters);
		}

    #endregion 参数集检索结束

    #region 生成参数

		public static IDataParameter MakeInParam(string ParamName, DbType DbType, int Size, object Value)
		{
			return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
		}

		public static IDataParameter MakeOutParam(string ParamName, DbType DbType, int Size)
		{
			return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
		}

		public static IDataParameter MakeParam(string ParamName, DbType DbType, Int32 Size, ParameterDirection Direction, object Value)
		{
			IDataParameter param;

			param = Provider.MakeParam(ParamName, DbType, Size);
          
			param.Direction = Direction;
			if (!(Direction == ParameterDirection.Output && Value == null))
				param.Value = Value;

			return param;
		}

    #endregion 生成参数结束

    #region 执行ExecuteScalar,将结果以字符串类型输出。

		public static string ExecuteScalarToStr(CommandType commandType, string commandText)
		{
			object ec = ExecuteScalar(commandType, commandText);
			if (ec == null)
			{
				return "";
			}
			return ec.ToString();
		}

        
		public static string ExecuteScalarToStr(CommandType commandType, string commandText, params IDataParameter[] commandParameters)
		{
			object ec = ExecuteScalar(commandType, commandText, commandParameters);
			if (ec == null)
			{
				return "";
			}
			return ec.ToString();
		}

    
    #endregion

	}
    #endregion

#else
    #region 数据访问助手类  For NET2.0
    /// <summary>
    /// 数据访问助手类
    /// </summary>
    public class DbHelper
    {
        #region 私有变量

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        protected static string m_connectionstring = null;

        /// <summary>
        /// DbProviderFactory实例
        /// </summary>
        private static DbProviderFactory m_factory = null;

        /// <summary>
        /// CnTaxLawyer数据接口
        /// </summary>
        private static IDbProvider m_provider = null;

        /// <summary>
        /// 查询次数统计
        /// </summary>
        private static int m_querycount = 0;
        /// <summary>
        /// Parameters缓存哈希表
        /// </summary>
        private static Hashtable m_paramcache = Hashtable.Synchronized(new Hashtable());
        private static object lockHelper = new object();

        #endregion

#if DEBUG
        private static string m_querydetail = "";
        public static string QueryDetail
        {
            get { return m_querydetail; }
            set { m_querydetail = value; }
        }
        private static string GetQueryDetail(string commandText, DateTime dtStart, DateTime dtEnd, DbParameter[] cmdParams)
        {
            string tr = "<tr style=\"background: rgb(255, 255, 255) none repeat scroll 0%; -moz-background-clip: -moz-initial; -moz-background-origin: -moz-initial; -moz-background-inline-policy: -moz-initial;\">";
            string colums = "";
            string dbtypes = "";
            string values = "";
            string paramdetails = "";
            if (cmdParams != null && cmdParams.Length > 0)
            {
                foreach (DbParameter param in cmdParams)
                {
                    if (param == null)
                    {
                        continue;
                    }

                    colums += "<td>" + param.ParameterName + "</td>";
                    dbtypes += "<td>" + param.DbType.ToString() + "</td>";
                    values += "<td>" + param.Value.ToString() + "</td>";
                }
                paramdetails = string.Format("<table width=\"100%\" cellspacing=\"1\" cellpadding=\"0\" style=\"background: rgb(255, 255, 255) none repeat scroll 0%; margin-top: 5px; font-size: 12px; display: block; -moz-background-clip: -moz-initial; -moz-background-origin: -moz-initial; -moz-background-inline-policy: -moz-initial;\">{0}{1}</tr>{0}{2}</tr>{0}{3}</tr></table>", tr, colums, dbtypes, values);
            }
            return string.Format("<center><div style=\"border: 1px solid black; margin: 2px; padding: 1em; text-align: left; width: 96%; clear: both;\"><div style=\"font-size: 12px; float: right; width: 100px; margin-bottom: 5px;\"><b>TIME:</b> {0}</div><span style=\"font-size: 12px;\">{1}{2}</span></div><br /></center>", dtEnd.Subtract(dtStart).TotalMilliseconds / 1000, commandText, paramdetails);
        }
#endif

        #region 属性

        /// <summary>
        /// 查询次数统计
        /// </summary>
        public static int QueryCount
        {
            get { return m_querycount; }
            set { m_querycount = value; }
        }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (m_connectionstring == null)
                {
                    m_connectionstring = BaseConfigs.GetDBConnectString;
                }
                return m_connectionstring;
            }
            set
            {
                m_connectionstring = value;
            }
        }

        /// <summary>
        /// IDbProvider接口
        /// </summary>
        public static IDbProvider Provider
        {
            get
            {
                if (m_provider == null)
                {
                    lock (lockHelper)
                    {
                        if (m_provider == null)
                        {
                            try
                            {
                                m_provider = (IDbProvider)Activator.CreateInstance(Type.GetType(string.Format("SenserModels.Data.{0}Provider, SenserModels.Data.{0}", BaseConfigs.GetDbType), false, true));
                            }
                            catch
                            {
                                throw new Exception("请检查SenserModelsApp.config中Dbtype节点数据库类型是否正确，例如：SqlServer、Access、MySql");
                            }

                        }
                    }

                    //m_provider = new DbProviderFinder().GetDbProvider("accesss");


                }
                return m_provider;
            }
        }

        /// <summary>
        /// DbFactory实例
        /// </summary>
        public static DbProviderFactory Factory
        {
            get
            {
                if (m_factory == null)
                {
                    m_factory = Provider.Instance();
                }
                return m_factory;
            }
        }

        /// <summary>
        /// 刷新数据库提供者
        /// </summary>
        public static void ResetDbProvider()
        {
            BaseConfigs.ResetConfig();
            DatabaseProvider.ResetDbProvider();
            m_connectionstring = null;
            m_factory = null;
            m_provider = null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 将DbParameter参数数组(参数值)分配给DbCommand命令.
        /// 这个方法将给任何一个参数分配DBNull.Value;
        /// 该操作将阻止默认值的使用.
        /// </summary>
        /// <param name="command">命令名</param>
        /// <param name="commandParameters">DbParameters数组</param>
        private static void AttachParameters(DbCommand command, DbParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (DbParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // 检查未分配值的输出参数,将其分配以DBNull.Value.
                        if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        /// <summary>
        /// 将DataRow类型的列值分配到DbParameter参数数组.
        /// </summary>
        /// <param name="commandParameters">要分配值的DbParameter参数数组</param>
        /// <param name="dataRow">将要分配给存储过程参数的DataRow</param>
        private static void AssignParameterValues(DbParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                return;
            }

            int i = 0;
            // 设置参数值
            foreach (DbParameter commandParameter in commandParameters)
            {
                // 创建参数名称,如果不存在,只抛出一个异常.
                if (commandParameter.ParameterName == null ||
                    commandParameter.ParameterName.Length <= 1)
                    throw new Exception(
                        string.Format("请提供参数{0}一个有效的名称{1}.", i, commandParameter.ParameterName));
                // 从dataRow的表中获取为参数数组中数组名称的列的索引.
                // 如果存在和参数名称相同的列,则将列值赋给当前名称的参数.
                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                i++;
            }
        }

        /// <summary>
        /// 将一个对象数组分配给DbParameter参数数组.
        /// </summary>
        /// <param name="commandParameters">要分配值的DbParameter参数数组</param>
        /// <param name="parameterValues">将要分配给存储过程参数的对象数组</param>
        private static void AssignParameterValues(DbParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }

            // 确保对象数组个数与参数个数匹配,如果不匹配,抛出一个异常.
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("参数值个数与参数不匹配.");
            }

            // 给参数赋值
            for (int i = 0, j = commandParameters.Length; i < j; i++)
            {
                // If the current array value derives from IDbDataParameter, then assign its Value property
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// 预处理用户提供的命令,数据库连接/事务/命令类型/参数
        /// </summary>
        /// <param name="command">要处理的DbCommand</param>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">一个有效的事务或者是null值</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名或都SQL命令文本</param>
        /// <param name="commandParameters">和命令相关联的DbParameter参数数组,如果没有参数为'null'</param>
        /// <param name="mustCloseConnection"><c>true</c> 如果连接是打开的,则为true,其它情况下为false.</param>
        private static void PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // 给命令分配一个数据库连接.
            command.Connection = connection;

            // 设置命令文本(存储过程名或SQL语句)
            command.CommandText = commandText;

            // 分配事务
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // 设置命令类型.
            command.CommandType = commandType;

            // 分配命令参数
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        /// <summary>
        /// 探索运行时的存储过程,返回DbParameter参数数组.
        /// 初始化参数值为 DBNull.Value.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        private static DbParameter[] DiscoverSpParameterSet(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            DbCommand cmd = connection.CreateCommand();
            cmd.CommandText = spName;
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            // 检索cmd指定的存储过程的参数信息,并填充到cmd的Parameters参数集中.
            Provider.DeriveParameters(cmd);
            connection.Close();
            // 如果不包含返回值参数,将参数集中的每一个参数删除.
            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }

            // 创建参数数组
            DbParameter[] discoveredParameters = new DbParameter[cmd.Parameters.Count];
            // 将cmd的Parameters参数集复制到discoveredParameters数组.
            cmd.Parameters.CopyTo(discoveredParameters, 0);

            // 初始化参数值为 DBNull.Value.
            foreach (DbParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }

        /// <summary>
        /// DbParameter参数数组的深层拷贝.
        /// </summary>
        /// <param name="originalParameters">原始参数数组</param>
        /// <returns>返回一个同样的参数数组</returns>
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] clonedParameters = new DbParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        #endregion 私有方法结束

        #region ExecuteNonQuery方法

        /// <summary>
        /// 执行指定连接字符串,类型的DbCommand.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery("SELECT * FROM [table123]");
        /// </remarks>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串,类型的DbCommand.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery("SELECT * FROM [table123]");
        /// </remarks>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out int id, string commandText)
        {
            return ExecuteNonQuery(out id, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串,类型的DbCommand.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串,并返回刚插入的自增ID
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out int id, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定连接字符串,类型的DbCommand.如果没有提供参数,不返回结果.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 执行指定连接字符串并返回刚插入的自增ID,类型的DbCommand.如果没有提供参数,不返回结果.
        /// </summary>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">DbParameter参数数组</param>
        /// <returns>返回命令影响的行数</returns>
        public static int ExecuteNonQuery(out int id, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {

            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return ExecuteNonQuery(out id, connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令 
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令并返回自增ID 
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(out int id, DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">T存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // 创建DbCommand命令,并进行预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Finally, execute the command
            int retval = cmd.ExecuteNonQuery();

            // 清除参数,以便再次使用.
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">T存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(out int id, DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (Provider.GetLastIdSql().Trim() == "") throw new ArgumentNullException("GetLastIdSql is \"\"");

            // 创建DbCommand命令,并进行预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行命令
            int retval = cmd.ExecuteNonQuery();
            // 清除参数,以便再次使用.
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = Provider.GetLastIdSql();

#if DEBUG
            DateTime dt1 = DateTime.Now;
#endif
            id = int.Parse(cmd.ExecuteScalar().ToString());
#if DEBUG
            DateTime dt2 = DateTime.Now;

            m_querydetail += GetQueryDetail(cmd.CommandText, dt1, dt2, commandParameters);
#endif
            m_querycount++;


            if (mustCloseConnection)
            {
                connection.Close();
            }
            return retval;
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,将对象数组的值赋给存储过程参数.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 给存储过程分配参数值
                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行带事务的DbCommand.
        /// </summary>
        /// <remarks>
        /// 示例.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回影响的行数/returns>
        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (DbParameter[])null);
        }


        /// <summary>
        /// 执行带事务的DbCommand.
        /// </summary>
        /// <remarks>
        /// 示例.:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回影响的行数/returns>
        public static int ExecuteNonQuery(out int id, DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(out id, transaction, commandType, commandText, (DbParameter[])null);
        }


        /// <summary>
        /// 执行带事务的DbCommand(指定参数).
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // 预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行
            int retval = cmd.ExecuteNonQuery();

            // 清除参数集,以便再次使用.
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// 执行带事务的DbCommand(指定参数).
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(out int id, DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // 预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行
            int retval = cmd.ExecuteNonQuery();
            // 清除参数,以便再次使用.
            cmd.Parameters.Clear();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = Provider.GetLastIdSql();
            id = int.Parse(cmd.ExecuteScalar().ToString());
            return retval;
        }

        /// <summary>
        /// 执行带事务的DbCommand(指定参数值).
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值
        /// 示例:  
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回受影响的行数</returns>
        public static int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // 没有参数值
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteNonQuery方法结束

        #region ExecuteCommandWithSplitter方法
        /// <summary>
        /// 运行含有GO命令的多条SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令字符串</param>
        /// <param name="splitter">分割字符串</param>
        public static void ExecuteCommandWithSplitter(string commandText, string splitter)
        {
            int startPos = 0;

            do
            {
                int lastPos = commandText.IndexOf(splitter, startPos);
                int len = (lastPos > startPos ? lastPos : commandText.Length) - startPos;
                string query = commandText.Substring(startPos, len);

                if (query.Trim().Length > 0)
                {
                    try
                    {
                        ExecuteNonQuery(CommandType.Text, query);
                    }
                    catch { ;}
                }

                if (lastPos == -1)
                    break;
                else
                    startPos = lastPos + splitter.Length;
            } while (startPos < commandText.Length);

        }

        /// <summary>
        /// 运行含有GO命令的多条SQL命令
        /// </summary>
        /// <param name="commandText">SQL命令字符串</param>
        public static void ExecuteCommandWithSplitter(string commandText)
        {
            ExecuteCommandWithSplitter(commandText, "\r\nGO\r\n");
        }
        #endregion ExecuteCommandWithSplitter方法结束

        #region ExecuteDataset方法


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset("SELECT * FROM [table1]");
        /// </remarks>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(string commandText)
        {
            return ExecuteDataset(CommandType.Text, commandText, (DbParameter[])null);
        }


        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(CommandType commandType, string commandText)
        {
            return ExecuteDataset(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例: 
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamters参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");

            // 创建并打开数据库连接对象,操作完成释放对象.

            //using (DbConnection connection = (DbConnection)new System.Data.SqlClient.SqlConnection(ConnectionString))
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                // 调用指定数据库连接字符串重载方法.
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,直接提供参数值,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值.
        /// 示例: 
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(string spName, params object[] parameterValues)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中检索存储过程参数
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 给存储过程参数分配值
                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定存储过程参数,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // 预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // 创建DbDataAdapter和DataSet.
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();



#if DEBUG
                DateTime dt1 = DateTime.Now;
#endif
                // 填充DataSet.
                da.Fill(ds);

#if DEBUG
                DateTime dt2 = DateTime.Now;

                m_querydetail += GetQueryDetail(cmd.CommandText, dt1, dt2, commandParameters);
#endif
                m_querycount++;

                cmd.Parameters.Clear();


                if (mustCloseConnection)
                    connection.Close();

                return ds;
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数值,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输入参数和返回值.
        /// 示例.:  
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 比缓存中加载存储过程参数
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 给存储过程参数分配值
                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定事务的命令,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定事务的命令,指定参数,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // 预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // 创建 DataAdapter & DataSet
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = cmd;
                DataSet ds = new DataSet();
                da.Fill(ds);
                cmd.Parameters.Clear();
                return ds;
            }
        }

        /// <summary>
        /// 执行指定事务的命令,指定参数值,返回DataSet.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输入参数和返回值.
        /// 示例.:  
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public static DataSet ExecuteDataset(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 给存储过程参数分配值
                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteDataset数据集命令结束

        #region ExecuteReader 数据阅读器

        /// <summary>
        /// 枚举,标识数据库连接是由BaseDbHelper提供还是由调用者提供
        /// </summary>
        private enum DbConnectionOwnership
        {
            /// <summary>由BaseDbHelper提供连接</summary>
            Internal,
            /// <summary>由调用者提供连接</summary>
            External
        }

        /// <summary>
        /// 执行指定数据库连接对象的数据阅读器.
        /// </summary>
        /// <remarks>
        /// 如果是BaseDbHelper打开连接,当连接关闭DataReader也将关闭.
        /// 如果是调用都打开连接,DataReader由调用都管理.
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="transaction">一个有效的事务,或者为 'null'</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">DbParameters参数数组,如果没有参数则为'null'</param>
        /// <param name="connectionOwnership">标识数据库连接对象是由调用者提供还是由BaseDbHelper提供</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        private static DbDataReader ExecuteReader(DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters, DbConnectionOwnership connectionOwnership)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            bool mustCloseConnection = false;
            // 创建命令
            DbCommand cmd = Factory.CreateCommand();
            try
            {
                PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                // 创建数据阅读器
                DbDataReader dataReader;

#if DEBUG
                DateTime dt1 = DateTime.Now;
#endif
                if (connectionOwnership == DbConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
#if DEBUG
                DateTime dt2 = DateTime.Now;

                m_querydetail += GetQueryDetail(cmd.CommandText, dt1, dt2, commandParameters);
#endif
                m_querycount++;
                // 清除参数,以便再次使用..
                // HACK: There is a problem here, the output parameter values are fletched 
                // when the reader is closed, so if the parameters are detached from the command
                // then the SqlReader can磘 set its values. 
                // When this happen, the parameters can磘 be used again in other command.
                bool canClear = true;
                foreach (DbParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                        canClear = false;
                }

                if (canClear)
                {
                    //cmd.Dispose();
                    cmd.Parameters.Clear();
                }


                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                    connection.Close();
                throw;
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return ExecuteReader(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组(new DbParameter("@prodid", 24))</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            DbConnection connection = null;
            try
            {
                connection = Factory.CreateConnection();
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return ExecuteReader(connection, null, commandType, commandText, commandParameters, DbConnectionOwnership.Internal);
            }
            catch
            {
                // If we fail to return the SqlDatReader, we need to close the connection ourselves
                if (connection != null) connection.Close();
                throw;
            }

        }

        /// <summary>
        /// 执行指定数据库连接字符串的数据阅读器,指定参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(string spName, params object[] parameterValues)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的数据阅读器.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return ExecuteReader(connection, (DbTransaction)null, commandType, commandText, commandParameters, DbConnectionOwnership.External);
        }

        /// <summary>
        /// [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">T存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// [调用者方式]执行指定数据库事务的数据阅读器,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///   DbDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, DbConnectionOwnership.External);
        }

        /// <summary>
        /// [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  DbDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // 没有参数值
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteReader数据阅读器

        #region ExecuteScalar 返回结果集中的第一行第一列

        /// <summary>
        /// 执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText)
        {
            // 执行参数为空的方法
            return ExecuteScalar(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            // 创建并打开数据库连接对象,操作完成释放对象.
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                // 调用指定数据库连接字符串重载方法.
                return ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(string spName, params object[] parameterValues)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                return ExecuteScalar(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // 没有参数值
                return ExecuteScalar(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText)
        {
            // 执行参数为空的方法
            return ExecuteScalar(connection, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // 创建DbCommand命令,并进行预处理
            DbCommand cmd = Factory.CreateCommand();

            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (DbTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行DbCommand命令,并返回结果.
            object retval = cmd.ExecuteScalar();

            // 清除参数,以便再次使用.
            cmd.Parameters.Clear();

            if (mustCloseConnection)
                connection.Close();

            return retval;
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // 没有参数值
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定数据库事务的命令,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            // 执行参数为空的方法
            return ExecuteScalar(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// 执行指定数据库事务的命令,指定参数,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");

            // 创建DbCommand命令,并进行预处理
            DbCommand cmd = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

#if DEBUG
            DateTime dt1 = DateTime.Now;
#endif
            // 执行DbCommand命令,并返回结果.
            object retval = cmd.ExecuteScalar();
#if DEBUG
            DateTime dt2 = DateTime.Now;
            m_querydetail += GetQueryDetail(cmd.CommandText, dt1, dt2, commandParameters);
#endif
            m_querycount++;
            // 清除参数,以便再次使用.
            cmd.Parameters.Clear();
            return retval;
        }

        /// <summary>
        /// 执行指定数据库事务的命令,指定参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // PPull the parameters for this stored procedure from the parameter cache ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                // 没有参数值
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion ExecuteScalar

        #region FillDataset 填充数据集
        /// <summary>
        /// 执行指定数据库连接字符串的命令,映射数据表并填充数据集.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)</param>
        public static void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // 创建并打开数据库连接对象,操作完成释放对象.
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                // 调用指定数据库连接字符串重载方法.
                FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,映射数据表并填充数据集.指定命令参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        public static void FillDataset(CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            // 创建并打开数据库连接对象,操作完成释放对象.
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                // 调用指定数据库连接字符串重载方法.
                FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }

        /// <summary>
        /// 执行指定数据库连接字符串的命令,映射数据表并填充数据集,指定存储过程参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>    
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public static void FillDataset(string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            // 创建并打开数据库连接对象,操作完成释放对象.
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                // 调用指定数据库连接字符串重载方法.
                FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,映射数据表并填充数据集.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>    
        public static void FillDataset(DbConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,映射数据表并填充数据集,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        public static void FillDataset(DbConnection connection, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params DbParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// 执行指定数据库连接对象的命令,映射数据表并填充数据集,指定存储过程参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public static void FillDataset(DbConnection connection, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                // 没有参数值
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// 执行指定数据库事务的命令,映射数据表并填充数据集.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        public static void FillDataset(DbTransaction transaction, CommandType commandType,
            string commandText,
            DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// 执行指定数据库事务的命令,映射数据表并填充数据集,指定参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        public static void FillDataset(DbTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params DbParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// 执行指定数据库事务的命令,映射数据表并填充数据集,指定存储过程参数值.
        /// </summary>
        /// <remarks>
        /// 此方法不提供访问存储过程输出参数和返回值参数.
        /// 
        /// 示例:  
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public static void FillDataset(DbTransaction transaction, string spName,
            DataSet dataSet, string[] tableNames,
            params object[] parameterValues)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (dataSet == null) throw new ArgumentNullException("dataSet");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果有参数值
            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 给存储过程参数赋值
                AssignParameterValues(commandParameters, parameterValues);

                // 调用重载方法
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                // 没有参数值
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// [私有方法][内部调用]执行指定数据库连接对象/事务的命令,映射数据表并填充数据集,DataSet/TableNames/DbParameters.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new DbParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        /// 用户定义的表名 (可有是实际的表名.)
        /// </param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        private static void FillDataset(DbConnection connection, DbTransaction transaction, CommandType commandType,
            string commandText, DataSet dataSet, string[] tableNames,
            params DbParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            // 创建DbCommand命令,并进行预处理
            DbCommand command = Factory.CreateCommand();
            bool mustCloseConnection = false;
            PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行命令
            using (DbDataAdapter dataAdapter = Factory.CreateDataAdapter())
            {
                dataAdapter.SelectCommand = command;
                // 追加表映射
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";
                    for (int index = 0; index < tableNames.Length; index++)
                    {
                        if (tableNames[index] == null || tableNames[index].Length == 0) throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        dataAdapter.TableMappings.Add(tableName, tableNames[index]);
                        tableName = "Table" + (index + 1).ToString();
                    }
                }

                // 填充数据集使用默认表名称
                dataAdapter.Fill(dataSet);

                // 清除参数,以便再次使用.
                command.Parameters.Clear();
            }

            if (mustCloseConnection)
                connection.Close();
        }
        #endregion

        #region UpdateDataset 更新数据集
        /// <summary>
        /// 执行数据集更新到数据库,指定inserted, updated, or deleted命令.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">[追加记录]一个有效的SQL语句或存储过程</param>
        /// <param name="deleteCommand">[删除记录]一个有效的SQL语句或存储过程</param>
        /// <param name="updateCommand">[更新记录]一个有效的SQL语句或存储过程</param>
        /// <param name="dataSet">要更新到数据库的DataSet</param>
        /// <param name="tableName">要更新到数据库的DataTable</param>
        public static void UpdateDataset(DbCommand insertCommand, DbCommand deleteCommand, DbCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null) throw new ArgumentNullException("insertCommand");
            if (deleteCommand == null) throw new ArgumentNullException("deleteCommand");
            if (updateCommand == null) throw new ArgumentNullException("updateCommand");
            if (tableName == null || tableName.Length == 0) throw new ArgumentNullException("tableName");
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                updateCommand.Connection = connection;
                insertCommand.Connection = connection;
                deleteCommand.Connection = connection;
                using (DbDataAdapter dataAdapter = Factory.CreateDataAdapter())
                {
                    // 设置数据适配器命令
                    dataAdapter.UpdateCommand = updateCommand;
                    dataAdapter.InsertCommand = insertCommand;
                    dataAdapter.DeleteCommand = deleteCommand;

                    // 更新数据集改变到数据库
                    dataAdapter.Update(dataSet, tableName);

                    // 提交所有改变到数据集.
                    dataSet.AcceptChanges();
                }
            }
            // 创建DbDataAdapter,当操作完成后释放.
            
        }
        #endregion

        #region CreateCommand 创建一条DbCommand命令
        /// <summary>
        /// 创建DbCommand命令,指定数据库连接对象,存储过程名和参数.
        /// </summary>
        /// <remarks>
        /// 示例:  
        ///  DbCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="sourceColumns">源表的列名称数组</param>
        /// <returns>返回DbCommand命令</returns>
        public static DbCommand CreateCommand(DbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 创建命令
            DbCommand cmd = Factory.CreateCommand();
            cmd.CommandText = spName;
            cmd.Connection = connection;
            cmd.CommandType = CommandType.StoredProcedure;

            // 如果有参数值
            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 将源表的列到映射到DataSet命令中.
                for (int index = 0; index < sourceColumns.Length; index++)
                    commandParameters[index].SourceColumn = sourceColumns[index];

                // Attach the discovered parameters to the DbCommand object
                AttachParameters(cmd, commandParameters);
            }

            return cmd;
        }
        #endregion

        #region ExecuteNonQueryTypedParams 类型化参数(DataRow)
        /// <summary>
        /// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回受影响的行数.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(String spName, DataRow dataRow)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteNonQuery(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回受影响的行数.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(DbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回受影响的行数.
        /// </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQueryTypedParams(DbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // Sf the row has values, the store procedure parameters must be initialized
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region ExecuteDatasetTypedParams 类型化参数(DataRow)
        /// <summary>
        /// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataSet.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public static DataSet ExecuteDatasetTypedParams(String spName, DataRow dataRow)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            //如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteDataset(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteDataset(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataSet.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        /// 
        public static DataSet ExecuteDatasetTypedParams(DbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回DataSet.
        /// </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public static DataSet ExecuteDatasetTypedParams(DbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        #endregion

        #region ExecuteReaderTypedParams 类型化参数(DataRow)
        /// <summary>
        /// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataReader.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReaderTypedParams(String spName, DataRow dataRow)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteReader(ConnectionString, CommandType.StoredProcedure, spName);
            }
        }


        /// <summary>
        /// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataReader.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReaderTypedParams(DbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回DataReader.
        /// </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的DbDataReader</returns>
        public static DbDataReader ExecuteReaderTypedParams(DbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region ExecuteScalarTypedParams 类型化参数(DataRow)
        /// <summary>
        /// 执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalarTypedParams(String spName, DataRow dataRow)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteScalar(CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteScalar(CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalarTypedParams(DbConnection connection, String spName, DataRow dataRow)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// 执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        /// </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public static object ExecuteScalarTypedParams(DbTransaction transaction, String spName, DataRow dataRow)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");
            if (transaction != null && transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            // 如果row有值,存储过程必须初始化.
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                // 从缓存中加载存储过程参数,如果缓存中不存在则从数据库中检索参数信息并加载到缓存中. ()
                DbParameter[] commandParameters = GetSpParameterSet(transaction.Connection, spName);

                // 分配参数值
                AssignParameterValues(commandParameters, dataRow);

                return DbHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return DbHelper.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }
        #endregion

        #region 缓存方法

        /// <summary>
        /// 追加参数数组到缓存.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <param name="commandParameters">要缓存的参数数组</param>
        public static void CacheParameterSet(string commandText, params DbParameter[] commandParameters)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = ConnectionString + ":" + commandText;

            m_paramcache[hashKey] = commandParameters;
        }

        /// <summary>
        /// 从缓存中获取参数数组.
        /// </summary>
        /// <param name="ConnectionString">一个有效的数据库连接字符</param>
        /// <param name="commandText">存储过程名或SQL语句</param>
        /// <returns>参数数组</returns>
        public static DbParameter[] GetCachedParameterSet(string commandText)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            string hashKey = ConnectionString + ":" + commandText;

            DbParameter[] cachedParameters = m_paramcache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        #endregion 缓存方法结束

        #region 检索指定的存储过程的参数集

        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符</param>
        /// <param name="spName">存储过程名</param>
        /// <returns>返回DbParameter参数数组</returns>
        public static DbParameter[] GetSpParameterSet(string spName)
        {
            return GetSpParameterSet(spName, false);
        }

        /// <summary>
        /// 返回指定的存储过程的参数集
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="ConnectionString">一个有效的数据库连接字符.</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        public static DbParameter[] GetSpParameterSet(string spName, bool includeReturnValueParameter)
        {
            if (ConnectionString == null || ConnectionString.Length == 0) throw new ArgumentNullException("ConnectionString");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象).
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接字符</param>
        /// <param name="spName">存储过程名</param>
        /// <returns>返回DbParameter参数数组</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// [内部]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <remarks>
        /// 这个方法将查询数据库,并将信息存储到缓存.
        /// </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">
        /// 是否包含返回值参数
        /// </param>
        /// <returns>返回DbParameter参数数组</returns>
        internal static DbParameter[] GetSpParameterSet(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            using (DbConnection clonedConnection = (DbConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// [私有]返回指定的存储过程的参数集(使用连接对象)
        /// </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="includeReturnValueParameter">是否包含返回值参数</param>
        /// <returns>返回DbParameter参数数组</returns>
        private static DbParameter[] GetSpParameterSetInternal(DbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            if (spName == null || spName.Length == 0) throw new ArgumentNullException("spName");

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");

            DbParameter[] cachedParameters;

            cachedParameters = m_paramcache[hashKey] as DbParameter[];
            if (cachedParameters == null)
            {
                DbParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                m_paramcache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }

        #endregion 参数集检索结束

        #region 生成参数

        public static DbParameter MakeInParam(string ParamName, DbType DbType, int Size, object Value)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Input, Value);
        }

        public static DbParameter MakeOutParam(string ParamName, DbType DbType, int Size)
        {
            return MakeParam(ParamName, DbType, Size, ParameterDirection.Output, null);
        }

        public static DbParameter MakeParam(string ParamName, DbType DbType, Int32 Size, ParameterDirection Direction, object Value)
        {
            DbParameter param;

            param = Provider.MakeParam(ParamName, DbType, Size);

            param.Direction = Direction;
            if (!(Direction == ParameterDirection.Output && Value == null))
                param.Value = Value;

            return param;
        }

        #endregion 生成参数结束

        #region 执行ExecuteScalar,将结果以字符串类型输出。

        public static string ExecuteScalarToStr(CommandType commandType, string commandText)
        {
            object ec = ExecuteScalar(commandType, commandText);
            if (ec == null)
            {
                return "";
            }
            return ec.ToString();
        }


        public static string ExecuteScalarToStr(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            object ec = ExecuteScalar(commandType, commandText, commandParameters);
            if (ec == null)
            {
                return "";
            }
            return ec.ToString();
        }


        #endregion

    }
    #endregion

#endif
}
