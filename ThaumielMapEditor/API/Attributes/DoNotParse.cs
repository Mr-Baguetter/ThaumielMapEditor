using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThaumielMapEditor.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DoNotParse : Attribute
    {
    }
}
