using Microsoft.AspNetCore.Identity;

namespace Neodenit.Memento.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Skype { get; set; }
    }
}