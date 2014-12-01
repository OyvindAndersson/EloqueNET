using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET
{
    /// <summary>
    /// Used as a temporary table that is "under construction".
    /// After this has been configured and filled with commands that should
    /// be executed to create it, a proper ITable instance will reference
    /// the actual table in the database.
    /// </summary>
    class BlueprintTable : ITable
    {
        #region Var

        private string _name;

        QueryBuilder _builder;

        #endregion

        #region Properties
        #endregion

        #region Init

        public BlueprintTable(string name)
        {
            _name = name;
        }

        #endregion

        #region Builder Methods

        public void Build(DB connection)
        {
            _builder = new QueryBuilder(_name, connection);
        }

        #endregion
    }
}
