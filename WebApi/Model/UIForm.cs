using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi
{
    public class UIForm
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		[ForeignKey(nameof(BindedType))]
		public int? BindedTypeId { get; set; }
		//public virtual EntityType BindedType { get; set; }
		public virtual ModuleType BindedType { get; set; }
	}
}