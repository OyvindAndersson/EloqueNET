using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET.Grammar
{
    public class MysqlGrammar : BaseGrammar
    {
        #region Var
        #endregion

        #region Properties
        #endregion

        #region Init
        #endregion

        #region Methods

        protected override string WrapValue(string value)
        {
            if (value == "*")
                return value;

            return "`" + value.Replace("`", "``") + "`";
        }

        #endregion
    }
}
