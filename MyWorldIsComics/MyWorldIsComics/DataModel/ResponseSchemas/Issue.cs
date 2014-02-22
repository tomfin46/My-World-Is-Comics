using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    class Issue
    {
        public string Api_Detail_Url { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Issue_Number { get; set; }
    }
}
