namespace MyWorldIsComics.DataModel.Enums
{
    internal class Month
    {
        public enum MonthEnum
        {
            Jan,

            Feb,

            Mar,

            Apr,

            May,

            Jun,

            Jul,

            Aug,

            Sep,

            Oct,

            Nov,

            Dec
        }
        
        public static int GetMonthInt(MonthEnum resourcesEnum)
        {
            switch (resourcesEnum)
            {
                case MonthEnum.Jan:
                    return 1;
                case MonthEnum.Feb:
                    return 2;
                case MonthEnum.Mar:
                    return 3;
                case MonthEnum.Apr:
                    return 4;
                case MonthEnum.May:
                    return 5;
                case MonthEnum.Jun:
                    return 6;
                case MonthEnum.Jul:
                    return 7;
                case MonthEnum.Aug:
                    return 8;
                case MonthEnum.Sep:
                    return 9;
                case MonthEnum.Oct:
                    return 10;
                case MonthEnum.Nov:
                    return 11;
                case MonthEnum.Dec:
                    return 12;
            }
            return 0;
        }

        public static int GetMonthInt(string month)
        {
            switch (month)
            {
                case "Jan":
                    return 1;
                case "Feb":
                    return 2;
                case "Mar":
                    return 3;
                case "Apr":
                    return 4;
                case "May":
                    return 5;
                case "Jun":
                    return 6;
                case "Jul":
                    return 7;
                case "Aug":
                    return 8;
                case "Sep":
                    return 9;
                case "Oct":
                    return 10;
                case "Nov":
                    return 11;
                case "Dec":
                    return 12;
            }
            return 0;
        }
    }
}
