using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    public enum AggregateFunction
    {
        None,
        Avg,
        Count,
        Sum,
        Min,
        Max
    }

    public interface IAggregate
    {
        ColumnList Columns { get; set; }
        AggregateFunction Function { get; set; }
    }

    public class Aggregate : IAggregate
    {
        #region Var

        ColumnList _columns;
        AggregateFunction _function;

        #endregion

        #region Properties

        public ColumnList Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        public AggregateFunction Function
        {
            get { return _function; }
            set { _function = value; }
        }

        #endregion

        #region Init

        public Aggregate()
        {
            _columns = new ColumnList();
            _function = AggregateFunction.None;
        }

        #endregion

        #region Methods
        #endregion
    }
}
