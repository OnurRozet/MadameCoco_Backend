using MediatR;
using Microsoft.AspNetCore.Mvc;
using MadameCoco.Order.API.Features.Queries;
using MadameCoco.Shared.BaseModels;
using MadameCoco.Order.API.Features.Order.Commands.OrderCommands;
using MadameCoco.Order.API.Features.Order.Queries.OrderQueries;
using MadameCoco.Order.API.Features.Order.Results;

namespace MadameCoco.Order.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllOrdersQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsSuccess && result.ResultObject?.Detail is not null)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

}
