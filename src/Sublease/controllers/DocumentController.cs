using Lease.Factory;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.model;
using SharedLibraries.Database;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class DocumentController : ControllerBase
{
    private readonly DatabaseModel _context;

    public DocumentController(DatabaseModel context)
    {
        _context = context;
    }

    [HttpPost("sublease/{subleaseId}/contract")]
    public async Task<IActionResult> SubleaseContract(Guid subleaseId)
        => await GenerateAsync(subleaseId, ContractDocumentType.ContractWithActAndAnnex);

    [HttpPost("sublease/{subleaseId}/supplementary-agreement")]
    public async Task<IActionResult> SubleaseSupplementaryAgreement(Guid subleaseId)
        => await GenerateAsync(subleaseId, ContractDocumentType.SupplementaryAgreement);

    [HttpPost("sublease/{subleaseId}/return-act")]
    public async Task<IActionResult> SubleaseReturnAct(Guid subleaseId)
        => await GenerateAsync(subleaseId, ContractDocumentType.ReturnAct);

    [HttpPost("sublease/{subleaseId}/extension")]
    public async Task<IActionResult> SubleaseExtension(Guid subleaseId)
        => await GenerateAsync(subleaseId, ContractDocumentType.ExtensionAgreement);

    // ── Спільна логіка ────────────────────────────────────
    private async Task<IActionResult> GenerateAsync(Guid subleaseId, ContractDocumentType docType)
    {
        try
        {
            Console.WriteLine($"Sublease {subleaseId} not found.");
            var sublease = await _context.Subleases
                .Include(s => s.Group)
                    .ThenInclude(g => g.RentInfo)
                .Include(s => s.Group)
                    .ThenInclude(g => g.Counterparty)
                .Where(s => s.Id == subleaseId)
                .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException($"Sublease {subleaseId} not found.");

            Console.WriteLine($"RentInfo type: {sublease.Group.RentInfo?.GetType().Name}");
            Console.WriteLine($"RentInfo RentType value: {sublease.Group.RentInfo?.RentType}");
            Console.WriteLine($"Group: {sublease.Group?.Id}");
            Console.WriteLine($"Counterparty: {sublease.Group?.Counterparty?.GetType().Name}");
            Console.WriteLine($"Unknown rent type. {sublease.Group.RentInfo?.RentType.GetType().Name}");
            var leaseType = sublease.Group.RentInfo?.RentType switch
            {
                GroupRentType.Sublease => LeaseType.Sublease,
                GroupRentType.Type1 => LeaseType.DirectLease,
                GroupRentType.Type2 => LeaseType.FinancialLease,
                null => throw new InvalidOperationException("RentInfo is null."),
                _ => throw new InvalidOperationException("Unknown rent type.")
            };

            Console.WriteLine($"Unknown counterparty type: {sublease.Group.Counterparty.GetType().Name}");
            var counterparty = sublease.Group.Counterparty
                ?? throw new InvalidOperationException("Counterparty is null.");

            var contractorType = counterparty switch
            {
                CounterpartyLLC => ContractorType.Tov,
                CounterpartyFop => ContractorType.Fop,
                _ => throw new InvalidOperationException("Unknown counterparty type.")
            };

            var document = ContractDocumentFactory.Create(
                leaseType,
                contractorType,
                docType,
                _context);

            await document.CreateAsync(subleaseId);

            return Ok(new { message = "Document created." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}