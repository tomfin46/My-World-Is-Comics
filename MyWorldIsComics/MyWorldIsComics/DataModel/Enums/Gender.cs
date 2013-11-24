using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.Enums
{
    public class Gender
    {
        public enum GenderEnum
        {
            Male,

            Female,

            Other
        }

        public static GenderEnum GetGender(int gender)
        {
            switch (gender)
            {
                case 1:
                    return GenderEnum.Male;
                case 2:
                    return GenderEnum.Female;
            }
            return GenderEnum.Other;
        }
    }
}
