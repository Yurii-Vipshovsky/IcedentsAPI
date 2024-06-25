using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IncedentsAPI.Data;
using IncedentsAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IncedentsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contacts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _context.Contacts.Include(c => c.Account).ToListAsync();
            return Ok(contacts);
        }

        // GET: Contacts/Details/5
        [HttpGet("{contactEmail}")]
        public async Task<IActionResult> GetByName(string contactEmail)
        {
            if (contactEmail == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.Account)
                .FirstOrDefaultAsync(m => m.Email == contactEmail);
            if (contact == null)
            {
                return NotFound();
            }

            return Ok(contact);
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NewContactModel contact)
        {
            if (contact == null)
            {
                return BadRequest();
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

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
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

        // POST: Contacts/Delete/5
        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string contactEmail)
        {
            var contact = await _context.Contacts.FindAsync(contactEmail);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public class NewContactModel
        {
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string? AccountName { get; set; }
        }

        private async Task<Contact> CreateContactFromNewContactModel(NewContactModel contact) {
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
