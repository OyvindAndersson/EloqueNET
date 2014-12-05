using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    public class ColumnList : List<Column>
    {
        #region Var

        #endregion

        #region Properties
        #endregion

        #region Init
        public ColumnList()
            : base()
        {

        }

        public ColumnList(Column initialColumn)
            : base()
        {
            Add(initialColumn);
        }

        public ColumnList(string initialColumn)
            :base()
        {
            Add(new Column(initialColumn));
        }


        public ColumnList InitAdd(string column)
        {
            Add(new Column(column));
            return this;

        }
        public ColumnList InitAdd(string[] columns)
        {
            AddRange(columns.ToColumnList());
            return this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a range of columns to this list, only if the column name is not empty
        /// </summary>
        /// <param name="columns">IList<Column></param>
        public void AddRange(IList<Column> columns)
        {
            foreach(Column col in columns)
            {
                if(col.Name.Length > 0)
                    this.Add(col);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns"></param>
        public void AddRange(ColumnList columns)
        {
            foreach (Column col in columns)
            {
                if (col.Name.Length > 0)
                    this.Add(col);
            }
        }

        /// <summary>
        /// Adds an SQL '*' all column
        /// </summary>
        /// <returns>Index position of the column</returns>
        public void AddAllColumn()
        {
            Column col = new Column("*");

            if (!this.Contains(col, new CompareColumn()) )
                this.Add(col);
        }

        #endregion

        #region Sealed classes

        sealed class CompareColumn : IEqualityComparer<Column>
        {
            public bool Equals(Column a, Column b)
            {
                return a.Name == b.Name;
            }
            public int GetHashCode(Column column)
            {
                return 0;
            }
        }

        #endregion

    }

    public class Column
    {
        #region Var

        string _name;
        object _value;
        DbType _type;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public DbType ValueType
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion

        #region Init

        public Column(string name)
        {
            _name = name;
            _value = null;
            _type = DbType.String;
        }

        public Column(string name, object value)
        {
            _name = name;
            _value = value;
            _type = value.GetSqlValueType();
        }

        public Column(string name, object value, DbType type)
        {
            _name = name;
            _value = value;
            _type = type;
        }

        /// <summary>
        /// Create by passing a string explicitly
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static explicit operator Column(string name)
        {
            Column c = new Column(name);
            return c;
        }

        /// <summary>
        /// Implicit conversion of column to string, which gives the column name
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static implicit operator string(Column column)
        {
            return column.Name;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates if this column can be used in insert into, update and delete queries
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if(_name.Length == 0)
                return false;

            return true;
        }

        #endregion
    }
}
