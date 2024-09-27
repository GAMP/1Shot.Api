using _1Shot.Api.Controllers.Models;
using Gizmo.DAL;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace _1Shot.Api.Controllers.Controllers
{
    [ApiController]
    [Route("api/oneshot")]
    public class OneShotController : ControllerBase
    {
        private readonly IGizmoDbContextProvider _gizmoDbContextProvider;

        public OneShotController(IGizmoDbContextProvider gizmoDbContextProvider)
        {
            _gizmoDbContextProvider = gizmoDbContextProvider;
        }

        [HttpGet("getuseridbyidentification")]
        public User GetUserIdByIdentification([FromQuery] string id)
        {
            User result = new();
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var userMembers = dbContext.Set<Gizmo.DAL.Entities.UserMember>();
            var member = userMembers.SingleOrDefault(u => u.Identification == id);
            if (member != null)
            {
                result.UserId = member.Id;
            }
            return result;
        }

        [HttpGet("isproductdeletedbyinvoicelineid")]
        public async Task<bool?> IsProductDeletedByInvoiceLineIdAsync([FromQuery] int invoiceLineId, CancellationToken cancellationToken = default)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var invoiceLineTime = dbContext.Set<Gizmo.DAL.Entities.InvoiceLineTime>();
            return await invoiceLineTime.Where(product => product.Id == invoiceLineId).Select(product => (bool?)product.IsDeleted).SingleOrDefaultAsync(cancellationToken);
        }

        [HttpPut("markinvoicelinetimeisdeletedbyid")]
        public async Task<bool> MarkInvoiceLineTimeIsDeletedByIdAsync([FromQuery] int invoiceLineTimeId, CancellationToken cancellationToken = default)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var invoiceLineTime = dbContext.Set<Gizmo.DAL.Entities.InvoiceLineTime>();
            Gizmo.DAL.Entities.InvoiceLineTime entity = new()
            {
                Id = invoiceLineTimeId,
                IsDeleted = true
            };

            if (!await invoiceLineTime.AnyAsync(il => il.Id == invoiceLineTimeId))
            {
                return false;
            }

            dbContext.Entry(entity).Property(i => i.IsDeleted).IsModified = true;
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        [HttpGet("getinvoicelinetimesbyuserid")]
        public async Task<IEnumerable<object>> GetInvoiceLineTimesByUserIdAsync([FromQuery] int userId, CancellationToken cancellationToken = default)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var invoiceLineTime = dbContext.Set<Gizmo.DAL.Entities.InvoiceLineTime>();
            return await invoiceLineTime.Where(il => il.UserId == userId && il.IsDeleted == false && il.IsDepleted == false && il.IsVoided == false)
                .Select(il => new TimeProduct{ Id = il.Id }).ToListAsync(cancellationToken);
        }
    }
}
