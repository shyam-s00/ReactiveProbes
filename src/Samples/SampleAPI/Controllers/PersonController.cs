using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactiveProbes.Models;
// If your PersonContext is in a different namespace, update the using below:
using SampleAPI.Context;
using SampleAPI.Entities;

namespace ReactiveProbes.Controllers
{
    // ...existing code...
    [ApiController]
    [Route("api/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly PersonContext _context;

        public PersonController(PersonContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonDto>>> GetAll()
        {
            var persons = await _context.People
                .AsQueryable()
                .Include(p => p.PersonPhones)
                .Include(p => p.EmailAddresses)
                .Select(p => new PersonDto
                    {
                        PersonId = p.BusinessEntityId,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        PhoneNumber = p.PersonPhones.FirstOrDefault()!.PhoneNumber,
                        Email = p.EmailAddresses.FirstOrDefault()!.EmailAddress1!
                    })
                .Take(10)
                .ToListAsync();

            return Ok(persons);
        }
        

    }
    
    // ...existing code...
}
