using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
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
                };
                _ = await this.create_module(client, test_project.Id, module);
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
    }
}
