using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullNode
{
    public class Node
    {
        public ConnectionManager ConnectionManager { get; set; }
        public TransactionManager TransactionManager { get; set; }
    }
}
