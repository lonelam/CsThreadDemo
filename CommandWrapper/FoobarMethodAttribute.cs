namespace CommandWrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FoobarMethodAttribute : Attribute
    {
    }

    public class AsyncFoobarMethodAttribute : FoobarMethodAttribute
    {
    }
}
