using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartsUnlimited.Models;
using Newtonsoft.Json;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PartsUnlimited.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HavokController : ControllerBase
    {

        private readonly HavokContext _context;



        public HavokController(HavokContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Havok>>> GetHavok()
        {
            return await _context.Havoks.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Havok>> GetHavok(long id)
        {
            var havok = await _context.Havoks.FindAsync(id);

            if (havok == null)
            {
                return NotFound();
            }

            return havok;
        }
        // PUT: api/Todo/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHavokItem(long id, Havok item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        } 
     
   
        
    }
}
