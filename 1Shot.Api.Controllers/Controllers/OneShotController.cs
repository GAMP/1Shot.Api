using _1Shot.Api.Controllers.Models;
using Gizmo.DAL;
using Gizmo.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace _1Shot.Api.Controllers.Controllers
{
    [ApiController]
    [Route("api/oneshot")]
    [Authorize(AuthenticationSchemes = "Basic,Bearer", Roles = ClaimNames.OperatorRoleName, Policy = "access-web-api")]  //add auth requirements
    public class OneShotController : ControllerBase
    {
        private readonly IGizmoDbContextProvider _gizmoDbContextProvider;

        public OneShotController(IGizmoDbContextProvider gizmoDbContextProvider)
        {
            _gizmoDbContextProvider = gizmoDbContextProvider;
        }

        /// <summary>
        /// Gets user by identification string.
        /// </summary>
        /// <param name="identification">Identification string.</param>
        /// <returns>User model, null if no user is found.</returns>
        [HttpGet("getuseridbyidentification")]
        public User? GetUserIdByIdentification([FromQuery] string identification)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var userMembers = dbContext.Set<Gizmo.DAL.Entities.UserMember>();
            var member = userMembers.SingleOrDefault(u => u.Identification == identification);
            if (member == null)
                return null;

            return new Models.User() { UserId = member.Id };
        }

        /// <summary>
        /// Gets if purchase (InvoiceLine) is marked as deleted.
        /// </summary>
        /// <param name="invoiceLineId">Invoice line id.</param>
        /// <param name="cancellationToken">HTTP request cancellation token.</param>
        /// <returns>True or false, null in case invoice line is not found in database.</returns>
        [HttpGet("isproductdeletedbyinvoicelineid")]
        public async Task<bool?> IsProductDeletedByInvoiceLineIdAsync([FromQuery] int invoiceLineId, CancellationToken cancellationToken = default)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var invoiceLineTime = dbContext.Set<Gizmo.DAL.Entities.InvoiceLineTime>();
            return await invoiceLineTime.Where(product => product.Id == invoiceLineId).Select(product => (bool?)product.IsDeleted).SingleOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Marks invoice line as deleted.
        /// </summary>
        /// <param name="invoiceLineTimeId">Invoice line id.</param>
        /// <param name="cancellationToken">HTTP request cancellation token.</param>
        /// <returns>True if marked successfully, in case invoice line is not found in database.</returns>
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

        /// <summary>
        /// Gets usable user time purchases (InvoiceLineTime).
        /// </summary>
        /// <param name="userId">User id.</param>
        /// <param name="cancellationToken">HTTP request cancellation token.</param>
        /// <returns>List of usable time products, empty list in case user has no products or not found in database.</returns>
        [HttpGet("getinvoicelinetimesbyuserid")]
        public async Task<IEnumerable<TimeProduct>> GetInvoiceLineTimesByUserIdAsync([FromQuery] int userId, CancellationToken cancellationToken = default)
        {
            using var dbContext = (DbContext)_gizmoDbContextProvider.GetDbNonProxyContext();
            var invoiceLineTime = dbContext.Set<Gizmo.DAL.Entities.InvoiceLineTime>();
            return await invoiceLineTime.Where(il => il.UserId == userId && il.IsDeleted == false && il.IsDepleted == false && il.IsVoided == false)
                .Select(il => new TimeProduct { Id = il.Id }).ToListAsync(cancellationToken);
        }
    }
}
