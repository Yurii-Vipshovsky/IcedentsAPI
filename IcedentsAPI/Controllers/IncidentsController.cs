using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncedentsAPI.Data;
using IncedentsAPI.Models;

namespace IncedentsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    public class IncidentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IncidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All Incedents.
        /// </summary>
        /// <returns>A json list of Incidents</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Incidents/GetAll
        ///
        /// </remarks>
        /// <response code="200">Returns a json list of Incidents</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await _context.Incedents
                .Include(i=>i.Accounts)
                .ThenInclude(a=>a.Contacts)
                .ToListAsync();
            var incedents = await _context.Incedents.ToListAsync();

            return Ok(incedents);
        }

        /// <summary>
        /// Get specific Incident.
        /// </summary>
        /// <param name="incedentName"></param>
        /// <returns>A json of specific Incident</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Incidents/GetByName/:incedentName
        ///
        /// </remarks>
        /// <response code="200">Returns a json of specific Incident</response>
        /// <response code="404">If account with incedentName not found or incedentName is null</response>
        [HttpGet("{incedentName}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string incedentName)
        {
            if (incedentName == null)
            {
                return NotFound();
            }

            var incident = await _context.Incedents
                .Include(i => i.Accounts)
                .ThenInclude(a => a.Contacts)
                .FirstOrDefaultAsync(m => m.Name == incedentName);
            if (incident == null)
            {
                return NotFound();
            }

            return Ok(incident);
        }

        /// <summary>
        /// Create a new Incident.
        /// </summary>
        /// <returns>A json of created Incident</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Incidents/Create
        ///     {
        ///         "accountName": "string",
        ///         "contactFirstName": "string",
        ///         "contactLastName": "string",
        ///         "contactEmail": "email@email.com",
        ///         "incidentDescription": "string"
        ///     }
        ///
        /// If contactEmail exists than update contact information
        /// If doesn't exit create new contact
        /// </remarks>
        /// <response code="200">Returns a json of created Incident</response>
        /// <response code="400">If request body is null</response>
        /// <response code="404">If request account not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] RequestBody incident)
        {
            if (incident == null)
            {
                return BadRequest();
            }

            try
            {
                var account = await UpdateContactFromRequestBody(incident);
                var newIncedent = new Incident()
                {
                    Description = incident.IncidentDescription,
                };

                account.Incident = newIncedent;

                await _context.AddAsync(newIncedent);
                _context.Update(account);
                await _context.SaveChangesAsync();

                return Ok(newIncedent);
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Edit an existing Incident.
        /// </summary>
        /// <returns>A json of edited Incident</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Incidents/Edit/:incedentName
        ///     {
        ///         "accountName": "string",
        ///         "contactFirstName": "string",
        ///         "contactLastName": "string",
        ///         "contactEmail": "email@email.com",
        ///         "incidentDescription": "string"
        ///     }
        ///
        /// If contactEmail exists than update contact information
        /// If doesn't exit create new contact
        /// </remarks>
        /// <response code="200">Returns a json of edited Incident</response>
        /// <response code="400">If request body is null</response>
        /// <response code="404">If request account not found</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(string incidentName, [FromBody] RequestBody incident)
        {
            var oldIncedent = await _context.Incedents.FirstOrDefaultAsync(i => i.Name == incidentName);
            if (oldIncedent == null)
            {
                return NotFound();
            }
            try
            {
                var account = await UpdateContactFromRequestBody(incident);
                if(oldIncedent.Description != incident.IncidentDescription)
                {
                    oldIncedent.Description = incident.IncidentDescription;
                    _context.Update(oldIncedent);
                }

                account.IncedentName = incidentName;
            
                _context.Update(account);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentNullException ex)
            {
                return NotFound(ex.Message);
            }

            return Ok(oldIncedent);
        }

        /// <summary>
        /// Deletes a specific Incident item.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     DELETE: api/Incidents/Delete/:incedentName
        /// </remarks>
        /// <param name="incedentName"></param>
        /// <returns></returns>
        /// <response code="200">Signalize that incident deleted</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteConfirmed(string incedentName)
        {
            var incident = await _context.Incedents.FindAsync(incedentName);
            if (incident != null)
            {
                _context.Incedents.Remove(incident);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<Account> UpdateContactFromRequestBody(RequestBody incident)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Name == incident.AccountName);

            if (account == null)
            {
                throw new ArgumentNullException("Account not Found");
            }

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == incident.ContactEmail);
            if (contact != null)
            {
                bool isSame = true;
                if (contact.FirstName != incident.ContactFirstName)
                {
                    isSame = false;
                    contact.FirstName = incident.ContactFirstName;
                }
                if (contact.LastName != incident.ContactLastName)
                {
                    isSame = false;
                    contact.LastName = incident.ContactLastName;
                }
                if (contact.AccountName != incident.AccountName)
                {
                    isSame = false;
                    contact.AccountName = incident.AccountName;
                    contact.Account = account;
                }
                if (!isSame)
                {
                    _context.Update(contact);
                }
            }
            else
            {
                contact = new Contact()
                {
                    FirstName = incident.ContactFirstName,
                    LastName = incident.ContactLastName,
                    Email = incident.ContactEmail,
                    AccountName = incident.AccountName,
                    Account = account
                };
                await _context.Contacts.AddAsync(contact);
            }
            return account;
        }
    }
}
