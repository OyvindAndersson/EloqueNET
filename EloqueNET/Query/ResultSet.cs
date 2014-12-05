using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    /// <summary>
    /// Defines a row from a table result
    /// </summary>
    public class Row : List<Column>
    {
        #region Var
        #endregion

        #region Properties

        public Column this[string key]
        {
            get
            {
                foreach(Column col in this)
                {
                    if (col.Name == key)
                        return col;
                }

                throw new InvalidArgumentException("No element with key ['" + key + "'] in row!");
            }
        }

        #endregion

        #region Init

        public Row()
        { 
        }

        #endregion

        #region Methods
        #endregion
    }

    /// <summary>
    /// Defines a class that hold results after a query against the database
    /// </summary>
    public class ResultSet : List<Row>
    {
        #region Var

        #endregion

        #region Properties

        #endregion

        #region Init

        public ResultSet()
        {
        }

        #endregion

        #region Methods



        #endregion
    }
}
