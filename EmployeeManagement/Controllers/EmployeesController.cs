using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Models;
using EmployeeManagement.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<IActionResult> Index(
     string sortOrder,
     string nameSearch,
     string emailSearch,
     string mobileSearch,
     DateTime? dobSearch,
     int? pageNumber)
    {
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
        ViewData["MobileSortParm"] = sortOrder == "Mobile" ? "mobile_desc" : "Mobile";
        ViewData["DateOfBirthSortParm"] = sortOrder == "DateOfBirth" ? "dob_desc" : "DateOfBirth";
        ViewData["CurrentNameFilter"] = nameSearch;
        ViewData["CurrentEmailFilter"] = emailSearch;
        ViewData["CurrentMobileFilter"] = mobileSearch;
        ViewData["CurrentDobFilter"] = dobSearch?.ToString("yyyy-MM-dd");

        var employees = from e in _context.Employees select e;

        // Case-insensitive full name search
        if (!string.IsNullOrEmpty(nameSearch))
        {
            var lowerCaseNameSearch = nameSearch.ToLower();
            employees = employees.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(lowerCaseNameSearch));
        }
        if (!string.IsNullOrEmpty(emailSearch))
        {
            var lowerCaseEmailSearch = emailSearch.ToLower();
            employees = employees.Where(e => e.Email.ToLower().Contains(lowerCaseEmailSearch));
        }
        if (!string.IsNullOrEmpty(mobileSearch))
        {
            var lowerCaseMobileSearch = mobileSearch.ToLower();
            employees = employees.Where(e => e.Mobile.ToLower().Contains(lowerCaseMobileSearch));
        }
        if (dobSearch.HasValue)
        {
            var dob = dobSearch.Value.Date;
            employees = employees.Where(e => e.DateOfBirth.Date == dob);
        }

        // Sorting functionality
        employees = sortOrder switch
        {
            "name_desc" => employees.OrderByDescending(e => e.FirstName),
            "Email" => employees.OrderBy(e => e.Email),
            "email_desc" => employees.OrderByDescending(e => e.Email),
            "Mobile" => employees.OrderBy(e => e.Mobile),
            "mobile_desc" => employees.OrderByDescending(e => e.Mobile),
            "DateOfBirth" => employees.OrderBy(e => e.DateOfBirth),
            "dob_desc" => employees.OrderByDescending(e => e.DateOfBirth),
            _ => employees.OrderBy(e => e.FirstName),
        };

        // Pagination functionality
        int pageSize = 10;
        var paginatedEmployees = await PaginatedList<Employee>.CreateAsync(employees.AsNoTracking(), pageNumber ?? 1, pageSize);

        // For debugging: Check the count of employees returned
        Console.WriteLine($"Number of employees found: {paginatedEmployees.Count}");

        return View(paginatedEmployees);
    }





    [HttpGet]
    public IActionResult Create()
    {
        return View(); 
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Mobile,DateOfBirth")] Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data." });
        }

        employee.DateOfBirth = employee.DateOfBirth.ToUniversalTime();

        _context.Add(employee);
        await _context.SaveChangesAsync();

        
        return Json(new { success = true, message = "Employee successfully created!" });
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

        employee.DateOfBirth = employee.DateOfBirth.ToLocalTime();

        return View(employee); 
    }

    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Invalid data." });
        }

        try
        {
            employee.DateOfBirth = employee.DateOfBirth.ToUniversalTime(); 

            _context.Update(employee);
            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected > 0)
            {
             
                return Json(new { success = true, message = "Employee successfully edited!" });
            }
            else
            {
                
                return Json(new { success = false, message = "No changes were made." });
            }
        }
        catch (DbUpdateConcurrencyException)
        {
           
            return Json(new { success = false, message = "Failed to edit employee due to concurrency issues." });
        }
        catch (Exception ex)
        {
         
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    
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

        return View(employee); 
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index)); 
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}
