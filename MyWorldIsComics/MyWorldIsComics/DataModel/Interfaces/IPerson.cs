using System;
using MyWorldIsComics.DataModel.Enums;

namespace MyWorldIsComics.DataModel.Interfaces
{
    interface IPerson
    {
        DateTime Birth { get; set; }
        Gender.GenderEnum Gender { get; set; }
    }
}
