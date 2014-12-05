using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using AsysORM.EloqueNET.Grammar;

namespace AsysORM.MySQL
{
    public class MySqlDB : DB
    {

        #region Init

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="AsysORM.DB"/>
        public MySqlDB(string server, string userid, string password, string database)
            : base(server, userid, password, database)
        {
        }

        /// <summary>
        /// Creates a new instance of the MySqlDB class, and tries to connect to its database.
        /// </summary>
        /// <see cref="AsysORM.DB"/>
        public MySqlDB(string connectionString)
            : base(connectionString)
        {
        }

        #endregion

        #region DB Connection implementation

        /// <summary>
        /// 
        /// </summary>
        protected override void Connect()
        {
            try
            {
                Connection = new MySqlConnection(ConnectionString);
                Connection.Open();

                // Create the associated command object
                Command = Connection.CreateCommand();
            }
            catch (MySqlException ex)
            {
                throw new AsysORMException(ex.Message, ex);
            }
        }

        protected override BaseGrammar GetDefaultQueryGrammar()
        {
            return new MysqlGrammar();
        }

        #endregion
    }
}
