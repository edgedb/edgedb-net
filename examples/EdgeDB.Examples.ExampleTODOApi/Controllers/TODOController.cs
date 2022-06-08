using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EdgeDB.Examples.ExampleTODOApi.Controllers
{
    public class TODOController : Controller
    {
        private readonly EdgeDBClient _client;

        public TODOController(EdgeDBClient client)
        {
            _client = client;
        }

        [HttpGet("/todos")]
        public async Task<IActionResult> GetTODOs()
        {
            var todos = await _client.QueryAsync<TODOModel>("select TODO { title, description, state, date_created }").ConfigureAwait(false);

            return Ok(todos);
        }

        [HttpPost("/todos")]
        public async Task<IActionResult> CreateTODO([FromBody]TODOModel todo)
        {
            // validate request
            if (string.IsNullOrEmpty(todo.Title) || string.IsNullOrEmpty(todo.Description))
                return BadRequest();

            var query = "insert TODO { title := <str>$title, description := <str>$description, state := <State>$state }";
            await _client.ExecuteAsync(query, new Dictionary<string, object?>
            {
                {"title", todo.Title},
                {"description", todo.Description},
                {"state", todo.State }
            });

            return NoContent();
        }

        [HttpDelete("/todos")]
        public async Task<IActionResult> DeleteTODO([FromQuery, Required]string title)
        {
            var result = await _client.QueryAsync<object>("delete TODO filter .title = <str>$title", new Dictionary<string, object?> { { "title", title } });
            
            return result.Count > 0 ? NoContent() : NotFound();
        }

        [HttpPatch("/todos")]
        public async Task<IActionResult> UpdateTODO([FromQuery, Required] string title, [FromQuery, Required]TODOState state)
        {
            var result = await _client.QueryAsync<object>("update TODO filter .title = <str>$title set { state := <State>$state }", new Dictionary<string, object?> 
            { 
                { "title", title } ,
                { "state", state }
            });
            return result.Count > 0 ? NoContent() : NotFound();
        }
    }
}
