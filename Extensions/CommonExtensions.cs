using System;

namespace GodotStudy.Extensions;

public static class EnumExtensions
{
    public static int ToInt(this Enum enumValue)
    {
        return Convert.ToInt32(enumValue);
    }
}
