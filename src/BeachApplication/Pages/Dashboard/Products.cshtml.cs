using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeachApplication.Pages.Dashboard;

[Authorize(Policy = "UserActive")]
public class ProductsModel : PageModel
{
    public void OnGet()
    {
    }
}