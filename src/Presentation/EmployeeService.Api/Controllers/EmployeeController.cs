using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BuildingBlocks.Presentation.Controllers;
using EmployeeService.Application.Features.Employees.Commands.CreateEmployee;

namespace EmployeeService.Api.Controllers
{
    [Authorize]
    [Asp.Versioning.ApiVersion("1.0")]
    public class EmployeeController : ApiControllerBase
    {
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
        {
            return CreateResponse(await Mediator.Send(command));
        }

        [Authorize(Roles = "Employee,Admin,Manager")]
        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            return Ok(BuildingBlocks.Presentation.Models.ApiResponse<object>.Success(new { Message = $"Employee {id} retrieved successfully." }, TraceId));
        }

        [Authorize(Roles = "Employee,Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] EmployeeService.Application.Features.Employees.Queries.GetEmployees.GetEmployeesQuery query)
        {
            return CreateResponse(await Mediator.Send(query));
        }

        [AllowAnonymous]
        [HttpGet("public-info")]
        public IActionResult GetPublicInfo()
        {
            return Ok(BuildingBlocks.Presentation.Models.ApiResponse<object>.Success(new { Info = "Public endpoint without token." }, TraceId));
        }
    }
}
