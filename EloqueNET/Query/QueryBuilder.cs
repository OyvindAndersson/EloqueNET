using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using AsysORM.Database;
using AsysORM.EloqueNET.Grammar;

namespace AsysORM.EloqueNET.Query
{
    #region Helpers and datatypes

    public delegate QueryBuilder QueryCallback(QueryBuilder query);

    /// <summary>
    /// Condition for a clause
    /// </summary>
    public enum Conditional
    {
        And = 0,
        Or
    }

    /// <summary>
    /// Type of valuebinding
    /// </summary>
    public enum BindingType
    {
        Select,
        Join,
        Where,
        Having,
        Order
    }

    /// <summary>
    /// Named query types
    /// </summary>
    public enum QueryType
    {
        None,
        Select = 0,
        Insert,
        Update,
        Delete,
    }

    /// <summary>
    /// Sql operators
    /// <see cref="AsysORM.EloqueNET.Extensions"/> GetOperatorName() extension method @ Extensions.cs, 
    /// which returns the sql-string of the operator 
    /// </summary>
    public enum Is
    {
        EqualTo = 0,
        GreaterThan,
        LessThan,

        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        NotEqualTo,

        Between,
        Like,
        NotLike,
        In,
        Null,
        NotNull,
    }

    #endregion

    /// <summary>
    /// SQL QueryBuilder class
    /// </summary>
    public class QueryBuilder
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
        private IList<HavingClause> _havings;
        private IList<OrderClause> _orders;
        private IList<UnionClause> _unions;
        private IList<string> _groups;

        private int _limit;
        private int _offset;


        public const string CreatedAtColumnName = "created_at";
        public const string UpdatedAtColumnName = "updated_at";

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

        public IAggregate GetAggregate
        {
            get { return _aggregate; }
        }

        public ColumnList Columns
        {
            get { return _columns; }
        }

        public bool IsDistinct
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

        public IList<string> Groups
        {
            get { return _groups; }
        }

        public IList<HavingClause> Havings
        {
            get { return _havings; }
        }

        public IList<OrderClause> Orders
        {
            get { return _orders; }
        }

        public IList<UnionClause> Unions
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

        public QueryBuilder(DB connection, BaseGrammar grammar)
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

            _havings = new List<HavingClause>();

            _orders = new List<OrderClause>();

            _joins = new List<JoinClause>();

            _groups = new List<string>();

            _aggregate = null;

            _columns = new ColumnList();

            _unions = new List<UnionClause>();

            _distinct = false;

