using System;
using System.Collections.Generic;

namespace Reqnroll.Bindings.Discovery
{
    public interface IReqnrollAttributesFilter
    {
        IEnumerable<Attribute> FilterForReqnrollAttributes(IEnumerable<Attribute> customAttributes);
    }
}
