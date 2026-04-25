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
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        [FromRoute] Guid id,
        [FromServices]ITaskService taskService,
        CancellationToken cancellationToken)
    {
        return Ok(await taskService.GetById(id, cancellationToken));
    }
    
    [HttpPost]
    public async Task<ActionResult> AddTask(
        [FromBody] AddTaskRequest taskRequest,
        [FromServices]ITaskService taskService, CancellationToken cancellationToken)
    {
        var task = await taskService.Add(taskRequest, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }
}
