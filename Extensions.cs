using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aveva.Core.Database;

namespace VolumeWizardAddin
{
    public static class Extensions
    {
        public static bool GetOrCreateMember(this DbElement owner, string memberName, DbElementType memberType, out DbElement element)
        {
            if (!DbElement.Parse(memberName, out element, out _))
            {
                if (owner.IsCreatable(memberType))
                {
                    element = owner.Create(1, memberType);
                    element.SetAttribute(DbAttributeInstance.NAME, memberName);
                    return true;
                }
                else
                {
                    element = null;
                    return false;
                }
            }
            else
            {
                if (element.GetElement(DbAttributeInstance.OWNER) == owner && element.GetElementType() == memberType)
                {
                    return true;
                }
                else
                {
                    element = null;
                    return false;
                }
            }
        }
    }
}