            _from = "";
        }

        #endregion

        #region Methods

        #region Selects and constraints

        public QueryBuilder Select(ColumnList columns)
        {
            _columns = columns;
            return this;
        }

        public QueryBuilder Select(Column column)
        {
            _columns.Clear();
            _columns.Add(column);
            return this;
        }

        public QueryBuilder Select(string columnName)
        {
            _columns.Clear();
            _columns.Add(new Column(columnName));
            return this;
        }

        public QueryBuilder Select(string[] columnNames)
        {
            _columns.Clear();
            foreach (string column in columnNames)
            {
                if (column.Length > 0)
                    _columns.Add(new Column(column));
            }
            return this;
        }

        public QueryBuilder AddSelect(ColumnList columns)
        {
            _columns.AddRange(columns);
            return this;
        }

        public QueryBuilder AddSelect(Column column)
        {
            _columns.Add(column);
            return this;
        }

        public QueryBuilder AddSelect(string columnName)
        {
            if (columnName.Length == 0)
                return this;

            _columns.Add(new Column(columnName));
            return this;
        }

        public QueryBuilder Distinct()
        {
            _distinct = true;
            return this;
        }

        public QueryBuilder From(string table)
        {
            _from = table;
            return this;
        }

        #endregion

        #region Joins

        public QueryBuilder Join(string table, string one, Is op = Is.Null, string two = null, JoinType type = JoinType.Inner, bool where = false)
        {
            var join = new JoinClause(type, table);

            _joins.Add(join.On(one, op, two, Conditional.And, where));

            return this;
        }

        public QueryBuilder JoinWhere(string table, string one, Is op = Is.Null, string two = null, JoinType type = JoinType.Inner)
        {
            return Join(table, one, op, two, type, true);
        }

        public QueryBuilder LeftJoin(string table, string one, Is op = Is.Null, string two = null)
        {
            return Join(table, one, op, two, JoinType.Left);
        }

        public QueryBuilder LeftJoinWhere(string table, string one, Is op = Is.Null, string two = null)
        {
            return JoinWhere(table, one, op, two, JoinType.Left);
        }

        public QueryBuilder RightJoin(string table, string one, Is op = Is.Null, string two = null)
        {
            return Join(table, one, op, two, JoinType.Right);
        }

        public QueryBuilder RightJoinWhere(string table, string one, Is op = Is.Null, string two = null)
        {
            return JoinWhere(table, one, op, two, JoinType.Right);
        }

        #endregion

        #region Wheres

        public QueryBuilder Where(Column column, Is op, object value = null, Conditional condition = Conditional.And)
        {
            if (InvalidOperatorAndValue(op, value))
            {
                throw new InvalidArgumentException("A value must be provided");
            }

            if (value == null)
            {
                return WhereNull(column, condition, op != Is.EqualTo);
            }

            WhereClause newWhere = new WhereClause(ClauseType.Basic, column, op, value, condition);
            _wheres.Add(newWhere);

            if (value.GetType() != typeof(Expression))
            {
                AddBinding(BindingType.Where, value);
            }

            return this;
        }

        public QueryBuilder Where(string column, Is op, object value = null, Conditional condition = Conditional.And)
        {
            return Where(new Column(column), op, value, condition);
        }

        public QueryBuilder Where(QueryCallback callback, Conditional condition = Conditional.And)
        {
            return WhereNested(callback, condition);
        }

        public QueryBuilder Where(string column, Is op, QueryCallback value = null, Conditional condition = Conditional.And)
        {
            if (InvalidOperatorAndValue(op, value))
            {
                throw new InvalidArgumentException("A value must be provided");
            }

            if (value == null)
            {
                return Where(column, op, value, condition);
            }

            return WhereSubQuery(column, op, value, condition);
        }

        public QueryBuilder OrWhere(string column, Is op, object value = null)
        {
            return Where(column, op, value, Conditional.Or);
        }

        public QueryBuilder OrWhere(QueryCallback callback)
        {
            return WhereNested(callback, Conditional.Or);
        }

        public QueryBuilder WhereBetween(string column, List<object> values, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause where = new WhereClause();
            where.Column = new Column(column);
            where.Type = ClauseType.Between;
            where.Condition = condition;
            where.Not = not;

            _wheres.Add(where);

            AddBinding(BindingType.Where, values);

            return this;
        }

        public QueryBuilder OrWhereBetween(string column, List<object> values)
        {
            return WhereBetween(column, values, Conditional.Or);
        }

        public QueryBuilder WhereNotBetween(string column, List<object> values, Conditional condition = Conditional.And)
        {
            return WhereBetween(column, values, condition, true);
        }

        public QueryBuilder WhereNull(Column column, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause newWhere = new WhereClause((not ? ClauseType.NotNull : ClauseType.Null), column, condition);

            _wheres.Add(newWhere);

            return this;
        }

        public QueryBuilder OrWhereNull(Column column)
        {
            return WhereNull(column, Conditional.Or);
        }

        public QueryBuilder WhereNotNull(Column column, Conditional condition = Conditional.And)
        {
            return WhereNull(column, condition, true);
        }

        public QueryBuilder OrWhereNotNull(Column column)
        {
            return WhereNotNull(column, Conditional.Or);
        }

        /// <summary>
        /// Add a full sub-select to the query
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public QueryBuilder WhereSubQuery(Column column, Is op, QueryCallback callback, Conditional condition)
        {
            WhereClause where = new WhereClause();
            where.Column = column;
            where.Type = ClauseType.Sub;
            where.Operator = op;
            where.Query = NewQuery();
            where.Condition = condition;

            callback(where.Query);

            _wheres.Add(where);

            MergeBindings(where.Query);

            return this;
        }

        public QueryBuilder WhereSubQuery(string column, Is op, QueryCallback callback, Conditional condition)
        {
            return WhereSubQuery(new Column(column), op, callback, condition);
        }

        public QueryBuilder WhereNested(QueryCallback callback, Conditional condition = Conditional.And)
        {
            QueryBuilder query = NewQuery();
            query.From(_from);

            // Do something...
            callback(query);

            return AddNestedWhereQuery(query, condition);
        }

        public QueryBuilder AddNestedWhereQuery(QueryBuilder query, Conditional condition = Conditional.And)
        {
            if (query != null && query.Wheres.Count > 0)
            {
                WhereClause where = new WhereClause();
                where.Type = ClauseType.Nested;
                where.Query = query;
                where.Condition = condition;

                _wheres.Add(where);

                MergeBindings(query);
            }

            return this;
        }

        public QueryBuilder WhereExists(QueryCallback callback, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause where = new WhereClause();
            where.Type = not ? ClauseType.NotExists : ClauseType.Exists;
            where.Query = NewQuery();
            where.Condition = condition;

            callback(where.Query);

            _wheres.Add(where);
            MergeBindings(where.Query);

            return this;
        }

        public QueryBuilder OrWhereExists(QueryCallback callback, bool not = false)
        {
            return WhereExists(callback, Conditional.Or, not);
        }

        public QueryBuilder WhereNotExists(QueryCallback callback, Conditional condition = Conditional.And)
        {
            return WhereExists(callback, condition, true);
        }

        public QueryBuilder OrWhereNotExists(QueryCallback callback)
        {
            return OrWhereExists(callback, true);
        }

        public QueryBuilder WhereIn(Column column, object[] values, Conditional condition = Conditional.And, bool not = false)
        {
            WhereClause where = new WhereClause();
            where.Type = not ? ClauseType.NotIn : ClauseType.In;
            where.Column = column;
            where.Condition = condition;
            where.Value = new List<object>(values);

            _wheres.Add(where);
            AddBinding(BindingType.Where, values);

            return this;
        }

        public QueryBuilder WhereIn(string column, object[] values, Conditional condition = Conditional.And, bool not = false)
        {
            return WhereIn(new Column(column), values, condition, not);
        }

        public QueryBuilder OrWhereIn(Column column, object[] values)
        {
            return WhereIn(column, values, Conditional.Or);
        }

        public QueryBuilder OrWhereIn(string column, object[] values)
        {
            return WhereIn(new Column(column), values, Conditional.Or);
        }

        public QueryBuilder WhereNotIn(Column column, object[] values, Conditional condition = Conditional.And)
        {
            return WhereIn(column, values, condition, true);
        }

        public QueryBuilder WhereNotIn(string column, object[] values, Conditional condition = Conditional.And)
        {
            return WhereIn(new Column(column), values, condition, true);
        }

        public QueryBuilder OrWhereNotIn(Column column, object[] values)
        {
            return WhereIn(column, values, Conditional.Or, true);
        }

        public QueryBuilder OrWhereNotIn(string column, object[] values)
        {
            return WhereIn(new Column(column), values, Conditional.Or, true);
        }

        /// <summary>
        /// Add a "where-in" with a sub-select to the query
        /// </summary>
        /// <param name="column"></param>
        /// <param name="callback"></param>
        /// <param name="condition"></param>
        /// <param name="not"></param>
        /// <returns></returns>
        public QueryBuilder WhereInSub(Column column, QueryCallback callback, Conditional condition, bool not)
        {
            WhereClause where = new WhereClause();
            where.Type = not ? ClauseType.NotInSub : ClauseType.InSub;
            where.Column = column;
            where.Query = NewQuery();
            where.Condition = condition;

            callback(where.Query);

            MergeBindings(where.Query);

            return this;
        }

        public QueryBuilder WhereInSub(string column, QueryCallback callback, Conditional condition, bool not)
        {
            return WhereInSub(new Column(column), callback, condition, not);
        }

        public QueryBuilder WhereDay(Column column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Day, column, op, value, condition);
        }

        public QueryBuilder WhereDay(string column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Day, new Column(column), op, value, condition);
        }

        public QueryBuilder WhereMonth(Column column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Month, column, op, value, condition);
        }

        public QueryBuilder WhereMonth(string column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Month, new Column(column), op, value, condition);
        }

        public QueryBuilder WhereYear(Column column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Year, column, op, value, condition);
        }

        public QueryBuilder WhereYear(string column, Is op, int value, Conditional condition = Conditional.And)
        {
            return AddDatabaseWhere(ClauseType.Year, new Column(column), op, value, condition);
        }

        public QueryBuilder GroupBy(params string[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Length > 0)
                    _groups.Add(list[i]);
            }

            return this;
        }

        public QueryBuilder Having(Column column, Is op = Is.Null, object value = null, Conditional condition = Conditional.And)
        {
            HavingClause having = new HavingClause() { Type = ClauseType.Basic };

            _havings.Add(having);

            AddBinding(BindingType.Having, value);

            return this;
        }

        public QueryBuilder Having(string column, Is op = Is.Null, object value = null, Conditional condition = Conditional.And)
        {
            return Having(new Column(column), op, value, condition);
        }

        public QueryBuilder OrHaving(Column column, Is op = Is.Null, object value = null)
        {
            return Having(column, op, value, Conditional.Or);
        }

        public QueryBuilder OrHaving(string column, Is op = Is.Null, object value = null)
        {
            return OrHaving(new Column(column), op, value);
        }

        public QueryBuilder OrderBy(Column column, OrderDirection direction)
        {
            OrderClause order = new OrderClause() { Column = column, Direction = direction };

            _orders.Add(order);

            return this;
        }

        public QueryBuilder OrderBy(string column, OrderDirection direction)
        {
            return OrderBy(new Column(column), direction);
        }

        public QueryBuilder Latest(string column = CreatedAtColumnName)
        {
            return OrderBy(new Column(column), OrderDirection.Desc);
        }

        public QueryBuilder Oldest(string column = CreatedAtColumnName)
        {
            return OrderBy(new Column(column), OrderDirection.Asc);
        }

        #endregion

        #region Unions

        public QueryBuilder Union(QueryBuilder query, bool all = false)
        {
            _unions.Add(new UnionClause(query, all));

            MergeBindings(query);

            return this;
        }

        public QueryBuilder Union(QueryCallback query, bool all = false)
        {
            QueryBuilder q = NewQuery();
            query(q);

            _unions.Add(new UnionClause(q, all));

            MergeBindings(q);

            return this;
        }

        #endregion

        #region Constraints

        public QueryBuilder Offset(int offsetValue)
        {
            _offset = Math.Max(0, offsetValue);
            return this;
        }

        public QueryBuilder Limit(int limit)
        {
            if (limit > 0)
                _limit = limit;

            return this;
        }

        #endregion

        #region Aggregates

        public int Min(string column)
        {
            int r = 0;
            if (Int32.TryParse(Aggregate(AggregateFunction.Min, new ColumnList().InitAdd(column)).Value.ToString(), out r))
            {
                return r;
            }
            else
            {
                return r;
                //throw new InvalidOperationException("Returntype invalid!");
            }
        }

        public int Max(string column)
        {
            int r = 0;
            if (Int32.TryParse(Aggregate(AggregateFunction.Max, new ColumnList().InitAdd(column)).Value.ToString(), out r))
            {
                return r;
            }
            else
            {
                return r;
                //throw new InvalidOperationException("Returntype invalid!");
            }
        }

        public int Sum(string column)
        {
            // TODO Remove useless if/else if no logging/throwing should occur
            int r = 0;
            if (Int32.TryParse(Aggregate(AggregateFunction.Sum, new ColumnList().InitAdd(column)).Value.ToString(), out r))
            {
                return r;
            }
            else
            {
                return r;
                //throw new InvalidOperationException("Returntype invalid!");
            }
        }

        public double Avg(string column)
        {
            double r = 0;
            if (Double.TryParse(Aggregate(AggregateFunction.Avg, new ColumnList().InitAdd(column)).Value.ToString(), out r))
            {
                return r;
            }
            else
            {
                return r;
                //throw new InvalidOperationException("Returntype invalid!");
            }
        }

        public int Count(params string[] column)
        {
            ColumnList columns = new ColumnList();
            if (column.Length == 0)
            {
                columns.Add((Column)"*");
            }
            else
            {
                foreach (string col in column)
                {
                    columns.Add(new Column(col));
                }
            }


            // TODO Remove useless if/else if no logging/throwing should occur
            int r = 0;
            if (Int32.TryParse(Aggregate(AggregateFunction.Count, columns).Value.ToString(), out r))
            {
                return r;
            }
            else
            {
                return r;
                //throw new InvalidOperationException("Returntype invalid!");
            }
        }

        public Column Aggregate(AggregateFunction function, ColumnList columns)
        {
            if (columns.Count == 0)
                columns.AddAllColumn();

            _aggregate = new Aggregate();
            _aggregate.Columns = columns;
            _aggregate.Function = function;

            ColumnList prevColumns = _columns;

            ResultSet results = Get(columns);

            _aggregate = null;
            _columns = prevColumns;

            if (results.Count > 0 && results[0] != null)
            {
                Console.WriteLine(results[0].ToString());

                return results[0]["aggregate"];
            }

            return null;
        }

        #endregion

        #endregion

        #region Query and execution

        /// <summary>
        /// Returns a new instance of this builder type
        /// </summary>
        /// <returns></returns>
        public QueryBuilder NewQuery()
        {
            return new QueryBuilder(_connection, _grammar);
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

        public ResultSet Get()
        {
            return RunSelect();
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
        public ResultSet Get(params string[] columns)
        {
            return Get(columns.ToColumnList());
        }

        public int Insert(params Column[] columns)
        {
            ColumnList list = new ColumnList();
            foreach(Column col in columns)
            {
                if (col.IsValid())
                    list.Add(col);
            }

            string sql = _grammar.CompileInsert(this, list);

            return _connection.Insert(sql, list);
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
        public QueryBuilder AddBinding(BindingType type, object value)
        {
            // should never happend if the enum is used...
            if (!_bindings.ContainsKey(type))
                throw new InvalidArgumentException("Invalid binding type " + type.ToString());

            Type valueType = value.GetType();
            if (valueType.IsArray)
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
        private void MergeBindings(QueryBuilder otherQuery)
        {
            foreach (KeyValuePair<BindingType, IList<object>> binding in otherQuery.Bindings)
            {
                foreach (object item in binding.Value)
                    _bindings[binding.Key].Add(item);
            }
        }

        protected QueryBuilder AddDatabaseWhere(ClauseType type, Column column, Is op, object value, Conditional condition)
        {
            WhereClause where = new WhereClause()
            {
                Type = type,
                Column = column,
                Operator = op,
                Value = value,
                Condition = condition
            };

            _wheres.Add(where);
            AddBinding(BindingType.Where, value);

            return this;
        }

        #endregion
    }
}
