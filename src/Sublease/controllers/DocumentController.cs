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
            var builder = new ContractDataBuilder(_context);
            var data = await builder.BuildAsync(subleaseId);

            var contractorType = data.IsFop ? ContractorType.Fop : ContractorType.Tov;
            var leaseType = data.RentInfo?.RentType switch
            {
                GroupRentType.Sublease => LeaseType.Sublease,
                GroupRentType.Type1 => LeaseType.Rent1,
                GroupRentType.Type2 => LeaseType.Rent2,
                _ => throw new InvalidOperationException("Unknown rent type.")
            };

            var document = ContractDocumentFactory.Create(leaseType, contractorType, docType, _context);
            await document.CreateAsync(subleaseId);

            return Ok(new { message = "Document created." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}