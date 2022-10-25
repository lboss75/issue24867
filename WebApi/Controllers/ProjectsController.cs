using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
	public class ProjectsController : ODataController
	{
		private readonly DbModel db_;
		public ProjectsController(DbModel db)
		{
			this.db_ = db;
		}

		public async Task<IActionResult> Post([FromBody] Project value)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			value.Modules = new List<Module>();
			await this.db_.Projects.AddAsync(value);
			await this.db_.SaveChangesAsync();
			return Created(value);
		}


		public async Task<IActionResult> PostToModules([FromODataUri] int key, [FromBody] Module value)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var Project = this.db_.Projects.SingleOrDefault(x => x.Id == key);
			if (null == Project)
			{
				return NotFound($"Project {key} was not found");
			}

			Project.Modules.Add(value);
			await this.db_.SaveChangesAsync();
			return Created(value);
		}

	}
}
