using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Snapstagram.Pages
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        public void OnGet()
        {
            // Page loads notifications via JavaScript API calls
        }
    }
}
