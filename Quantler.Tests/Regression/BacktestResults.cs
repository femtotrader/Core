using Quantler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantler.Tests.Regression
{
    class BacktestResults
    {
        public Result Result { get; set; }
        public PendingOrder[] Orders { get; set; }
        public Trade[] Trades { get; set; }
    }
}
