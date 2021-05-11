using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi
{
    public class Property
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }		
		[ForeignKey(nameof(Type))]
		public int TypeId { get; set; }
		public virtual ModuleType Type { get; set; }
	}
}