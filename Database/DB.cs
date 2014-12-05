using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsysORM.Database;
using AsysORM.Diagnostics;
using AsysORM.EloqueNET;
using AsysORM.EloqueNET.Query;
using AsysORM.EloqueNET.Grammar;

namespace AsysORM
{
    public abstract class DB : IDatabaseConnection, IDisposable
    {
        #region Vars

        private IDbConnection _connection;

        private IDbCommand _cmd;

        private string _connectionString;

        private BaseGrammar _queryGrammar;

        private readonly ExecutionTimer _execTimer = new ExecutionTimer(false);
        bool _useTimer = false;

        public const string BindingNamePrefix = "@binding";

        #endregion

        #region Properties

        /// <summary>
        /// Indicates wheter or not the database connection is open
        /// </summary>
        public bool IsConnected
        {
            get 
            {
                if (_connection == null)
                    return false;

                return _connection.State != ConnectionState.Closed;
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public IDbConnection Connection
        {
            get { return _connection; }
            internal set { _connection = value; }
        }

        public IDbCommand Command
        {
            get { return _cmd; }
            internal set { _cmd = value; }
        }

        public BaseGrammar QueryGrammar
        {
            get { return _queryGrammar; }
            internal set { _queryGrammar = value; }
        }

        /// <summary>
        /// Get/Sets wheter or not to actively time executions against the database
        /// </summary>
        public bool UseTimer
        {
            get { return _useTimer; }
            set { _useTimer = value; }
        }

        #endregion

        #region Init

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        public DB(string server, string userid, string password, string database)
        {
            _connectionString = @"server=" + server + ";userid=" + userid + ";password=" + password + ";database=" + database;
            Connect();

            UseDefaultQueryGrammar();
        }

        /// <summary>
        /// Creates a new instance of the DB class, and tries to connect to its database.
        /// </summary>
        /// <param name="ConnectionString">Connection string</param>
        public DB(string connectionString)
        {
            _connectionString = connectionString;
            Connect();

            UseDefaultQueryGrammar();
        }

        /// <summary>
        /// Sets the connection to use the default grammar class as the query grammar class
        /// </summary>
        public void UseDefaultQueryGrammar()
        {
            _queryGrammar = GetDefaultQueryGrammar();
        }

        /// <summary>
        /// Gets the default query grammar type for this connection class.
        /// <remarks>Override this in any SQL connection type that uses specific grammar rules</remarks>
        /// </summary>
        /// <returns></returns>
        protected virtual BaseGrammar GetDefaultQueryGrammar()
        {
            return new BaseGrammar();
        }

        #endregion

        #region Connection Methods

        /// <summary>
        /// Connects to the database
        /// </summary>
        abstract protected void Connect();

        /// <summary>
        /// 
        /// </summary>
        protected void Close()
        {
            if(_connection != null)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Tries to reconnet to the database if the connection is lost or broken
        /// </summary>
        /// <exception cref="AsysORM.Database.DBException">DBException</exception>
        /// <returns>true if connected or reconnected, false if not</returns>
        protected bool TryReconnect()
        {
            if (!IsConnected)
            {
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    throw new DBException(ex.Message, ex);
                }
            }

            return true;
        }
        
        #endregion

        #region Querying

        /// <summary>
        /// Execute a select statement and return the results
        /// @TODO: Clean up console writelines
        /// </summary>
        /// <param name="query"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public ResultSet Select(string query, BindingsDict bindings)
        {
            IDbCommand cmd = PrepareCommand(query, bindings);
            ResultSet results = new ResultSet();

            try
            {
                if(_useTimer) 
                    _execTimer.Start();

                IDataReader dataReader = cmd.ExecuteReader();

                Console.WriteLine("\nReading dataresult\n----------------------------");

                while (dataReader.Read())
                {
                    Row row = new Row();

                    for(int i = 0; i < dataReader.FieldCount; i++)
                    {
                        // Add this column to the current result row
                        Column newCol = new Column(dataReader.GetName(i), dataReader[i], dataReader[i].GetSqlValueType());
                        row.Add(newCol);

                        Console.WriteLine("[{0}]: '{1}'", dataReader.GetName(i), dataReader[i].ToString());
                    }

                    // Add the row to our final result set
                    results.Add(row);
                }

                #region Debug - Write out command text

                // Print out all the parameters we added to the Command object
                Console.WriteLine("\nUsing:\n[CommandText]: '{0}'", cmd.CommandText);
                foreach (IDbDataParameter param in cmd.Parameters)
                {
                    Console.WriteLine("[" + param.DbType.ToString() + "] Name: '" + param.ParameterName + "', Value: " + param.Value.ToString());
                }

                Console.WriteLine("-");

                #endregion

                dataReader.Close();

                if (_useTimer) 
                    _execTimer.Stop();
            }
            catch(Exception ex)
            {
                if (_useTimer)
                    _execTimer.Stop();

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new DBException("Failed to execute reader on Select query!", ex);
            }

            return results;
            
        }

        public int Insert(string query, ColumnList bindings)
        {
            IDbCommand cmd = PrepareCommand(query, bindings);
            int result = 0;

            Console.WriteLine("\nPrepared insert command text: '{0}'\n-", cmd.CommandText);

            try
            {
                if (_useTimer)
                    _execTimer.Start();

                result = cmd.ExecuteNonQuery();

                if (_useTimer)
                    _execTimer.Stop();

                #region Debug - Write out command text

                // Print out all the parameters we added to the Command object
                Console.WriteLine("\nUsing:\n[CommandText]: '{0}'", cmd.CommandText);
                foreach (IDbDataParameter param in cmd.Parameters)
                {
                    Console.WriteLine("[" + param.DbType.ToString() + "] Name: '" + param.ParameterName + "', Value: " + param.Value.ToString());
                }

                Console.WriteLine("-");

                #endregion

            }
            catch (Exception ex)
            {
                if (_useTimer)
                    _execTimer.Stop();

                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new DBException("Failed to execute reader on Insert Into query!", ex);
            }

            return result;
        }

        /// <summary>
        /// Begin a QueryBuilder query against a database table
        /// </summary>
        /// <param name="fromTable">Database table to build query for</param>
        /// <returns></returns>
        public QueryBuilder Table(string fromTable)
        {
            var query = new QueryBuilder(this, QueryGrammar);

            return query.From(fromTable);
        }

        /// <summary>
        /// Prepare a query for execution by replacing parameter names with their binding value
        /// </summary>
        /// <param name="query"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public IDbCommand PrepareCommand(string query, BindingsDict bindings)
        {
            // Create the command object for our query
            IDbCommand cmd = Connection.CreateCommand();
            cmd.Connection = Connection;
            cmd.CommandText = query;

            // Bind all parameters to the query (if any)
            int i = 0;
            foreach(KeyValuePair<BindingType, IList<object>> kv in bindings)
            {
                foreach(object value in kv.Value)
                {
                    IDbDataParameter param = cmd.CreateParameter();
                    param.DbType = value.GetSqlValueType();
                    param.Value = value ?? DBNull.Value;
                    param.ParameterName = DB.BindingNamePrefix + i.ToString(); // Parameter prefix name is constant

                    cmd.Parameters.Add(param);

                    ++i;
                }
            }

            return cmd;
        }

        /// <summary>
        /// Prepare a query for execution by replacing parameter names with their binding value
        /// </summary>
        /// <param name="query"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public IDbCommand PrepareCommand(string query, ColumnList bindings)
        {
            // Create the command object for our query
            IDbCommand cmd = Connection.CreateCommand();
            cmd.Connection = Connection;
            cmd.CommandText = query;

            int i = 0;
            foreach(Column column in bindings)
            {
                IDbDataParameter param = cmd.CreateParameter();
                param.DbType = column.Value.GetSqlValueType();
                param.Value = column.Value;
                param.ParameterName = DB.BindingNamePrefix + i.ToString();

                cmd.Parameters.Add(param);

                i++;
            }

            return cmd;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Returns the executiontime of the last database query, in milliseconds
        /// </summary>
        /// <returns></returns>
        public string GetLastExecTimeInMs()
        {
            return _execTimer.ExecTimeInMs;
        }

        public string GetLastExecTimeInNs()
        {
            return _execTimer.ExecTimeInNs;
        }

        #endregion

        #region GC

        /// <summary>
        /// Closes any existing connection and cleans up other sql stuff.
        /// </summary>
        public void Dispose()
        {
            Close();

            _connection = null;

            if(_cmd != null)
            {
                _cmd.Cancel();
                _cmd.Dispose();
                _cmd = null;
            }
        }

        #endregion
    }
}
