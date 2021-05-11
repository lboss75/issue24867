using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi
{
    public abstract class ModuleType
    {
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[MaxLength(100)]
		public string Name { get; set; }
		[ForeignKey(nameof(Module))]
		public int ModuleId { get; set; }
		[InverseProperty(nameof(WebApi.Module.ModuleTypes))]
		public virtual Module Module { get; set; }
	}
}
