using System.Collections.Generic;

namespace WebApi
{
    public class EntityType : ModuleType
    {
        public virtual IList<Property> Properties { get; set; }
    }
}