using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApi;
using Xunit;

namespace Test
{
    public class ProjectBuilderTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        protected readonly WebApplicationFactory<Startup> factory_;
        public ProjectBuilderTest(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory_ = factory;
        }

        [Fact]
        public async Task CreateProjectTest()
        {
            const string project_name = "test_project";

            using (var client = this.factory_.CreateClient())
            {
                var test_project = await this.create_project(client, project_name);
                Assert.Equal(project_name, test_project.Name);

                var module = new Module
                {
                    Name = "Default Module",
                    Namespace = "TestProject.Default Module",
                };
                var odata_module = await this.create_module(client, test_project.Id, module);

                var project_types = await this.get_all_types(client, test_project.Id);
                var string_module = project_types.Single(x => x.Namespace == "System");
                var string_type = string_module.ModuleTypes.Single(x => x.Name == "String");

                var entity_type = new EntityType
                {
                    Name = "Type1",
                    Properties = new List<Property>
                    {
                        new Property
                        {
                            Name = "Name",
                            TypeId = string_type.Id,
                        }
                    }
                };
                var module_type = await this.create_module_type(client, test_project.Id, odata_module.Id, entity_type);
                Assert.Equal(entity_type.Name, module_type.Name);

                var form = new UIForm
                {
                    Name = "Form1",
                    BindedTypeId = module_type.Id
                };
                var module_form = await this.create_form(client, test_project.Id, odata_module.Id, form);
                Assert.Equal(form.Name, module_form.Name);

                var module_tables = await this.get_forms(client, test_project.Id, odata_module.Id);
                Assert.Contains(module_tables, x => x.Name == module_form.Name && x.BindedType.Module.Namespace + "." + x.BindedType.Name == odata_module.Namespace + "." + module_type.Name);
            }
        }
        protected async Task<Project> create_project(HttpClient client, string project_name)
        {
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Post, "/odata/Projects"))
            {
                registerMessage.Content = new StringContent(
                    JsonConvert.SerializeObject(new Project { Name = project_name },
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }),
                    Encoding.UTF8, "application/json");
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                Assert.Equal(System.Net.HttpStatusCode.Created, registerResult.StatusCode);
                return JsonConvert.DeserializeObject<Project>(body);
            }
        }
        protected async Task<Module> create_module(HttpClient client, int project_id, Module module)
        {
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Post, $"/odata/Projects({project_id})/Modules"))
            {
                registerMessage.Content = new StringContent(
                    SerializeObject(module),
                    Encoding.UTF8, "application/json");
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                registerResult.EnsureSuccessStatusCode();

                Assert.Equal(System.Net.HttpStatusCode.Created, registerResult.StatusCode);
                return JsonConvert.DeserializeObject<Module>(body);
            }
        }
        protected async Task<ModuleType> create_module_type(HttpClient client, int project_id, int module_id, ModuleType type)
        {
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Post, $"/odata/Projects({project_id})/Modules({module_id})/ModuleTypes"))
            {
                registerMessage.Content = new StringContent(
                    AddOdataType(SerializeObject(type), type.GetType().FullName),
                    Encoding.UTF8, "application/json");
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                registerResult.EnsureSuccessStatusCode();
                Assert.Equal(System.Net.HttpStatusCode.Created, registerResult.StatusCode);
                var items = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
                if ("#" + typeof(EntityType).FullName == items["@odata.type"])
                {
                    return JsonConvert.DeserializeObject<EntityType>(body);
                }
                else
                {
                    throw new ArgumentOutOfRangeException($"Invalid type {items["@odata.type"]}");
                }
            }
        }
        protected async Task<IEnumerable<Module>> get_all_types(HttpClient client, int project_id)
        {
            //Import.Odata.IVySoft.SiteBuilder.Mails.Model.
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Get, $"/odata/Projects({project_id})/Modules?$expand=ModuleTypes"))
            {
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                registerResult.EnsureSuccessStatusCode();
                Assert.Equal(System.Net.HttpStatusCode.OK, registerResult.StatusCode);
                var result = Newtonsoft.Json.Linq.JObject.Parse(body);
                return result["value"].Children().Select(x => Deserialize<Module>(x));
            }
        }
        protected async Task<UIForm> create_form(HttpClient client, int project_id, int module_id, UIForm form)
        {
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Post, $"/odata/Projects({project_id})/Modules({module_id})/Forms()"))
            {
                registerMessage.Content = new StringContent(
                    SerializeObject(form),
                    Encoding.UTF8, "application/json");
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                registerResult.EnsureSuccessStatusCode();

                Assert.Equal(System.Net.HttpStatusCode.Created, registerResult.StatusCode);
                return JsonConvert.DeserializeObject<UIForm>(body);
            }
        }
        protected async Task<IEnumerable<UIForm>> get_forms(HttpClient client, int project_id, int module_id)
        {
            using (var registerMessage = new HttpRequestMessage(HttpMethod.Get, $"/odata/Projects({project_id})/Modules({module_id})/Forms?$expand=BindedType($expand=Module)"))
            {
                var registerResult = await client.SendAsync(registerMessage);
                var body = await registerResult.Content.ReadAsStringAsync();

                registerResult.EnsureSuccessStatusCode();

                Assert.Equal(System.Net.HttpStatusCode.OK, registerResult.StatusCode);

                var result = Newtonsoft.Json.Linq.JObject.Parse(body);
                return result["value"].Children().Select(x => Deserialize<UIForm>(x));
            }
        }
        protected static string SerializeObject(object value)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            options.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return JsonConvert.SerializeObject(value, options);
        }
        private static void Prepare(Newtonsoft.Json.Linq.JToken body)
        {
            foreach (var token in body.Children())
            {
                if (token.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    var obj = (Newtonsoft.Json.Linq.JObject)token;
                    var prop = obj.Property("@odata.type");
                    if (null != prop)
                    {
                        var type_name = prop.Value.ToObject<string>();
                        if (!type_name.StartsWith("#"))
                        {
                            throw new ArgumentException($"Invalid tag @odata.type type name {type_name}");
                        }
                        obj.AddFirst(new Newtonsoft.Json.Linq.JProperty("$type", type_name.Substring(1) + "," + type_name.Substring(1, type_name.LastIndexOf('.') - 1)));
                        obj.Remove(prop.Name);
                    }
                }
                Prepare(token);
            }
        }
        public static T Deserialize<T>(Newtonsoft.Json.Linq.JToken body)
        {
            Prepare(body);
            return body.ToObject<T>(new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto });
        }

        protected string AddOdataType(string body, string type)
        {
            return $"{{\"@odata.type\":\"#{type}\", {body.Substring(1, body.Length - 2)}}}";
        }
    }
}
