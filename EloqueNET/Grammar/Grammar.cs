using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AsysORM.EloqueNET.Query;

namespace AsysORM.EloqueNET.Grammar
{
    public enum SelectComponents
    {
        Aggregate,
        Columns,
        From,
        Joins,
        Wheres,
        Groups,
        Havings,
        Orders,
        Limit,
        Offset,
        Unions,
        Lock
    }

    public class BaseGrammar
    {
        #region Var

        private int _paramNameIndexer;
        private const string _paramNamePrefix = "@binding";

        #endregion

        #region Properties

        public string ParameterPrefix
        {
            get { return _paramNamePrefix; }
        }

        public int ParameterNameIndex
        {
            get { return _paramNameIndexer; }
        }

        #endregion

        #region Init

        public BaseGrammar()
        {
            _paramNameIndexer = 0;
        }

        #endregion

        #region Methods

        #region Public compile types (select, insert, update and delete)

        public string CompileSelect(QueryBuilder query)
        {
            ResetBindingsIndexer();

            if (query.Columns == null || query.Columns.Count == 0)
                query.Columns.AddAllColumn();

            return ConcatenateComponents(CompileComponents(query));
        }

        public string CompileInsert(QueryBuilder query, List<object> values)
        {
            return "";
        }

        public string CompileInsertGetId(QueryBuilder query, List<object> values, string sequence)
        {
            return "";
        }

        public string CompileUpdate(QueryBuilder query, List<object> values)
        {
            return "";
        }

        public string CompileDelete(QueryBuilder query)
        {
            return "";
        }

        #endregion

        #region Component compiles

        /// <summary>
        /// Compile all components of the select query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected Dictionary<SelectComponents, string> CompileComponents(QueryBuilder query)
        {
            var sql = new Dictionary<SelectComponents, string>();

            // aggregate
            if(query.GetAggregate != null)
                sql.Add(SelectComponents.Aggregate, CompileAggregate(query, query.GetAggregate));

            // columns
            if(query.Columns != null && query.Columns.Count > 0)
                sql.Add(SelectComponents.Columns, CompileColumns(query, query.Columns));

            // from
            if(query.GetFrom.Length > 0)
                sql.Add(SelectComponents.From, CompileFrom(query, query.GetFrom));

            // joins
            if(query.Joins != null && query.Joins.Count > 0)
                sql.Add(SelectComponents.Joins, CompileJoins(query, query.Joins));

            // wheres
            if(query.Wheres != null && query.Wheres.Count > 0)
                sql.Add(SelectComponents.Wheres, CompileWheres(query));

            // Groups
            if(query.Groups != null && query.Groups.Count > 0)
                sql.Add(SelectComponents.Groups, CompileGroups(query, query.Groups));

            // Havings
            if(query.Havings != null && query.Havings.Count > 0)
                sql.Add(SelectComponents.Havings, CompileHavings(query, query.Havings));

            // Orders
            if(query.Orders != null && query.Orders.Count > 0)
                sql.Add(SelectComponents.Orders, CompileOrders(query, query.Orders));

            // Limit
            if(query.GetLimit > 0)
                sql.Add(SelectComponents.Limit, CompileLimit(query, query.GetLimit));

            // Offset
            if(query.GetOffset > 0)
                sql.Add(SelectComponents.Offset, CompileOffset(query, query.GetOffset));

            // Unions
            if(query.Unions != null && query.Unions.Count > 0)
                sql.Add(SelectComponents.Unions, CompileUnions(query, query.Unions));

            return sql;

        }

        /// <summary>
        /// Compile an aggregate function
        /// </summary>
        /// <param name="query"></param>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        protected string CompileAggregate(QueryBuilder query, IAggregate aggregate)
        {
            string col = Columnize(aggregate.Columns);

            if(query.IsDistinct && col != "*")
            {
                col = "distinct " + col;
            }

            return "select " + aggregate.Function.ToString().ToLower() + "(" + col + ") as aggregate";
        }

