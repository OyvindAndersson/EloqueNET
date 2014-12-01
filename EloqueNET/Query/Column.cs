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
        string _type;

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

        public string ValueType
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
            _type = null;
        }

        public Column(string name, object value, string type)
        {
            _name = name;
            _value = value;
            _type = type;
        }

        #endregion

        #region Methods
        #endregion
    }
}
