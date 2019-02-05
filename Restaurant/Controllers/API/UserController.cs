using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Restaurant.Controllers.API
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;

        }

        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get(string type,string query=null)
        {
            if (type.Equals("email") && query != null)
            {
                var customerQuery = _context.Users.Where(u => u.Email.ToLower().Contains(query.ToLower()));
                return Ok(customerQuery.ToList());
            }

            return Ok();
        }

        
    }
}
