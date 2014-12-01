using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using AsysORM.Database;
using AsysORM.EloqueNET.Grammar;

namespace AsysORM.EloqueNET.Query
{
    #region Helpers and datatypes

    public delegate Builder QueryCallback(Builder query);

    public enum Conditional
    {
        And = 0,
        Or
    }

    public enum BindingType
    {
        Select,
        Join,
        Where,
        Having,
        Order
    }

    #endregion

    /// <summary>
    /// SQL Builder class
    /// </summary>
    public class Builder
    {
        #region Var

        private BaseGrammar _grammar;

        private DB _connection;

        private BindingsDict _bindings;

        /// <summary>
        /// An aggregate function and column to be run
        /// </summary>
        private IAggregate _aggregate;

        /// <summary>
        /// The columns that should be returned
        /// </summary>
        private ColumnList _columns;

        /// <summary>
        /// Indicates if the query returns distinct results
        /// </summary>
        private bool _distinct;

        /// <summary>
        /// The table which the query is targeting
        /// </summary>
        private string _from;

        private IList<JoinClause> _joins;
        private IList<WhereClause> _wheres;
        private List<string> _groups;
        private List<string> _havings;
        private List<string> _orders;
        private int _limit;
        private int _offset;
        private List<string> _unions;

        #endregion

        #region Properties

        public BindingsDict Bindings
        {
            get { return _bindings; }
        }

        public BaseGrammar Grammar
        {
            get { return _grammar; }
        }

        public IAggregate Aggregate
        {
            get { return _aggregate; }
        }

        public ColumnList Columns
        {
            get { return _columns; }
        }

        public bool GetDistinct
        {
            get { return _distinct; }
        }

        public string GetFrom
        {
            get { return _from; }
        }

        public IList<JoinClause> Joins
        {
            get { return _joins; }
        }

        public IList<WhereClause> Wheres
        {
            get { return _wheres; }
        }

        public List<string> Groups
        {
            get { return _groups; }
        }

        public List<string> Havings
        {
            get { return _havings; }
        }

        public List<string> Orders
        {
            get { return _orders; }
        }

        public List<string> Unions
        {
            get { return _unions; }
        }

        public int GetLimit
        {
            get { return _limit; }
        }

        public int GetOffset
        {
            get { return _offset; }
        }

        #endregion

        #region Init

        public Builder(DB connection, BaseGrammar grammar)
        {
            Reset();

            _connection = connection;
            _grammar = grammar;
        }

        private void Reset()
        {
            _grammar = null;
            _connection = null;

            _bindings = new BindingsDict();

            _wheres = new List<WhereClause>();

            _aggregate = null;

            _columns = new ColumnList();

            _distinct = false;

            _from = "";
        }

        #endregion

        #region Methods

        #region Selects and constraints

        public Builder Select(ColumnList columns)
        {
            _columns = columns;
            return this;
        }

        public Builder Select(Column column)
        {
            _columns.Clear();
            _columns.Add(column);
            return this;
        }

        public Builder Select(string columnName)
        {
            _columns.Clear();
            _columns.Add(new Column(columnName));
            return this;
        }

        public Builder Select(string[] columnNames)
        {
            _columns.Clear();
            foreach(string column in columnNames)
            {
                if (column.Length > 0)
                    _columns.Add(new Column(column));
            }
            return this;
        }

        public Builder AddSelect(ColumnList columns)
        {
            _columns.AddRange(columns);
            return this;
        }

        public Builder AddSelect(Column column)
        {
            _columns.Add(column);
            return this;
        }

        public Builder AddSelect(string columnName)
        {
            if (columnName.Length == 0)
                return this;

            _columns.Add(new Column(columnName));
            return this;
        }

        public Builder Distinct()
        {
            _distinct = true;
            return this;
        }

        public Builder From(string table)
        {
            _from = table;
            return this;
        }

        #endregion

        #region Joins

