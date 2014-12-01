using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AsysORM.EloqueNET.Reflection
{
    public class ColumnPropertyMetadata : PropertyMetadata
    {
        #region Var

        bool _visible = true;
        string _columnName;
        bool _guarded = false;

        #endregion

        #region Properties

        /// <summary>
        /// Wheter or not this column property should be visible in the result-set from a 
        /// db query
        /// </summary>
        public bool Visible 
        { 
            get { return _visible; } 
        }

        /// <summary>
        /// Wheter or not this column can be written to (updated) in the database
        /// </summary>
        public bool Guarded
        {
            get { return _guarded; }
            set { _guarded = value; }
        }

        /// <summary>
        /// Name of the column in the database
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
        }

        #endregion

        #region Init

        public ColumnPropertyMetadata(string tableColumnName)
            : base()
        {
            _columnName = tableColumnName;
        }

        public ColumnPropertyMetadata(string tableColumnName, bool visible, bool guarded)
            : base()
        {
            _columnName = tableColumnName;
            _visible = visible;
            _guarded = guarded;
        }

        public ColumnPropertyMetadata(object defaultValue, string tableColumnName, bool visible, bool guarded)
            : base(defaultValue)
        {
            _columnName = tableColumnName;
            _visible = visible;
            _guarded = guarded;
        }

        public ColumnPropertyMetadata(string tableColumnName, PropertyChangedCallback propertyChangedCallback)
            : base(propertyChangedCallback)
        {

        }

        public ColumnPropertyMetadata(string tableColumnName, bool visible, object defaultValue, PropertyChangedCallback propertyChangedCallback)
            : base(defaultValue, propertyChangedCallback)
        {

        }

        public ColumnPropertyMetadata(string tableColumnName, 
            bool visible,
            object defaultValue, 
            PropertyChangedCallback propertyChangedCallback, 
            CoerceValueCallback coerceValueCallback)
            : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {

        }

        #endregion

        #region Methods
        #endregion

        
    }
}
