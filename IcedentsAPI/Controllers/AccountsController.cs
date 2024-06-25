using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IncedentsAPI.Data;
using IncedentsAPI.Models;

namespace IncedentsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _context.Accounts
                                    .Include(a => a.Contacts)
                                    .Include(a => a.Incident)
                                    .ToListAsync();
            return Ok(accounts);
        }

        // GET: Accounts/Details/5
        [HttpGet("{accountName}")]
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

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
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

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
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

        // POST: Accounts/Delete/5
        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string AccountName)
        {
            var account = await _context.Accounts.FindAsync(AccountName);
            if (account != null)
            {
                _context.Accounts.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public class NewAccountModel
        {
            public string Name { get; set; }
            public string ContactEmail { get; set; }
            public string? IncedentName { get; set; }
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

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.Name == id);
        }
    }
}
