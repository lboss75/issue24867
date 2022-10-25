using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace WebApi
{
    internal class ODataModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var projects = builder.EntitySet<Project>("Projects");

            projects.EntityType.HasMany(x => x.Modules).Contained();
            //builder.EntityType<Project>().HasMany(x => x.Modules).Contained();
            //builder.EntityType<Project>().CollectionProperty(x => x.Modules);

            return builder.GetEdmModel();
        }
    }
}