        public Builder Join(string table, string one, Is op = Is.Null, string two = null, JoinType type = JoinType.Inner, bool where = false)
        {
            var join = new JoinClause(type, table);

            _joins.Add( join.On(one, op, two, Conditional.And, where) );

            return this;
        }

        public Builder JoinWhere(string table, string one, Is op = Is.Null, string two = null, JoinType type = JoinType.Inner)
        {
            return Join(table, one, op, two, type, true);
        }

        public Builder LeftJoin(string table, string one, Is op = Is.Null, string two = null)
        {
            return Join(table, one, op, two, JoinType.Left);
        }

        public Builder LeftJoinWhere(string table, string one, Is op = Is.Null, string two = null)
        {
            return JoinWhere(table, one, op, two, JoinType.Left);
        }

        public Builder RightJoin(string table, string one, Is op = Is.Null, string two = null)
        {
            return Join(table, one, op, two, JoinType.Right);
        }

        public Builder RightJoinWhere(string table, string one, Is op = Is.Null, string two = null)
        {
            return JoinWhere(table, one, op, two, JoinType.Right);
        }

        #endregion

        #region Wheres

        public Builder Where(Column column, Is op, object value = null, Conditional condition = Conditional.And)
        {
            if(InvalidOperatorAndValue(op, value))
            {
                throw new InvalidArgumentException("A value must be provided");
            }

            // TODO: If the column is *** we assume the dev wants to begin a nested where statement 
            // which is wrapped in parenthesis.

            // TODO: Check if the dev is performing an entire sub-select within the query

            if(value == null)
            {
                return WhereNull(column, condition, op != Is.EqualTo);
            }

            WhereClause newWhere = new WhereClause(WhereType.Basic, column, op, value, condition);
            _wheres.Add(newWhere);

            if(value.GetType() != typeof(Expression))
            {
                AddBinding(BindingType.Where, value);
            }

            return this;
        }

        public Builder Where(string column, Is op, object value = null, Conditional condition = Conditional.And)
        {
            return Where(new Column(column), op, value, condition);
        }

        public Builder Where(QueryCallback callback, Conditional condition = Conditional.And)
        {
            return WhereNested(callback, condition);
        }

        public Builder OrWhere(string column, Is op, object value = null)
        {
            return Where(column, op, value, Conditional.Or);
        }

        public Builder OrWhere(QueryCallback callback)
        {
            return WhereNested(callback, Conditional.Or);
        }

        public Builder WhereBetween(string column, List<object> values, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause where = new WhereClause();
            where.Column = new Column(column);
            where.Type = WhereType.Between;
            where.Condition = condition;
            where.Not = not;

            _wheres.Add(where);

            AddBinding(BindingType.Where, values);

            return this;
        }

        public Builder OrWhereBetween(string column, List<object> values)
        {
            return WhereBetween(column, values, Conditional.Or);
        }

        public Builder WhereNotBetween(string column, List<object> values, Conditional condition = Conditional.And)
        {
            return WhereBetween(column, values, condition, true);
        }

        public Builder WhereNull(Column column, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause newWhere = new WhereClause((not ? WhereType.NotNull : WhereType.Null), column, condition);

            _wheres.Add(newWhere);

            return this;
        }

        public Builder OrWhereNull(Column column)
        {
            return WhereNull(column, Conditional.Or);
        }

        public Builder WhereNotNull(Column column, Conditional condition = Conditional.And)
        {
            return WhereNull(column, condition, true);
        }

        public Builder OrWhereNotNull(Column column)
        {
            return WhereNotNull(column, Conditional.Or);
        }

        public Builder WhereSubQuery(Column column, Is op, Builder subQuery)
        {
            // TODO: Add / merge subquery stuff...

            return this;
        }

        public Builder WhereNested(QueryCallback callback, Conditional condition = Conditional.And)
        {
            Builder query = NewQuery();
            query.From(_from);

            // Do something...
            callback(query);

            return AddNestedWhereQuery(query, condition);
        }

