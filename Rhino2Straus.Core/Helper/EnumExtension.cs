using System;
using System.Linq;
using System.Reflection;

namespace Rhino2Straus.Core.Helper
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum genericEnum)
        {
            Type genericEnumType = genericEnum.GetType();
            MemberInfo[] memberInfos = genericEnumType.GetMember(genericEnum.ToString());
            if (memberInfos != null && memberInfos.Length > 0)
            {
                var attribs = memberInfos[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attribs != null && attribs.Length > 0)
                {
                    return ((System.ComponentModel.DescriptionAttribute)attribs.ElementAt(0)).Description;
                }
            }
            return genericEnum.ToString();
        }
    }
}
