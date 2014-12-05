using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.Database
{
    public class DBException : AsysORMException
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init

        public DBException() : base() { }
        public DBException(string message) : base(message) { }
        public DBException(string message, Exception innerException) : base(message, innerException) { }

        #endregion

        #region Methods
        #endregion
    }
}
