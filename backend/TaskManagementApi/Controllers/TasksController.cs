using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Tasks;

namespace TaskManagementApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromServices]ITaskService taskService, CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetTasks(cancellationToken));
    }
}
