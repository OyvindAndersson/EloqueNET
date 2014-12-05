using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET
{
    public class Table : ITable
    {
        #region Var

        private string _name;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        #endregion

        #region Init

        public Table(string name)
        {
            _name = name;
        }

        #endregion

        #region QueryBuilder Methods

        public void Increments(string columnName)
        {

        }

        #endregion

    }
}
