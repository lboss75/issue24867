using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi
{
    public class Module
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		public string Namespace { get; set; }
		public virtual IList<UIForm> Forms { get; set; }
		[InverseProperty(nameof(ModuleType.Module))]
        public virtual IList<ModuleType> ModuleTypes { get; set; }
		[ForeignKey(nameof(Project))]
		public int ProjectId { get; set; }
		[InverseProperty(nameof(WebApi.Project.Modules))]
		public virtual Project Project { get; set; }
    }
}