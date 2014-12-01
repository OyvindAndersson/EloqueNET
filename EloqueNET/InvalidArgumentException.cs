using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET
{
    class InvalidArgumentException : Exception
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init

        public InvalidArgumentException(string message)
            : base(message) { }

        public InvalidArgumentException(string message, Exception innerException)
            : base(message, innerException) { }

        #endregion

        #region Methods
        #endregion
    }
}
