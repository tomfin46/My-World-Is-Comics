using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWorldIsComics.DataModel.Enums;

namespace MyWorldIsComics.DataModel.Interfaces
{
    interface IPerson
    {
        DateTime Birth { get; set; }
        Gender.GenderEnum Gender { get; set; }
    }
}
