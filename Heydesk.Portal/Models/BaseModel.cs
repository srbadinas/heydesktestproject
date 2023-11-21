using Heydesk.Entities.Models;

namespace Heydesk.Portal.Models
{
    public class BaseModel
    {
        public User LoggedInUser { get; set; } = new User();
        public string Message { get; set; }
    }
}
