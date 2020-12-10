using DBLib.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthorizeErrorEFCoreSample.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly NorthwindContext db;

        public EmployeesController(
            NorthwindContext _DBContext
            )
        {
            this.db = _DBContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await db.Employees.ToListAsync());
        }
    }
}