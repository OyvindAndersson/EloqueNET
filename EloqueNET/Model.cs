using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

using AsysORM.EloqueNET.Reflection;

namespace AsysORM.EloqueNET
{
    public class ModelTimestampEventArgs : EventArgs
    {
        #region Var

        DateTime _timestamp;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the DateTime event arg
        /// </summary>
        public DateTime TimeStamp
        {
            get { return _timestamp; }
        }

        #endregion

        #region Init

        /// <summary>
        /// Creates a new instance of the model timestamp event args
        /// </summary>
        /// <param name="time"></param>
        public ModelTimestampEventArgs(DateTime time)
        {
            _timestamp = time;
        }

        #endregion

    }

    /// <summary>
    /// A model class that maps directly to a row in a specific table in a database.
    /// 
    /// Any dependency property registered with the ColumnPropertyMetadata propertymetadata class is
    /// considered a column in a table. See <see cref="AsysORM.EloqueNET.Reflection.ColumnPropertyMetadata"/> for docs on
    /// how to configure the metadata to fit the needs.
    /// 
    /// @todo - Add functionality for relations
    /// @todo - Change column to attribute (naming convention)
    /// </summary>
    public class Model : DependencyObject, IModel
    {
        #region Var

        string _table;
        bool _incrementing = true;
        bool _exists = false;

        

        #endregion

        #region Regular Dependency property registration

        public static readonly DependencyProperty PrimaryKeyProperty = DependencyProperty.Register("PrimaryKey", typeof(string), typeof(Model),
            new PropertyMetadata("id"));

        public static readonly DependencyProperty UseTimestampsProperty = DependencyProperty.Register("UseTimestamps", typeof(bool), typeof(Model),
            new PropertyMetadata(true));

        #endregion

        #region Regular properties

        /// <summary>
        /// 
        /// </summary>
        public string PrimaryKey
        {
            get { return (string)GetValue(PrimaryKeyProperty); }
            set { SetValue(PrimaryKeyProperty, value); }
        }

        /// <summary>
        /// Indicates wheter the model should insert created_at and updated_at timestamps in the database.
        /// </summary>
        public bool UseTimestamps
        {
            get { return (bool)GetValue(UseTimestampsProperty); }
            set { SetValue(UseTimestampsProperty, value); }
        }

        /// <summary>
        /// Indicates if the model exists
        /// </summary>
        public bool Exists
        {
            get { return _exists; }
        }

        #endregion

        #region Column Dependency property registration

        public static readonly DependencyProperty CreatedAtProperty = DependencyProperty.Register("CreatedAt", typeof(DateTime), typeof(Model),
            new ColumnPropertyMetadata(DateTime.UtcNow, "created_at", true, true));

        public static readonly DependencyProperty UpdatedAtProperty = DependencyProperty.Register("UpdatedAt", typeof(DateTime), typeof(Model),
            new ColumnPropertyMetadata(DateTime.UtcNow, "updated_at", true, true));

        #endregion

        #region Column Properties

        /// <summary>
        /// Gets created at datetime
        /// </summary>
        public DateTime CreatedAt
        {
            get { return (DateTime)GetValue(CreatedAtProperty); }
            protected set { SetValue(CreatedAtProperty, value); }
        }

        /// <summary>
        /// Stores the time of a update in the database
        /// </summary>
        public DateTime UpdatedAt
        {
            get { return (DateTime)GetValue(UpdatedAtProperty); }
            protected set { SetValue(UpdatedAtProperty, value); }
        }

        #endregion

        #region Events

        public event EventHandler<ModelTimestampEventArgs> ModelCreatedEvent;
        public event EventHandler<ModelTimestampEventArgs> ModelUpdatedEvent;

        #endregion

        #region Event handlers

        protected virtual void OnModelCreatedEvent(ModelTimestampEventArgs e)
        {
            EventHandler<ModelTimestampEventArgs> handler = ModelCreatedEvent;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnModelUpdatedEvent(ModelTimestampEventArgs e)
        {
            EventHandler<ModelTimestampEventArgs> handler = ModelUpdatedEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region Init

        public Model()
        {

        }

        #endregion

        #region Methods

        public Model Save()
        {
            // save to db
            // update / create


            return this;
        }

        public string GetTable()
        {
            if (_table.Length > 0)
                return _table;

            // If no table names has been provided, we assume the models' name in snakecase
            return this.GetType().Name.ToSnakeCase();
        }

        #endregion
    }
}
