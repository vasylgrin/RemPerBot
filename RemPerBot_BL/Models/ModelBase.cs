using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemBerBot_BL.Models
{
    public abstract class ModelBase
    {
        public abstract int Id { get; set; }
        public abstract long ChatId { get; set; }

        public abstract string PrintData();
    }
}
