using SkiControl.Data;
using SkiControl.Models;

namespace SkiControl.Services;

public class CompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public MyCompany GetDetails()
    {
        var company = _context.MojaFirma.FirstOrDefault();
        if (company == null)
        {
            company = new MyCompany();
            _context.MojaFirma.Add(company);
            _context.SaveChanges();
        }
        return company;
    }

    public void SaveDetails(MyCompany company)
    {
        _context.MojaFirma.Update(company);
        _context.SaveChanges();
    }
}