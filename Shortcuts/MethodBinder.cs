using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class MethodBinder : Binder
        {
            public MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref Object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out Object state)
            {
                throw new NotImplementedException("Not yet implemented");
            }

            public FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, Object value, CultureInfo culture)
            {
                throw new NotImplementedException("Not yet implemented");
            }

            public void ReorderArgumentArray(ref Object[] args, Object state)
            {
                throw new NotImplementedException("Not yet implemented");
            }

            public MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException("Not yet implemented");
            }

            public abstract PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
            {
                throw new NotImplementedException("Not yet implemented");
            }
        }
    }
}
