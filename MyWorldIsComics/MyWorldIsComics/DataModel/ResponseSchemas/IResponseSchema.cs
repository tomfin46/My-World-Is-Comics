using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.ResponseSchemas
{
    interface IResponseSchema
    {
        string Deck { get; set; }
        int Id { get; set; }
        Images Image { get; set; }
        string Name { get; set; }
        Publisher Publisher { get; set; }
    }
}
