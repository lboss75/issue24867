using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace WebApi
{
    internal class ODataModel
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var projects = builder.EntitySet<Project>("Projects");
            builder.EntityType<Project>().HasMany(x => x.Modules).Contained();
            builder.EntityType<Module>().HasMany(x => x.ModuleTypes).Contained();
            builder.EntityType<Module>().HasMany(x => x.Forms).Contained();
            builder.EntityType<EntityType>().HasMany(x => x.Properties).Contained();

            builder.EntityType<UIForm>().HasOptional(x => x.BindedType).IsNavigable();
            builder.EntityType<ModuleType>().HasRequired(x => x.Module).IsNavigable();

            return builder.GetEdmModel();
        }
    }
}