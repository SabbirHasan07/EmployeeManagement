using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;
using EmployeeManagement.Data;
using System.Linq;
using System.Threading.Tasks;

public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Employees (Index action for listing employees)
    public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
        ViewData["CurrentFilter"] = searchString;

        var employees = from e in _context.Employees select e;

        // Search functionality
        if (!string.IsNullOrEmpty(searchString))
        {
            employees = employees.Where(e => e.FirstName.Contains(searchString) || e.Email.Contains(searchString));
        }

        // Sorting functionality
        employees = sortOrder switch
        {
            "name_desc" => employees.OrderByDescending(e => e.FirstName),
            "Email" => employees.OrderBy(e => e.Email),
            "email_desc" => employees.OrderByDescending(e => e.Email),
            _ => employees.OrderBy(e => e.FirstName),
        };

        // Pagination functionality
        int pageSize = 10;
        return View(await PaginatedList<Employee>.CreateAsync(employees.AsNoTracking(), pageNumber ?? 1, pageSize));
    }

    // GET: Employees/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(); // This will look for a Create.cshtml view inside Views/Employees folder
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Mobile,DateOfBirth,Photo")] Employee employee)
    {
        if (ModelState.IsValid)
        {
            // Ensure DateOfBirth is in UTC
            employee.DateOfBirth = DateTime.SpecifyKind(employee.DateOfBirth, DateTimeKind.Utc);

            _context.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(employee);
    }

    // GET: Employees/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);  // This will return the Edit view (Edit.cshtml)
    }


    // GET: Employees/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee); // This will look for Delete.cshtml in Views/Employees folder
    }

    // POST: Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index)); // Redirect to employee list after successful delete
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Mobile,DateOfBirth,Photo")] Employee employee)
    {
        if (id != employee.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index)); 
        }
        return View(employee); 
    }



}
