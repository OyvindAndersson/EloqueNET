using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    public interface IAggregate
    {
        List<Column> Columns { get; }
        string Function { get; }
    }

    public class Aggregate : IAggregate
    {
        #region Var

        List<Column> _columns;
        string _function;

        #endregion

        #region Properties

        public List<Column> Columns
        {
            get { return _columns; }
        }

        public string Function
        {
            get { return _function; }
        }

        #endregion

        #region Init

        public Aggregate()
        {
            _columns = new List<Column>();
            _function = "";
        }

        #endregion

        #region Methods
        #endregion
    }
}
