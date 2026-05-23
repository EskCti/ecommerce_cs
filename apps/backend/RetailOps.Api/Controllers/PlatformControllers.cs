using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetailOps.Platform.Core.Application.Dtos;
using RetailOps.Platform.Core.Application.Queries;
using RetailOps.Platform.Core.Application.UseCases;

namespace RetailOps.Api.Controllers;

[ApiController]
[Route("api/platform")]
public sealed class PlatformTrialController(RegisterTrialCompanyUseCase registerTrial) : ControllerBase
{
    [HttpPost("trial")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterTrial([FromBody] RegisterTrialInDto request, CancellationToken ct)
    {
        var result = await registerTrial.Execute(request);
        if (result.IsFailure)
        {
            if (result.Error.Contains("Email already", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { error = result.Error });

            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

[ApiController]
[Route("api/platform/companies")]
[Authorize(Policy = "SasOnly")]
public sealed class PlatformCompaniesController(
    ListCompaniesQuery listCompanies,
    GetCompanyByIdQuery getCompany,
    CreateCompanyUseCase createCompany,
    UpdateCompanyUseCase updateCompany,
    SaveContractUseCase saveContract,
    IssueTenantInvoiceUseCase issueInvoice,
    SuspendOverdueTenantUseCase suspendTenant) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? name,
        [FromQuery] bool? active,
        [FromQuery] bool? trial,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await listCompanies.Execute(name, active, trial, page, pageSize, ct);
        return Ok(new { items = result.Items, total = result.Total });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var company = await getCompany.Execute(id, ct);
        return company is null ? NotFound() : Ok(company);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyInDto request, CancellationToken ct)
    {
        var result = await createCompany.Execute(request);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyBody body, CancellationToken ct)
    {
        var result = await updateCompany.Execute(new UpdateCompanyInDto(
            id, body.Name, body.Phone, body.Email, body.Cpf, body.Cnpj, body.MonthlyFee, body.Active));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPut("{id:int}/contracts")]
    public async Task<IActionResult> SaveContract(int id, [FromBody] SaveContractBody body, CancellationToken ct)
    {
        var result = await saveContract.Execute(new SaveContractInDto(id, body.ContractId, body.Text, body.SignedDate));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/invoices")]
    public async Task<IActionResult> IssueInvoice(int id, [FromBody] IssueInvoiceBody body, CancellationToken ct)
    {
        var result = await issueInvoice.Execute(new IssueInvoiceInDto(id, body.Amount, body.DueDate));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { issued = true });
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id, CancellationToken ct)
    {
        var result = await suspendTenant.Execute(new SuspendTenantInDto(id));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { suspended = true });
    }

    public sealed record UpdateCompanyBody(
        string Name,
        string? Phone,
        string? Email,
        string? Cpf,
        string? Cnpj,
        decimal MonthlyFee,
        bool Active);

    public sealed record SaveContractBody(int? ContractId, string Text, DateOnly SignedDate);
    public sealed record IssueInvoiceBody(decimal Amount, DateOnly DueDate);
}
