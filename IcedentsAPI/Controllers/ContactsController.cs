using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncedentsAPI.Data;
using IncedentsAPI.Models;
using System.Text.RegularExpressions;
using IncedentsAPI.Services;

namespace IncedentsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All Contacts.
        /// </summary>
        /// <returns>A json list of Contacts</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/contacts/GetAll
        ///
        /// </remarks>
        /// <response code="200">Returns a json list of Contacts</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _context.Contacts.Include(c => c.Account).ToListAsync();
            return Ok(contacts);
        }

        /// <summary>
        /// Get specific Contact.
        /// </summary>
        /// <param name="ContactEmail"></param>
        /// <returns>A json of specific Contact</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/contacts/GetByName/:contactEmail
        ///
        /// </remarks>
        /// <response code="200">Returns a json of specific Contact</response>
        /// <response code="404">If contact with contactEmail not found or contactEmail is null</response>
        [HttpGet("{contactEmail}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string ContactEmail)
        {
            if (ContactEmail == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.Account)
                .FirstOrDefaultAsync(m => m.Email == ContactEmail);
            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }

        /// <summary>
        /// Create a new Contact.
        /// </summary>
        /// <returns>A json of created Contact</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/contacts/Create
        ///     {
        ///         "email": "email@email.com",
        ///         "firstName": "string",
        ///         "lastName": "string",
        ///         "accountName": "string" Can Be NULL
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns a json of created Contact</response>
        /// <response code="400">If contact with Email already exists or request body is null or Email is isvalid</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] NewContactModel contact)
        {
            if (contact == null)
            {
                return BadRequest("Enter Contact");
            }

            if(await _context.Contacts.AnyAsync(c => c.Email == contact.Email ))
            {
                return BadRequest("Contact with same email exist");
            }

            try
            {
                Contact newContact = await CreateContactFromNewContactModel(contact);
                await _context.Contacts.AddAsync(newContact);
                await _context.SaveChangesAsync();
                return Ok(newContact);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        /// <summary>
        /// Edit an existing Contact.
        /// </summary>
        /// <returns>A json of edited Contact</returns>
        /// <param name="ContactEmail"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/contacts/Edit/:ContactEmail
        ///     {
        ///         "email": ContactEmail,
        ///         "firstName": "string",
        ///         "lastName": "string",
        ///         "accountName": "string" Can Be NULL
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns a json of edited Contact</response>
        /// <response code="400">Errors of editing</response>
        /// <response code="404">If contact with Email doesn't exist or request body is null or
        ///     ContactEmail isn't equal to email field in request body
        /// </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(string ContactEmail, [FromBody] NewContactModel contact)
        {
            if (contact == null || ContactEmail != contact.Email ||
                !(await _context.Contacts.AnyAsync(e => e.Email == ContactEmail)))
            {
                return NotFound();
            }

            try
            {
                Contact editContact = await CreateContactFromNewContactModel(contact);
                _context.Update(editContact);
                await _context.SaveChangesAsync();
                return Ok(contact);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }            
        }

        /// <summary>
        /// Deletes a specific Contact item.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     DELETE: api/contacts/Delete/:contactEmail
        /// </remarks>
        /// <param name="contactEmail"></param>
        /// <returns></returns>
        /// <response code="200">Signalize that contact deleted</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteConfirmed(string contactEmail)
        {
            var contact = await _context.Contacts.FindAsync(contactEmail);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<Contact> CreateContactFromNewContactModel(NewContactModel contact) {
            if (!ValidationService.IsValidEmail(contact.Email))
            {
                throw new ArgumentException("Invalid email format");
            }

            Contact newContact = new Contact()
            {
                Email = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
            };

            if (contact.AccountName != null)
            {
                if (await _context.Accounts.AnyAsync(a => a.Name == contact.AccountName))
                {
                    newContact.AccountName = contact.AccountName;
                }
                else
                {
                    throw new ArgumentException("Account with name = " + newContact.AccountName + " doesn't exist");
                }
            }

            return newContact;
        }
    }
}