        public Builder AddNestedWhereQuery(Builder query, Conditional condition = Conditional.And)
        {
            if(query != null && query.Wheres.Count > 0)
            {
                WhereClause where = new WhereClause();
                where.Type = WhereType.Nested;
                where.Query = query;
                where.Condition = condition;

                _wheres.Add(where);

                MergeBindings(query);
            }

            return this;
        }

        #endregion

        #region Constraints

        public Builder Offset(int offsetValue)
        {
            _offset = Math.Max(0, offsetValue);
            return this;
        }

        public Builder Limit(int limit)
        {
            if (limit > 0)
                _limit = limit;

            return this;
        }

        public Builder Union(Builder query, bool all = false)
        {
            throw new NotImplementedException("union");
        }

        #endregion

        #endregion

        #region Query and execution

        /// <summary>
        /// Returns a new instance of this builder type
        /// </summary>
        /// <returns></returns>
        public Builder NewQuery()
        {
            return new Builder(_connection, _grammar);
        }

        /// <summary>
        /// Get the SQL representation of the query.
        /// </summary>
        /// <returns></returns>
        public string ToSql()
        {
            return _grammar.CompileSelect(this);
        }

        /// <summary>
        /// Execute a query for a single record by ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public ResultSet Find(int id, ColumnList columns)
        {
            if (columns.Count == 0)
            {
                columns.AddAllColumn();
            }
                
            return this.Where("id", Is.EqualTo, id).First(columns);
        }

        /// <summary>
        /// Execute the query and get the first result
        /// </summary>
        /// <returns></returns>
        public ResultSet First()
        {
            if (_columns.Count == 0)
                _columns.AddAllColumn();

            ResultSet results = this.Limit(1).Get(_columns);
            return results.Count > 0 ? results : null;
        }

        /// <summary>
        /// Execute the query and get the first result
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public ResultSet First(ColumnList columns)
        {
            ResultSet results = this.Limit(1).Get(columns);

            return results.Count > 0 ? results : null;
        }

        /// <summary>
        /// Execute the query as a "select" statement
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public ResultSet Get(ColumnList columns)
        {
            if (_columns.Count == 0)
                _columns = columns;

            // TODO: Check cache

            return RunSelect();
        }

        /// <summary>
        /// Execute the query as a "select" statement
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public ResultSet Get(string[] columns)
        {
            return Get(columns.ToColumnList());
        }

        /// <summary>
        /// Run the query as a "select" statement against the connection.
        /// </summary>
        /// <returns></returns>
        protected virtual ResultSet RunSelect()
        {
            return _connection.Select(ToSql(), Bindings);
        }

        #endregion

        #region Helper functions

        /// <summary>
        /// Checks if an operator on the value is legal.
        /// If the value is null, there can only be an "EqualTo" operator for it
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        /// <returns>Returns true if it is a legal pair, false if not</returns>
        private bool InvalidOperatorAndValue(Is op, object value)
        {
            return (op != Is.EqualTo && value == null);
        }

        /// <summary>
        /// Add a binding to the bindings collection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Builder AddBinding(BindingType type, object value)
        {
            // should never happend if the enum is used...
            if (!_bindings.ContainsKey(type))
                throw new InvalidArgumentException("Invalid binding type " + type.ToString());

            Type valueType = value.GetType();
            if(valueType.IsArray)
            {
                foreach (object val in value as IEnumerable<object>)
                {
                    _bindings[type].Add(val);
                }
            }
            else
            {
                _bindings[type].Add(value);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherQuery"></param>
        private void MergeBindings(Builder otherQuery)
        {
            foreach(KeyValuePair<BindingType, IList<object>> binding in otherQuery.Bindings)
            {
                foreach (object item in binding.Value)
                    _bindings[binding.Key].Add(item);
            }
        }

        #endregion
    }
}
