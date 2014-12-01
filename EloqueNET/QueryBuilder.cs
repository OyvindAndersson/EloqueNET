using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using AsysORM.EloqueNET.Reflection;

namespace AsysORM.EloqueNET
{
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

    /// <summary>
    /// Represents orderby directions
    /// </summary>
    public enum Order
    {
        Ascending = 0,
        Descending,
    }

    /// <summary>
    /// A Class for building up a SQL query command by chaining methods together for flexible and readable building
    /// @todo Take a Grammar object in constructor, as some of these clauses work differently in other engines
    /// </summary>
    public class QueryBuilder
    {
        #region Var

        /// <summary>
        /// The command this builder should be working with/on
        /// </summary>
        IDbCommand _cmd;

        /// <summary>
        /// The working variable
        /// </summary>
        string _cmdText;

        /// <summary>
        /// The tablename this query is building for
        /// </summary>
        string _table;

        /// <summary>
        /// The type of query this builder is building (Select, insert, delete, etc)
        /// </summary>
        QueryType _queryType;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the current SQL query string
        /// </summary>
        public string CommandText
        {
            get { return _cmdText; }
        }

        #endregion

        #region Init

        public QueryBuilder(string table, DB connection)
        {
            _table = table;
            _cmd = connection.Command;

            _queryType = QueryType.None;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the table which the query is targeting.
        /// </summary>
        /// <param name="table">tablename</param>
        /// <returns></returns>
        public QueryBuilder From( string table)
        {
            _table = table;
            return this;
        }

        /// <summary>
        /// Selects a set of columns (or all if zero) on the table
        /// </summary>
        /// <param name="columns">An array of column names to select on the table</param>
        /// <returns>A querybuilder to select columns w/o conditions</returns>
        public QueryBuilder Select( string[] columns )
        {
            _queryType = QueryType.Select;

            _cmdText = "SELECT ";

            if (columns == null || columns.Length <= 0)
            {
                _cmdText += "* FROM " + _table;
            }
            else
            {
                for (int i = 0; i < columns.Length; i++ )
                {
                    _cmdText += columns[i];
                    if (i < columns.Length)
                        _cmdText += ",";
                }
            }

            return this;
        }

        /// <summary>
        /// Creates a statement that selects all columns from the table for this query builder
        /// </summary>
        /// <returns></returns>
        public QueryBuilder All()
        {
            _queryType = QueryType.Select;
            _cmdText = "SELECT * FROM " + _table;

            return this;
        }

        /// <summary>
        /// Adds a where clause to the query string. A method of query must have been set first (I.e: Select)
        /// </summary>
        /// <param name="column"></param>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public QueryBuilder Where(string column, Is condition, object value)
        {
            _cmdText += " WHERE " + column + " " + GetOperatorName(condition) + " '" + value.ToString() + "'"; 

            return this;
        }

        /// <summary>
        /// The IN operator allows you to specify multiple values in a WHERE clause.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public QueryBuilder WhereIn(string column, List<object> values)
        {
            if (values.Count == 0)
                return this;

            _cmdText += " WHERE " + column + " IN ('";

            object last = values[values.Count - 1];
            foreach (object value in values)
            {
                _cmdText += value.ToString();

                if (value != last)
                {
                    _cmdText += "','";
                }
            }
            _cmdText += "')";

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public QueryBuilder And(string column, Is condition, object value)
        {
            _cmdText += " AND " + column + " " + GetOperatorName(condition) + " '" + value.ToString() + "'"; 

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public QueryBuilder Or(string column, Is condition, object value)
        {
            _cmdText += " OR " + column + " " + GetOperatorName(condition) + " '" + value.ToString() + "'";

            return this;
        }

        /// <summary>
        /// Order by a column
        /// </summary>
        /// <param name="column">The column to order by</param>
        /// <param name="order">The order to use</param>
        /// <returns></returns>
        public QueryBuilder OrderBy(string column, Order order)
        {
            _cmdText += " ORDER BY " + column + " " + GetOrderName(order);

            return this;
        }

        /// <summary>
        /// Order by one or more columns
        /// </summary>
        /// <param name="columns">The column(s) to order by</param>
        /// <param name="order">The order to use</param>
        /// <returns></returns>
        public QueryBuilder OrderBy(List<string> columns, Order order)
        {

            _cmdText += " ORDER BY ";

            string lastItem = columns[columns.Count - 1];
            foreach(string col in columns)
            {
                // empty column names are not valid!
                if (col.Length <= 0)
                    continue;

                _cmdText += col;

                // If this is not the last column in line, seperate with comma
                // @todo should we check this? Might be good to actually give 
                // invalid syntax if anyone would provide empty col names
                if (col != lastItem)
                    _cmdText += ",";
            }

            _cmdText += " " + GetOrderName(order);

            return this;
        }

        /// <summary>
        /// Limits the result to <paramref name="count"/> results.
        /// </summary>
        /// <param name="count">The number of results to limit result-set to</param>
        /// <returns>updated query builder with limit</returns>
        public QueryBuilder Limit(int count)
        {
            _cmdText += " LIMIT " + count.ToString();

            return this;
        }


        public void Get()
        {
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns a string of the operator referred to in the 'Is' enum.
        /// </summary>
        /// <param name="conditionOperator">The operator to get the string representation from</param>
        /// <returns>The operator as a valid SQL syntax string</returns>
        private string GetOperatorName(Is conditionOperator)
        {
            switch(conditionOperator)
            {
                case Is.EqualTo:
                    return "=";

                case Is.Between:
                    return "BETWEEN";

                case Is.GreaterThan:
                    return ">";

                case Is.GreaterThanOrEqualTo:
                    return ">=";

                case Is.In:
                    return "IN";

                case Is.LessThan:
                    return "<";

                case Is.LessThanOrEqualTo:
                    return "<=";

                case Is.Like:
                    return "LIKE";

                case Is.NotLike:
                    return "NOT LIKE";

                case Is.NotEqualTo:
                    return "<>";

                case Is.NotNull:
                    return "IS NOT";

                case Is.Null:
                    return "IS";
            }

            throw new AsysORMException("No valid condition operator provided!");
        }

        /// <summary>
        /// Returns the textual sql syntax representation of the OrderBy values
        /// </summary>
        /// <param name="order">The order enum instance</param>
        /// <returns></returns>
        private string GetOrderName(Order order)
        {
            switch(order)
            {
                case Order.Ascending:
                    return "ASC";
                case Order.Descending:
                    return "DESC";
            }

            throw new AsysORMException("No valid order provided!");
        }

        #endregion
    }
}