        /// <summary>
        /// Compile together all columns for the select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected string CompileColumns(QueryBuilder query, ColumnList columns)
        {
            if (query.GetAggregate != null)
                return "";

            var select = query.IsDistinct ? "select distinct " : "select ";

            return select + Columnize(columns);
        }

        /// <summary>
        /// Compile the from table select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        protected string CompileFrom(QueryBuilder query, string table)
        {
            return "from " + WrapTable(table);
        }

        protected string CompileJoins(QueryBuilder query, IList<JoinClause> joins)
        {
            IList<string> sql = new List<string>();

            foreach(JoinClause join in joins)
            {
                string table = WrapTable(join.Table);
                IList<string> clauses = new List<string>();

                foreach(ClauseInJoin clause in join.Clauses)
                {
                    clauses.Add(CompileJoinConstraint(clause));
                }

                foreach(string binding in join.Bindings)
                {
                    query.AddBinding(BindingType.Join, binding);
                }


                clauses[0] = clauses[0].PregReplace("and |or ", "", 1);
                string clausesSql = String.Join(" ", clauses);

                sql.Add(join.Type.ToString().AddSpacesToSentence(false).ToLower() + " join " + table + " on " + clausesSql);
            }

            return String.Join(" ", sql);
        }

        protected string CompileJoinConstraint(ClauseInJoin clause)
        {
            string first = Wrap(clause.First);
            string second = clause.Where ? "?" : Wrap(clause.Second);

            return clause.Condition.ToString().ToLower() + " " + first + " " + clause.Operator.GetOperatorName() + " " + second;
        }

