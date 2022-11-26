using System;
namespace UsersClassLibrary.Models
{
	public class UserOutModule
	{
		public string Username { get; set; }
        public int IdUser { get; set; }

        public UserOutModule(User user)
		{
			this.Username = user.Username;
			this.IdUser = user.IdUser;
		}
	}
}

