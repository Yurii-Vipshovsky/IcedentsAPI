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
    public class IncidentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IncidentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Incidents
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

        // GET: Incidents/Details/5
        [HttpGet("{incedentName}")]
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

        // POST: Incidents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] RequestBody incident)
        {
            if (incident == null)
            {
                return BadRequest();
            }

            //var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Name == incident.AccountName);

            //if (account == null)
            //{
            //    return NotFound();
            //}

            //var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Email == incident.ContactEmail);
            //if (contact != null)
            //{
            //    bool isSame = true;
            //    if(contact.FirstName != incident.ContactFirstName)
            //    {
            //        isSame = false;
            //        contact.FirstName = incident.ContactFirstName;
            //    }
            //    if(contact.LastName != incident.ContactLastName)
            //    {
            //        isSame = false;
            //        contact.LastName = incident.ContactLastName;
            //    }
            //    if(contact.AccountName != incident.AccountName)
            //    {
            //        isSame = false;
            //        contact.AccountName = incident.AccountName;
            //        contact.Account = account;
            //    }
            //    if (!isSame)
            //    {
            //        _context.Update(contact);
            //    }
            //}
            //else
            //{
            //    contact = new Contact()
            //    {
            //        FirstName = incident.ContactFirstName,
            //        LastName = incident.ContactLastName,
            //        Email = incident.ContactEmail,
            //        AccountName = incident.AccountName,
            //        Account = account
            //    };
            //    await _context.Contacts.AddAsync(contact);
            //}

            //var newIncedent = new Incident()
            //{
            //    Description = incident.IncidentDescription,
            //};

            //account.Incident = newIncedent;

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

        // POST: Incidents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string incidentName, [FromBody] RequestBody incident)
        {
            var oldIncedent = await _context.Incedents.FirstOrDefaultAsync(i => i.Name == incidentName);
            if (oldIncedent == null)
            {
                return NotFound();
            }
            var account = await UpdateContactFromRequestBody(incident);
            if(oldIncedent.Description != incident.IncidentDescription)
            {
                oldIncedent.Description = incident.IncidentDescription;
                _context.Update(oldIncedent);
            }

            account.IncedentName = incidentName;
            
            _context.Update(account);
            await _context.SaveChangesAsync();

            return Ok(oldIncedent);
        }

        // POST: Incidents/Delete/5
        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string incedentName)
        {
            var incident = await _context.Incedents.FindAsync(incedentName);
            if (incident != null)
            {
                _context.Incedents.Remove(incident);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public class RequestBody
        {
            public string AccountName { get; set; }
            public string ContactFirstName { get; set; }
            public string ContactLastName { get; set; }
            public string ContactEmail { get; set; } // unique identifier,
            public string IncidentDescription { get; set; }
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

        private bool IncidentExists(string id)
        {
            return _context.Incedents.Any(e => e.Name == id);
        }
    }
}
