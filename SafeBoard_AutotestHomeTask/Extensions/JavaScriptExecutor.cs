using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace SafeBoard_AutotestHomeTask.Extensions
{
	//comfortable feature to execute js
	public static class JavaScriptExecutor
	{
		public static IJavaScriptExecutor Scripts(this IWebDriver driver)
		{
			return (IJavaScriptExecutor)driver;
		}
	}
}
