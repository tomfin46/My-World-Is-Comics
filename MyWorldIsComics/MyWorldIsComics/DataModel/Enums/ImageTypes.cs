using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWorldIsComics.DataModel.Enums
{
    class ImageTypes
    {
        public enum ImageTypesEnum
        {
            SquareMini, SquareAvatar, ScaleAvatar, ScaleSmall, ScaleMedium, ScaleLarge, ScreenMedium
        }

        public static string GetImageType(ImageTypesEnum imageTypesEnum)
        {
            switch (imageTypesEnum)
            {
                case ImageTypesEnum.SquareMini:
                    return "square_mini";
                case ImageTypesEnum.SquareAvatar:
                    return "square_avatar";
                case ImageTypesEnum.ScaleAvatar:
                    return "scale_avatar";
                case ImageTypesEnum.ScaleSmall:
                    return "scale_small";
                case ImageTypesEnum.ScaleMedium:
                    return "scale_medium";
                case ImageTypesEnum.ScaleLarge:
                    return "scale_large";
                case ImageTypesEnum.ScreenMedium:
                    return "screen_medium";
            }
            return "";
        }
    }
}
