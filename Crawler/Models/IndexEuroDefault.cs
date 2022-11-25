using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Models
{
    public class IndexEuroDefault
    {
        public virtual int Year { get; set; }

        public virtual int Month { get; set; }

        public virtual double Price { get; set; }
    }
}
