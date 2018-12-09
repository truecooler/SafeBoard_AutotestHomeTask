using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SafeBoard_AutotestHomeTask.Utils
{
    public class Helper
    {
		public static bool IsUnderLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}


		//bug in .net core. details: https://stackoverflow.com/questions/46836472/selenium-with-net-core-performance-impact-multiple-threads-in-iwebelement
		public static void FixDriverCommandExecutionDelay(IWebDriver driver)
		{
			PropertyInfo commandExecutorProperty = typeof(RemoteWebDriver).GetProperty("CommandExecutor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
			ICommandExecutor commandExecutor = (ICommandExecutor)commandExecutorProperty.GetValue(driver);

			FieldInfo remoteServerUriField = commandExecutor.GetType().GetField("remoteServerUri", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField);

			if (remoteServerUriField == null)
			{
				FieldInfo internalExecutorField = commandExecutor.GetType().GetField("internalExecutor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
				commandExecutor = (ICommandExecutor)internalExecutorField.GetValue(commandExecutor);
				remoteServerUriField = commandExecutor.GetType().GetField("remoteServerUri", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField);
			}

			if (remoteServerUriField != null)
			{
				string remoteServerUri = remoteServerUriField.GetValue(commandExecutor).ToString();

				string localhostUriPrefix = "http://localhost";

				if (remoteServerUri.StartsWith(localhostUriPrefix))
				{
					remoteServerUri = remoteServerUri.Replace(localhostUriPrefix, "http://127.0.0.1");

					remoteServerUriField.SetValue(commandExecutor, new Uri(remoteServerUri));
				}
			}
		}

	}
}
