using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncedentsAPI.Data;
using IncedentsAPI.Models;

namespace IncedentsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get All Accounts.
        /// </summary>
        /// <returns>A json list of Accounts</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Accounts/GetAll
        ///
        /// </remarks>
        /// <response code="200">Returns a json list of Accounts</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _context.Accounts
                                    .Include(a => a.Contacts)
                                    .Include(a => a.Incident)
                                    .ToListAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Get specific Account.
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns>A json of specific Account</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Accounts/GetByName/:accountName
        ///
        /// </remarks>
        /// <response code="200">Returns a json of specific Account</response>
        /// <response code="404">If account with accountName not found or accountName is null</response>
        [HttpGet("{accountName}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByName(string accountName)
        {
            if (accountName == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .Include(a => a.Contacts)
                .Include(a => a.Incident)
                .FirstOrDefaultAsync(m => m.Name == accountName);
            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        /// <summary>
        /// Create a new Account.
        /// </summary>
        /// <returns>A json of created Account</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Accounts/Create
        ///     {
        ///         "name": "string",
        ///         "contactEmail": "email@email.com",
        ///         "incedentName": "string" Can be Null
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns a json of created Account</response>
        /// <response code="400">If request body is null</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] NewAccountModel account)
        {
            if (account == null)
            {
                return BadRequest();
            }

            try
            {
                var newData = await CreateAccountFromNewAccountModel(account);
                await _context.Accounts.AddAsync(newData.Item1);
                _context.Update(newData.Item2);
                await _context.SaveChangesAsync();
                return Ok(newData.Item1);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Edit an existing Account.
        /// </summary>
        /// <returns>A json of edited Account</returns>
        /// <param name="AccountName"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Accounts/Edit/:AccountName
        ///     {
        ///         "name": "string",
        ///         "contactEmail": "email@email.com",
        ///         "incedentName": "string" Can be Null
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Returns a json of edited Account</response>
        /// <response code="400">Errors of editing</response>
        /// <response code="404">If account with Email doesn't exist or request body is null or 
        ///     AccountName isn't equal to name field in request body
        /// </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(string AccountName, [FromBody] NewAccountModel account)
        {
            if (account == null || AccountName != account.Name ||
                !(await _context.Accounts.AnyAsync(e => e.Name == AccountName)))
            {
                return NotFound();
            }

            try
            {
                var editElem = await CreateAccountFromNewAccountModel(account);
                _context.Update(editElem.Item1);
                _context.Update(editElem.Item2);
                await _context.SaveChangesAsync();
                return Ok(editElem.Item1);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a specific Account item.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     DELETE: api/Accounts/Delete/:AccountName
        /// </remarks>
        /// <param name="AccountName"></param>
        /// <returns></returns>
        /// <response code="200">Signalize that account deleted</response>
        [HttpDelete]
        public async Task<IActionResult> DeleteConfirmed(string AccountName)
        {
            var account = await _context.Accounts.FindAsync(AccountName);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<(Account,Contact)> CreateAccountFromNewAccountModel(NewAccountModel account)
        {
            if (account == null || account.ContactEmail == null)
            {
                throw new ArgumentException("Can't create Account");
            }
            Account newAccount = new Account()
            {
                Name = account.Name,
                Contacts = new List<Contact>()
            };

            var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == account.ContactEmail);

            if (contact == null) 
            {
                throw new ArgumentException("Contact Not Found");
            }

            contact.AccountName = account.Name;
            newAccount.Contacts.Add(contact);

            if (account.IncedentName != null)
            {
                if (await _context.Incedents.AnyAsync(i => i.Name == account.IncedentName))
                {
                    newAccount.IncedentName = account.IncedentName;
                }
                else
                {
                    throw new ArgumentException("Incedent with name = " + account.IncedentName + " doesn't exist");
                }                
            }
            return (newAccount, contact);
        }
    }
}
