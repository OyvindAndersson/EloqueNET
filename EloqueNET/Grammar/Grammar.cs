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

        public string CompileSelect(Builder query)
        {
            ResetBindingsIndexer();

            if (query.Columns == null || query.Columns.Count == 0)
                query.Columns.AddAllColumn();

            return ConcatenateComponents(CompileComponents(query));
        }

        public string CompileInsert(Builder query, List<object> values)
        {
            return "";
        }

        public string CompileInsertGetId(Builder query, List<object> values, string sequence)
        {
            return "";
        }

        public string CompileUpdate(Builder query, List<object> values)
        {
            return "";
        }

        public string CompileDelete(Builder query)
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
        protected Dictionary<SelectComponents, string> CompileComponents(Builder query)
        {
            var sql = new Dictionary<SelectComponents, string>();

            // Compile aggregate

            // Compile columns
            if(query.Columns != null && query.Columns.Count > 0)
                sql.Add(SelectComponents.Columns, CompileColumns(query, query.Columns));

            // Compile from
            if(query.GetFrom.Length > 0)
                sql.Add(SelectComponents.From, CompileFrom(query, query.GetFrom));

            // Compile joins
            if(query.Joins != null && query.Joins.Count > 0)
                sql.Add(SelectComponents.Joins, CompileJoins(query, query.Joins));

            // Compile wheres
            if(query.Wheres != null && query.Wheres.Count > 0)
                sql.Add(SelectComponents.Wheres, CompileWheres(query));

            if(query.Groups != null && query.Groups.Count > 0)
                sql.Add(SelectComponents.Groups, CompileGroups(query, query.Groups));

            if(query.Havings != null && query.Havings.Count > 0)
                sql.Add(SelectComponents.Havings, CompileHavings(query));

            if(query.Orders != null && query.Orders.Count > 0)
                sql.Add(SelectComponents.Orders, CompileOrders(query, query.Orders));

            if(query.GetLimit > 0)
                sql.Add(SelectComponents.Limit, CompileLimit(query, query.GetLimit));

            if(query.GetOffset > 0)
                sql.Add(SelectComponents.Offset, CompileOffset(query, query.GetOffset));

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
        protected string CompileAggregate(Builder query, IAggregate aggregate)
        {
            return "";
        }

        /// <summary>
        /// Compile together all columns for the select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        protected string CompileColumns(Builder query, ColumnList columns)
        {
            if (query.Aggregate != null)
                return "";

            var select = query.GetDistinct ? "select distinct " : "select ";

            return select + Columnize(columns);
        }

        /// <summary>
        /// Compile the from table select
        /// </summary>
        /// <param name="query"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        protected string CompileFrom(Builder query, string table)
        {
            return "from " + WrapTable(table);
        }

        protected string CompileJoins(Builder query, IList<JoinClause> joins)
        {
            return "";
        }

        protected string CompileJoinConstraint(JoinClause clause)
        {
            return "";
        }

        /// <summary>
        /// Compile any where type. This is the main entry for compiling a where.
        /// A loop is performed over the query' 'Wheres' list, and the appropriate
        /// method is called for compiling the where clause depending on its type.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected string CompileWheres(Builder query)
        {
            var sql = new List<string>();

            if (query.Wheres == null)
                return "";

            foreach(WhereClause where in query.Wheres)
            {
                switch(where.Type)
                {
                    case WhereType.Basic:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereBasic(query, where));
                        break;
                    case WhereType.Between:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereBetween(query, where));
                        break;
                    case WhereType.Exists:
                        break;
                    case WhereType.In:
                        break;
                    case WhereType.InSub:
                        break;
                    case WhereType.Nested:
                        sql.Add(where.Condition.ToString().ToLower() + " " + WhereNested(query, where));
                        break;
                    case WhereType.NotExists:
                        break;
                    case WhereType.NotIn:
                        break;
                    case WhereType.NotInSub:
                        break;
                    case WhereType.NotNull:
                        break;
                    case WhereType.Null:
                        break;
                    case WhereType.Raw:
                        break;
                    case WhereType.Sub:
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

        protected string WhereNested(Builder query, WhereClause where)
        {
            if (where.Query == null)
                return "";

            // We substring to get rid of the "where " that is returned from CompileWheres
            return "(" + CompileWheres(where.Query).Substring(6) + ")";
        }

        protected string WhereSub(Builder query)
        {
            return "";
        }

        protected string WhereBasic(Builder query, WhereClause where)
        {
            var value = Parameter(where.Value);
            return Wrap(where.Column.Name) + " " + where.Operator.GetOperatorName() + " " + value;
        }

        protected string WhereBetween(Builder query, WhereClause where)
        {
            return Wrap(where.Column.Name) + " " + ( where.Not ? "not between" : "between" ) + " ? and ?";
        }

        protected string WhereExists(Builder query, WhereClause where)
        {
            return "exists (" + CompileSelect(where.Query) + ")";
        }

        protected string WhereNotExists(Builder query, WhereClause where)
        {
            return "not " + WhereExists(query, where);
        }

        protected string WhereIn(Builder query, WhereClause where)
        {
            //var values = Parameterize(where.Value);
            return "";
        }

        protected string WhereNotIn(Builder query, WhereClause where)
        {
            return "";
        }

        protected string WhereInSub(Builder query, WhereClause where)
        {
            return "";
        }

        protected string WhereNotInSub(Builder query, WhereClause where)
        {
            return "";
        }

        protected string WhereNull(Builder query, WhereClause where)
        {
            return "";
        }

        protected string WhereNotNull(Builder query, WhereClause where)
        {
            return "";
        }

        protected string CompileGroups(Builder query, object groups)
        {
            return "";
        }

        protected string CompileHavings(Builder query)
        {
            return "";
        }

        protected string CompileHaving(object having)
        {
            return "";
        }

        protected string CompileBasicHaving(object having)
        {
            return "";
        }

        protected string CompileOrders(Builder query, object orders)
        {
            return "";
        }

        protected string CompileLimit(Builder query, int limit)
        {
            return "limit " + limit.ToString();
        }

        protected string CompileOffset(Builder query, int offset)
        {
            return "offset " + offset.ToString();
        }

        protected string CompileUnions(Builder query, object unions)
        {
            return "";
        }

        protected string CompileUnion(object union)
        {
            return "";
        }

        protected string CompileLock(Builder query, bool value)
        {
            return "";
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
        public string Wrap(string value)
        {
            if( value.IndexOf(" as ") != -1 )
            {
                var segments = value.Split(' ');

                return Wrap(segments[0]) + " as " + Wrap(segments[2]);
            }

            var wrapped = new List<string>();
            var segs = value.Split('.');

            // If the value is not an aliased table expression, we'll just wrap it like normal, so if there is more than one
            // segment, we will wrap the first segments as if it was a table and the rest as just regular values.
            foreach (string segment in segs)
            {
                if(segs.Length > 1)
                {
                    wrapped.Add(WrapTable(segment));
                }
                else
                {
                    wrapped.Add(WrapValue(segment));
                }
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

        private string Parameterize(object[] values)
        {
            List<string> v = new List<string>();

            foreach(string value in values)
            {
                v.Add(Parameter(value));
            }

            return String.Join(", ", v.ToArray());
        }

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

        private void ResetBindingsIndexer()
        {
            _paramNameIndexer = 0;
        }

        #endregion

        #endregion
    }
}