        /// <summary>
        /// Compile any where type. This is the main entry for compiling a where.
        /// A loop is performed over the query' 'Wheres' list, and the appropriate
        /// method is called for compiling the where clause depending on its type.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected string CompileWheres(QueryBuilder query)
        {
            var sql = new List<string>();

            if (query.Wheres == null)
                return "";

            foreach(WhereClause where in query.Wheres)
            {
                switch(where.Type)
                {
                    case ClauseType.Basic:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereBasic(query, where));
                        break;
                    case ClauseType.Between:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereBetween(query, where));
                        break;
                    case ClauseType.Exists:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereExists(query, where));
                        break;
                    case ClauseType.In:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereIn(query, where));
                        break;
                    case ClauseType.InSub:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereInSub(query, where));
                        break;
                    case ClauseType.Nested:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNested(query, where));
                        break;
                    case ClauseType.NotExists:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNotExists(query, where));
                        break;
                    case ClauseType.NotIn:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNotIn(query, where));
                        break;
                    case ClauseType.NotInSub:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNotInSub(query, where));
                        break;
                    case ClauseType.NotNull:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNotNull(query, where));
                        break;
                    case ClauseType.Null:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNull(query, where));
                        break;
                    case ClauseType.Raw:
                        break;
                    case ClauseType.Sub:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereSub(query, where));
                        break;
                    default:
                        continue;
                }
            }

            if(sql.Count > 0)
            {
                string ret = String.Join(" ", sql.ToArray());

                return "where " + ret.PregReplace("and |or ", "", 1);
            }

            return "";
        }

        protected string WhereNested(QueryBuilder query, WhereClause where)
        {
            if (where.Query == null)
                return "";

            // We substring to get rid of the "where " that is returned from CompileWheres
            return "(" + CompileWheres(where.Query).Substring(6) + ")";
        }

        protected string WhereSub(QueryBuilder query, WhereClause where)
        {
            string select = CompileSelect(where.Query);
            return Wrap(where.Column.Name) + " " + where.Operator.GetOperatorName() + " (" + select + ")";
        }

        protected string WhereBasic(QueryBuilder query, WhereClause where)
        {
            var value = Parameter(where.Value);
            return Wrap(where.Column.Name) + " " + where.Operator.GetOperatorName() + " " + value;
        }

        protected string WhereBetween(QueryBuilder query, WhereClause where)
        {
            return Wrap(where.Column.Name) + " " + ( where.Not ? "not between" : "between" ) + " ? and ?";
        }

        protected string WhereExists(QueryBuilder query, WhereClause where)
        {
            return "exists (" + CompileSelect(where.Query) + ")";
        }

        protected string WhereNotExists(QueryBuilder query, WhereClause where)
        {
            return "not " + WhereExists(query, where);
        }

        protected string WhereIn(QueryBuilder query, WhereClause where)
        {
            string values = Parameterize(where.Value as IEnumerable<object>);
            return Wrap(where.Column.Name) + " in (" + values + ")";
        }

        protected string WhereNotIn(QueryBuilder query, WhereClause where)
        {
            string values = Parameterize(where.Value as IEnumerable<object>);
            return Wrap(where.Column.Name) + " not in (" + values + ")";
        }

        protected string WhereInSub(QueryBuilder query, WhereClause where)
        {
            string select = CompileSelect(where.Query);
            return Wrap(where.Column.Name) + " in (" + select + ")";
        }

        protected string WhereNotInSub(QueryBuilder query, WhereClause where)
        {
            string select = CompileSelect(where.Query);
            return Wrap(where.Column.Name) + " not in (" + select + ")";
        }

        protected string WhereNull(QueryBuilder query, WhereClause where)
        {
            return Wrap(where.Column.Name) + " is null";
        }

        protected string WhereNotNull(QueryBuilder query, WhereClause where)
        {
            return Wrap(where.Column.Name) + " is not null";
        }

        protected string WhereDay(QueryBuilder query, WhereClause where)
        {
            return DatabasedWhere(ClauseType.Day, query, where);
        }

        protected string WhereMonth(QueryBuilder query, WhereClause where)
        {
            return DatabasedWhere(ClauseType.Month, query, where);
        }

        protected string WhereYear(QueryBuilder query, WhereClause where)
        {
            return DatabasedWhere(ClauseType.Year, query, where);
        }

        protected string DatabasedWhere(ClauseType type, QueryBuilder query, WhereClause where)
        {
            string value = Parameter(where.Value);
            return type.ToString().ToLower() + "(" + Wrap(where.Column.Name) + ") " + where.Operator.GetOperatorName() + " " + value;
        }

        protected string CompileGroups(QueryBuilder query, IList<string> groups)
        {
            return "group by " + Columnize(groups);
        }

        protected string CompileHavings(QueryBuilder query, IList<HavingClause> havings)
        {
            // Split each compiled having sql with " " space
            string sql = String.Join(" ", havings.Select(n => CompileHaving(n)) );

            return "having " + sql.PregReplace("and ", "", 1);
        }

        protected string CompileHaving(HavingClause having)
        {
            // TODO: Check for RAW type
            return CompileBasicHaving(having);
        }

        protected string CompileBasicHaving(HavingClause having)
        {
            string column = having.Column.Name;
            string param = Parameter(having.Value);

            return having.Condition.ToString().ToLower() + " " + column + " " + having.Operator.GetOperatorName() + " " + param;
        }

        protected string CompileOrders(QueryBuilder query, IList<OrderClause> orders)
        {
            IList<string> orderSql = new List<string>();
            foreach(OrderClause order in orders)
            {
                orderSql.Add(Wrap(order.Column.Name) + " " + order.Direction.ToString().ToLower());
            }

           return "order by " + String.Join(", ", orderSql);
        }

        protected string CompileLimit(QueryBuilder query, int limit)
        {
            return "limit " + limit.ToString();
        }

        protected string CompileOffset(QueryBuilder query, int offset)
        {
            return "offset " + offset.ToString();
        }

        protected string CompileUnions(QueryBuilder query, IList<UnionClause> unions)
        {
            string sql = "";

            foreach(UnionClause union in query.Unions)
            {
                sql += CompileUnion(union);
            }
            return sql.TrimStart(' ');
        }

        protected string CompileUnion(UnionClause union)
        {
            string joiner = union.All ? " union all " : " union ";
            return joiner + union.Query.ToSql();
        }

        protected string CompileLock(QueryBuilder query, bool value)
        {
            return "";
        }

        #endregion

        #region Insert

        public string CompileInsert(QueryBuilder query, ColumnList values)
        {
            string table = WrapTable(query.GetFrom);
            string columns = Columnize(values);
            string parameters = Parameterize(values);

            return "insert into " + table + " (" + columns + ") values (" + parameters + ")";
        }

        #endregion

        #region Utility

        private string ConcatenateSegments(List<string> segments)
        {
            segments = segments.Where(s => !string.IsNullOrEmpty(s)).ToList();

            return String.Join(" ", segments.ToArray());
        }

        private string ConcatenateComponents(Dictionary<SelectComponents, string> components)
        {
            List<string> lst = new List<string>();
            foreach (KeyValuePair<SelectComponents, string> entry in components)
            {
                if (entry.Value != null && entry.Value.Length > 0)
                    lst.Add(entry.Value);
            }

            return String.Join(" ", lst.ToArray());
        }

        /// <summary>
        /// Wrap a value in keyword identifiers.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Wrap(object value)
        {
            if(value.GetType() == typeof(Expression))
            {
                return (value as Expression).ToString(); // TODO
            }

            string strVal = value.ToString();
            if (strVal.IndexOf(" as ") != -1)
            {
                var segments = strVal.Split(' ');

                return Wrap(segments[0]) + " as " + WrapValue(segments[2]);
            }

            var wrapped = new List<string>();
            var segs = strVal.Split('.');

            // If the value is not an aliased table expression, we'll just wrap it like normal, so if there is more than one
            // segment, we will wrap the first segments as if it was a table and the rest as just regular values.
            int i = 0;
            foreach (string segment in segs)
            {
                if(i == 0 && segs.Length > 1)
                {
                    wrapped.Add(WrapTable(segment));
                }
                else
                {
                    wrapped.Add(WrapValue(segment));
                }
                i++;
            }

            return String.Join(".", wrapped);
        }

        /// <summary>
        /// TODO: Add support for table prefix
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        protected string WrapTable(string table)
        {
            return Wrap(table);
        }

        protected virtual string WrapValue(string value)
        {
            if (value == "*")
                return value;

            return "\"" + value.Replace("\"", @"""") + "\"";
        }

        protected string Parameter(object value)
        {
            string paramPlaceholder = DB.BindingNamePrefix + ParameterNameIndex.ToString();
            _paramNameIndexer++;

            return paramPlaceholder;
        }

        private string Parameterize(IEnumerable<object> enumerable)
        {
            List<string> v = new List<string>();
            foreach(object value in enumerable)
            {
                v.Add(Parameter(value));
            }

            return String.Join(", ", v.ToArray());
        }

        /*
        private string Parameterize(object[] values)
        {
            List<string> v = new List<string>();

            foreach(string value in values)
            {
                v.Add(Parameter(value));
            }

            return String.Join(", ", v.ToArray());
        }*/

        private string Columnize(ColumnList columns)
        {
            string delimStr = "";

            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].Name.Length > 0)
                {
                    delimStr += Wrap(columns[i].Name);


                    if (i != columns.Count - 1)
                        delimStr += ", ";
                }
            }

            return delimStr;
        }

        private string Columnize(IList<string> columns)
        {
            string delimStr = "";

            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].Length > 0)
                {
                    delimStr += Wrap(columns[i]);


                    if (i != columns.Count - 1)
                        delimStr += ", ";
                }
            }

            return delimStr;
        }

        private void ResetBindingsIndexer()
        {
            _paramNameIndexer = 0;
        }

        #endregion

        #endregion
    }
}
