using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    public enum WhereType
    {
        Basic,
        Raw,
        Between,
        Nested,
        Sub,
        NotExists,
        Exists,
        NotIn,
        In,
        NotInSub,
        InSub,
        NotNull,
        Null,
    }

    public struct WhereClause
    {
        #region Var

        WhereType _type;

        Column _column;

        Is _operator;

        object _value;

        Conditional _condition;

        /// <summary>
        /// Indicates wheter it is a not operator (i.e: NOT NULL, NOT BETWEEN)
        /// </summary>
        bool _not;

        Builder _nestedQuery;

        #endregion

        #region Properties

        public WhereType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public Column Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public Is Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Conditional Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        /// <summary>
        /// Indicates wheter the operator is 'not'
        /// <example>NOT NULL, NOT BETWEEN etc.</example>
        /// </summary>
        public bool Not
        {
            get { return _not; }
            set { _not = value; }
        }

        public Builder Query
        {
            get { return _nestedQuery; }
            internal set { _nestedQuery = value; }
        }

        #endregion

        #region Init

        public WhereClause(WhereType type, Column column, Conditional condition)
        {
            _type = type;
            _column = column;
            _condition = condition;

            _operator = Is.EqualTo;
            _value = null;
            _not = false;
            _nestedQuery = null;
        }

        public WhereClause(WhereType type, Column column, Is op, object value, Conditional condition)
        {
            _type = type;
            _column = column;
            _operator = op;
            _value = value;
            _condition = condition;
            _not = false;
            _nestedQuery = null;
        }

        public WhereClause(WhereType type, Column column, Is op, object value, Conditional condition, bool not)
        {
            _type = type;
            _column = column;
            _operator = op;
            _value = value;
            _condition = condition;
            _not = not;
            _nestedQuery = null;
        }

        #endregion

        #region Methods
        #endregion
    }

    public class WhereClauseList : List<WhereClause>
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init

        public WhereClauseList() 
            : base()
        {

        }

        #endregion

        #region Methods
        #endregion
    }
}
