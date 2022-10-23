using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
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
		#region Project
		[EnableQuery]
		public IQueryable<Project> Get()
		{
			return this.db_.Projects;
		}
		[EnableQuery]
		public Project Get([FromODataUri] int key)
		{
			return this.Get().Single(x => x.Id == key);
		}
		static readonly string[] system_types = {
			"Boolean",  "Int32", "Int64", "String", "DateTime"
		};

		public async Task<IActionResult> Post([FromBody] Project value)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			#region Generate Namespace
			var ns = new System.Text.StringBuilder();
			foreach (var ch in value.Name)
			{
				if (char.IsLetterOrDigit(ch))
				{
					ns.Append(ch);
				}
				else
				{
					if (ns.Length > 0 && '.' != ns[ns.Length - 1])
					{
						ns.Append('.');
					}
				}
			}
			if (ns.Length == 0 || char.IsDigit(value.Name, 0))
			{
				ns.Insert(0, 'N');
			}

			#endregion
			var systemTypes = system_types.Select(x => new PrimitiveType { Name = x }).ToList<ModuleType>();

			value.Modules = new List<Module>
			{
				new Module
				{
					Name = "System",
					Namespace = "System",
					ModuleTypes = systemTypes,
				},
				new Module
				{
					Name = "Application",
					Namespace = ns.ToString() + ".App",
				}
			};
			await this.db_.Projects.AddAsync(value);
			await this.db_.SaveChangesAsync();
			return Created(value);
		}
		#endregion Project

		#region Modules
		[EnableQuery]
		public IQueryable<Module> GetModules([FromODataUri] int key)
		{
			return this.Get().Where(x => x.Id == key).SelectMany(x => x.Modules);
		}

		[EnableQuery]
		//[Microsoft.AspNetCode.OData.Routing.ODataRoute("Projects({key})/Modules({relatedKey})")]
		public Module GetModules([FromODataUri] int key, [FromODataUri] int relatedKey)
		{
			return this.Get().Where(x => x.Id == key).SelectMany(x => x.Modules).Single(y => y.Id == relatedKey);
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

		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({key})/Modules({relatedKey})")]
		public async Task<IActionResult> PutToModules([FromODataUri] int key, [FromODataUri] int relatedKey, [FromBody] Module value)
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

			var module = Project.Modules.SingleOrDefault(x => x.Id == relatedKey);
			if (null == module)
			{
				return NotFound($"Module {value.Id} was not found");
			}

			module.Name = value.Name;
			module.Namespace = value.Namespace;

			await this.db_.SaveChangesAsync();


			return Updated(value);
		}
		#endregion Modules

		#region Module Types
		[EnableQuery(MaxExpansionDepth = 5)]
		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/ModuleTypes")]
		public IQueryable<ModuleType> GetModuleTypes([FromODataUri] int project_key, [FromODataUri] int module_key)
		{
			return this.Get().Where(x => x.Id == project_key).SelectMany(x => x.Modules).Where(x => x.Id == module_key).SelectMany(x => x.ModuleTypes);
		}

		[EnableQuery(MaxExpansionDepth = 5)]
		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/ModuleTypes({type_key})")]
		public ModuleType GetModuleTypes([FromODataUri] int project_key, [FromODataUri] int module_key, [FromODataUri] int type_key)
		{
			return this.Get().Where(x => x.Id == project_key).SelectMany(x => x.Modules).Where(x => x.Id == module_key).SelectMany(x => x.ModuleTypes).SingleOrDefault(x => x.Id == type_key);
		}

		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/ModuleTypes")]
		public async Task<IActionResult> PostToModuleTypes([FromODataUri] int project_key, [FromODataUri] int module_key, [FromBody] ModuleType value)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var project = this.db_.Projects.SingleOrDefault(x => x.Id == project_key);
			if (null == project)
			{
				return NotFound($"Project {project_key} was not found");
			}
			var module = project.Modules.SingleOrDefault(x => x.Id == module_key);
			if (null == module)
			{
				return NotFound($"Module {module_key} was not found");
			}

			module.ModuleTypes.Add(value);
			await this.db_.SaveChangesAsync();
			return Created(value);
		}

		#endregion ModuleTypes

		#region Forms
		[EnableQuery(MaxExpansionDepth = 5)]
		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/Forms")]
		public IQueryable<UIForm> GetModuleForms([FromODataUri] int project_key, [FromODataUri] int module_key)
		{
			return this.Get().Where(x => x.Id == project_key).SelectMany(x => x.Modules).Where(x => x.Id == module_key).SelectMany(x => x.Forms).Include(x => x.BindedType).ThenInclude(x => x.Module);
		}

		[EnableQuery(MaxExpansionDepth = 5)]
		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/Forms({table_key})")]
		public UIForm GetModuleForms([FromODataUri] int project_key, [FromODataUri] int module_key, [FromODataUri] int form_key)
		{
			return this.Get().Where(x => x.Id == project_key).SelectMany(x => x.Modules).Where(x => x.Id == module_key).SelectMany(x => x.Forms).SingleOrDefault(x => x.Id == form_key);
		}

		//[Microsoft.AspNet.OData.Routing.ODataRoute("Projects({project_key})/Modules({module_key})/Forms")]
		public async Task<IActionResult> PostToModuleForms([FromODataUri] int project_key, [FromODataUri] int module_key, [FromBody] UIForm value)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var project = this.db_.Projects.SingleOrDefault(x => x.Id == project_key);
			if (null == project)
			{
				return NotFound($"Project {project_key} was not found");
			}
			var module = project.Modules.SingleOrDefault(x => x.Id == module_key);
			if (null == module)
			{
				return NotFound($"Module {module_key} was not found");
			}

			module.Forms.Add(value);
			await this.db_.SaveChangesAsync();
			return Created(value);
		}

		#endregion Forms
	}
}
