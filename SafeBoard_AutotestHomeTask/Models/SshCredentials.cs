using System;
using System.Collections.Generic;
using System.Text;

namespace SafeBoard_AutotestHomeTask.Models
{
	public class SshCredentials
	{
		public SshCredentials()
		{
			this.Host = "<enter_host>";
			this.Login = "<enter_login>";
			this.Password = "<enter_password>";
		}
		public string Host;
		public string Login;
		public string Password;
	}
}
