using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AsysORM
{
    public class AsysORMException : Exception
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init

        public AsysORMException() : base() { }

        public AsysORMException(string message) : base(message) { }

        protected AsysORMException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public AsysORMException(string message, Exception innerException) : base(message, innerException) { }

        #endregion

        #region Methods
        #endregion
    }
}
