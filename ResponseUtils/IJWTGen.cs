using UsersClassLibrary.Models;

namespace ResponseUtils
{
    public interface IJWTGen
    {
        string generateUserToken(User user);
        string? getUserIdFromToken(string token);
    }
}