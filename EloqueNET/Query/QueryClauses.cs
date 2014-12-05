using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    #region Enums

    /// <summary>
    /// 
    /// </summary>
    public enum ClauseType
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
        Day,
        Month,
        Year
    }

    /// <summary>
    /// Defines the type a join can have
    /// </summary>
    public enum JoinType
    {
        Inner = 0,
        Left,
        Right,

        Outer,
        LeftOuter,
        RightOuter,

        FullOuter,
    }

    public enum OrderDirection
    {
        Asc,
        Desc
    }

    #endregion

    #region Value types

    public struct WhereClause
    {
        #region Var

        ClauseType _type;

        Column _column;

        Is _operator;

        object _value;

        Conditional _condition;

        /// <summary>
        /// Indicates wheter it is a not operator (i.e: NOT NULL, NOT BETWEEN)
        /// </summary>
        bool _not;

        QueryBuilder _nestedQuery;

        #endregion

        #region Properties

        public ClauseType Type
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

        public QueryBuilder Query
        {
            get { return _nestedQuery; }
            internal set { _nestedQuery = value; }
        }

        #endregion

        #region Init

        public WhereClause(ClauseType type, Column column, Conditional condition)
        {
            _type = type;
            _column = column;
            _condition = condition;

            _operator = Is.EqualTo;
            _value = null;
            _not = false;
            _nestedQuery = null;
        }

        public WhereClause(ClauseType type, Column column, Is op, object value, Conditional condition)
        {
            _type = type;
            _column = column;
            _operator = op;
            _value = value;
            _condition = condition;
            _not = false;
            _nestedQuery = null;
        }

        public WhereClause(ClauseType type, Column column, Is op, object value, Conditional condition, bool not)
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

    public struct HavingClause
    {
        #region Var

        ClauseType _type;

        Column _column;

        Is _operator;

        object _value;

        Conditional _condition;

        QueryBuilder _query;

        #endregion

        #region Properties

        public ClauseType Type
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

        public QueryBuilder Query
        {
            get { return _query; }
            internal set { _query = value; }
        }

        #endregion

        #region Init

        public HavingClause(ClauseType type, Column column, Is op, object value, Conditional condition)
        {
            _type = type;
            _column = column;
            _operator = op;
            _value = value;
            _condition = condition;
            _query = null;
        }

        #endregion

        #region Methods
        #endregion

    }

    public struct OrderClause
    {
        #region Var

        Column _column;

        OrderDirection _direction;

        #endregion

        #region Properties

        public Column Column
        {
            get { return _column; }
            set { _column = value; }
        }

        public OrderDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        #endregion

        #region Init

        public OrderClause(Column column, OrderDirection direction)
        {
            _column = column;
            _direction = direction;
        }

        #endregion

        #region Methods
        #endregion
    }

    public struct UnionClause
    {
        #region Var

        QueryBuilder _query;
        bool _all;

        #endregion

        #region Properties

        public QueryBuilder Query
        {
            get { return _query; }
            set { _query = value; }
        }

        public bool All
        {
            get { return _all; }
            set { _all = value; }
        }

        #endregion

        #region Init

        public UnionClause(QueryBuilder query, bool all = false)
        {
            _query = query;
            _all = all;
        }

        #endregion

        #region Methods
        #endregion
    }

    /// <summary>
    /// Poor naming.. It represents a clause inside of a JoinClause class instance
    /// </summary>
    public struct ClauseInJoin
    {
        #region Var
        string _first;
        string _second;
        Is _operator;
        Conditional _condition;
        bool _where;

        #endregion

        #region Properties

        public string First
        {
            get { return _first; }
            set { _first = value; }
        }

        public string Second
        {
            get { return _second; }
            set { _second = value; }
        }

        public Is Operator
        {
            get { return _operator; }
            set { _operator = value; }
        }

        public Conditional Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public bool Where
        {
            get { return _where; }
            set { _where = value; }
        }

        #endregion

        #region Init

        public ClauseInJoin(string first, string second, Is op, Conditional condition, bool where)
        {
            _first = first;
            _second = second;
            _operator = op;
            _condition = condition;
            _where = where;
        }

        #endregion

        #region Methods
        #endregion
    }

    #endregion

    #region Classes

    /// <summary>
    /// 
    /// </summary>
    public class JoinClause
    {
        #region Var

        /// <summary>
        /// The 'on' clauses for the join
        /// </summary>
        IList<ClauseInJoin> _clauses;

        /// <summary>
        /// The 'on' bindings for the join
        /// </summary>
        List<string> _bindings;

        /// <summary>
        /// The type of join being performed
        /// </summary>
        JoinType _type;

        /// <summary>
        /// The table the join clause is joining to
        /// </summary>
        string _table;

        #endregion

        #region Properties

        public IList<ClauseInJoin> Clauses
        {
            get { return _clauses; }
        }

        public List<string> Bindings
        {
            get { return _bindings; }
        }

        public JoinType Type
        {
            get { return _type; }
        }

        public string Table
        {
            get { return _table; }
        }

        #endregion

        #region Init

        private JoinClause() { }

        public JoinClause(JoinType type, string table)
        {
            _clauses = new List<ClauseInJoin>();
            _bindings = new List<string>();
            _type = type;
            _table = table;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add an "on" clause to the join
        /// </summary>
        /// <param name="first"></param>
        /// <param name="op"></param>
        /// <param name="second"></param>
        /// <param name="condition"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public JoinClause On(string first, Is op, string second, Conditional condition = Conditional.And, bool where = false)
        {
            ClauseInJoin clause = new ClauseInJoin()
            {
                First = first,
                Operator = op,
                Second = second,
                Condition = condition,
                Where = where
            };

            _clauses.Add(clause);

            if (where)
            {
                _bindings.Add(second);
            }

            return this;
        }

        public JoinClause OrOn(string first, Is op, string second)
        {
            return On(first, op, second, Conditional.Or);
        }

        public JoinClause Where(string first, Is op, string second, Conditional condition = Conditional.And)
        {
            return On(first, op, second, condition, true);
        }

        public JoinClause OrWhere(string first, Is op, string second)
        {
            return On(first, op, second, Conditional.Or, true);
        }

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

    #endregion
}
