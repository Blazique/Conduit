using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conduit.Services.Access;

public interface Scope<C> where C : Component<C>
{
    public static abstract string Name { get; }
    public static abstract string Description { get; }
}
