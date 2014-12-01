using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
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

    public class JoinClause
    {
        #region Var

        /// <summary>
        /// The 'on' clauses for the join
        /// </summary>
        List<Dictionary<string, object>> _clauses;

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

        public List<Dictionary<string, object>> Clauses
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
            _clauses = new List<Dictionary<string, object>>();
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
            var clause = new Dictionary<string, object>();
            clause.Add("first", first);
            clause.Add("operator", op);
            clause.Add("second", second);
            clause.Add("boolean", condition);
            clause.Add("where", where);

            _clauses.Add(clause);

            if(where)
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
}
