using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsysORM.EloqueNET
{
    
    public class Schema
    {
        #region Static builder methods

        public static void Create(string name)
        {
            ITable table = new Table(name);


        }

        #endregion
    }
}
