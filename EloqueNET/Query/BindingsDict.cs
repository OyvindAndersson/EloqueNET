using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Query
{
    public class BindingsDict : Dictionary<BindingType, IList<object>>
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init

        public BindingsDict()
        {
            InitToDefaultBindings();
        }

        public void InitToDefaultBindings()
        {
            Add(BindingType.Select, new List<object>() { });
            Add(BindingType.Join, new List<object>() { });
            Add(BindingType.Where, new List<object>() { });
            Add(BindingType.Having, new List<object>() { });
            Add(BindingType.Order, new List<object>() { });
        }

        #endregion

        #region Methods
        #endregion
    }
}
