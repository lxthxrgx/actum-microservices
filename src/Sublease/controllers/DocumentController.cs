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
    public async Task<IActionResult> SubleaseContract(Guid subleaseId, [FromQuery] bool download = false)
        => await GenerateAsync(subleaseId, ContractDocumentType.ContractWithActAndAnnex, download);

    [HttpPost("sublease/{subleaseId}/supplementary-agreement")]
    public async Task<IActionResult> SubleaseSupplementaryAgreement(Guid subleaseId, [FromQuery] bool download = false)
        => await GenerateAsync(subleaseId, ContractDocumentType.SupplementaryAgreement, download);


    [HttpPost("sublease/{subleaseId}/return-act")]
    public async Task<IActionResult> SubleaseReturnAct(Guid subleaseId, [FromQuery] bool download = false)
        => await GenerateAsync(subleaseId, ContractDocumentType.ReturnAct, download);

    [HttpPost("sublease/{subleaseId}/extension")]
    public async Task<IActionResult> SubleaseExtension(Guid subleaseId, [FromQuery] bool download = false)
        => await GenerateAsync(subleaseId, ContractDocumentType.ExtensionAgreement, download);

    private async Task<IActionResult> GenerateAsync(Guid subleaseId, ContractDocumentType docType, bool download)
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
            var paths = await document.CreateAsync(subleaseId);

            if (!download)
            {
                return Ok(new { message = "Документи збережено.", files = paths.Select(Path.GetFileName) });
            }

            if (paths.Count == 1)
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(paths[0]);
                return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Path.GetFileName(paths[0]));
            }

            using var ms = new MemoryStream();
            using (var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var path in paths)
                {
                    var entry = zip.CreateEntry(Path.GetFileName(path));
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(path);
                    await fileStream.CopyToAsync(entryStream);
                }
            }
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms.ToArray(), "application/zip", $"documents-{subleaseId}.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}