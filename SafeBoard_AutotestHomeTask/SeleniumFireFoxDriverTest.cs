using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System.Threading;
using Newtonsoft.Json;
using System.Runtime.InteropServices;




namespace SafeBoard_AutotestHomeTask
{
	
	//comfortable feature to execute js
	public static class JavaScriptExecutor
	{
		public static IJavaScriptExecutor Scripts(this IWebDriver driver)
		{
			return (IJavaScriptExecutor)driver;
		}
	}


	//[Ignore]
	[TestClass]
	public class SeleniumFireFoxDriverTest
	{
		[DllImport("libc", SetLastError = true)]
		private static extern int сhmod(string pathname, int mode);

		public static string FireFoxDriversDirectory = "../../../../SeleniumFireFoxDrivers/";

		public static bool IsUnderLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}

		public class VkCredentials
		{
			public VkCredentials()
			{
				this.Login = "<enter_login>";
				this.Password = "<enter_password>";
			}
			public string Login;
			public string Password;
		}
		public static VkCredentials _VkCredentials;

		[ClassInitialize]
		public static void Init(TestContext context)
		{

			string settingsFile = "./VkCredentials.json";
			if (!File.Exists(settingsFile))
			{
				File.WriteAllText(settingsFile, JsonConvert.SerializeObject(new VkCredentials()));
				throw new FileNotFoundException("Файл настроек не найден, однако был создан файл настроек по-умолчанию в папке с программой. Заполните его параметры.");
			}
			_VkCredentials = JsonConvert.DeserializeObject<VkCredentials>(File.ReadAllText(settingsFile));
			
			if (IsUnderLinux)
			{
				Chmod(FireFoxDriversDirectory + "geckodriver", 777);
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


		

		//[Ignore]
		[TestMethod]
		//авторизоватся и оставить пост на своей стене
		public void VK_LoginAndMakePost_ExceptionShouldNotOccur()
		{
			using (IWebDriver driver = new FirefoxDriver(FireFoxDriversDirectory))
			{
				FixDriverCommandExecutionDelay(driver);
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

				driver.Url = "https://vk.com/";

				driver.FindElement(By.Id("index_email")).SendKeys(_VkCredentials.Login);//вставляем логин
				driver.FindElement(By.Id("index_pass")).SendKeys(_VkCredentials.Password);//вставляем пароль
				driver.FindElement(By.Id("index_login_button")).Click();//жмем кнопку входа
				driver.FindElement(By.Id("l_pr")).Click();//открываем страницу нашего профиля
				

				var input = driver.FindElement(By.Id("post_field"));
				
				driver.Scripts().ExecuteScript("arguments[0].scrollIntoView()", input);
				input.SendKeys("умею быстро придумывать числа: " + new Random().Next(100000,9999999).ToString());//вводим текст в поле нового поста

				//var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
				//wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("post_field"))).SendKeys("hello from KasperskyLab!");//вводим текст в поле нового поста
				//driver.FindElement(By.Id("send_post")).Click();//отправляем пост на стену
				//wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("send_post"))).Click();

				var button = driver.FindElement(By.Id("send_post"));
				driver.Scripts().ExecuteScript("arguments[0].click();", button);
				//button.Click();


				//отправляем пост на стену


				Thread.Sleep(5000);
				driver.Close();
			}
		}

		//[Ignore]
		[TestMethod]
		//авторизоваться, найти страницу Евгения, открыть его аватарку, поставить лайк и оставить комментарий
		public void VK_LoginThenLikeAndCommentEugene_ExceptionShouldNotOccur()
		{
			using (var driver = new FirefoxDriver(FireFoxDriversDirectory))
			{
				FixDriverCommandExecutionDelay(driver);
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

				driver.Url = "https://vk.com/";

				driver.FindElement(By.Id("index_email")).SendKeys(_VkCredentials.Login);
				driver.FindElement(By.Id("index_pass")).SendKeys(_VkCredentials.Password);
				driver.FindElement(By.Id("index_login_button")).Click();
				driver.FindElement(By.Id("l_fr")).Click();//открываем страницу с друзьями
				Thread.Sleep(500);//фоновые скрипты должны прогрузиться, в противном случае текстовое поле не реагирует на изменения
				driver.FindElement(By.Id("s_search")).SendKeys("Евгений Касперский");//вводим Евгения в поиск
				driver.FindElement(By.Id("s_search")).SendKeys(Keys.Enter);//подтверждаем.
				driver.FindElement(By.XPath("//*[@id=\"friends_search_cont\"]/div[1]/div[1]/div[3]/div[1]/a")).Click();//жмем на первого человека в списке
				driver.FindElement(By.XPath("//*[@id=\"profile_photo_link\"]/img")).Click();//открываем аватарку Евгения
				driver.FindElement(By.XPath("//*[@id=\"pv_narrow\"]/div[1]/div[1]/div/div/div[1]/div[3]/div/div[1]/a[1]/div[1]")).Click();//ставим лайк на аватарку Евгению
				driver.FindElement(By.XPath("//*[starts-with(@id,'reply_field')]")).SendKeys("отличное фото");//набираем ему комментарий под фото
				driver.FindElement(By.ClassName("addpost_button_wrap")).Click();//отправляем комментарий Евгению


				System.Threading.Thread.Sleep(5000);
				driver.Close();
			}
		}

		//[Ignore]
		[TestMethod]
		//авторизоваться, открыть первого друга в списке друзей, открыть аватарку, поставить лайк, оставить комментарий
		public void VK_LoginThenLikeAndCommentFirstFriend_ExceptionShouldNotOccur()
		{
			using (var driver = new FirefoxDriver(FireFoxDriversDirectory))
			{
				FixDriverCommandExecutionDelay(driver);
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

				driver.Url = "https://vk.com/";

				driver.FindElement(By.Id("index_email")).SendKeys(_VkCredentials.Login);
				driver.FindElement(By.Id("index_pass")).SendKeys(_VkCredentials.Password);
				driver.FindElement(By.Id("index_login_button")).Click();
				driver.FindElement(By.Id("l_fr")).Click();//открываем страницу с друзьями
				driver.FindElement(By.XPath("//*[@id=\"list_content\"]/div[1]/div[1]/div[4]/div[1]/a")).Click();//жмем на первого человека в списке
				driver.FindElement(By.XPath("//*[@id=\"profile_photo_link\"]/img")).Click();//открываем аватарку пользователя
				driver.FindElement(By.XPath("//*[@id=\"pv_narrow\"]/div[1]/div[1]/div/div/div[1]/div[3]/div/div[1]/a[1]/div[1]")).Click();//ставим лайк на аватарку
				driver.FindElement(By.XPath("//*[starts-with(@id,'reply_field')]")).SendKeys("спасииибоооо");//набираем ему комментарий под фото
				driver.FindElement(By.ClassName("addpost_button_wrap")).Click();//отправляем комментарий 


				System.Threading.Thread.Sleep(5000);
				driver.Close();
			}
		}

		//[Ignore]
		[TestMethod]
		//авторизоваться, зайти в первую группу, зайти к случайному участнику, заблокировать его, и разблокировать
		public void VK_LoginThenBlockAndUnlockUserFromFirstGroup_ExceptionShouldNotOccur()
		{
			using (var driver = new FirefoxDriver(FireFoxDriversDirectory))
			{
				FixDriverCommandExecutionDelay(driver);
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

				driver.Url = "https://vk.com/";

				driver.FindElement(By.Id("index_email")).SendKeys(_VkCredentials.Login);
				driver.FindElement(By.Id("index_pass")).SendKeys(_VkCredentials.Password);
				driver.FindElement(By.Id("index_login_button")).Click();
				driver.FindElement(By.Id("l_gr")).Click();//открываем страницу с группами
				driver.FindElement(By.XPath("//*[@id=\"groups_list_groups\"]/div[1]/a")).Click();//жмем на первую группу в списке
				driver.FindElement(By.XPath("//*[contains(@id,'_followers')]/a/div")).Click();//открываем список участников
				driver.FindElement(By.XPath("//*[@id=\"fans_rowsmembers\"]/div[1]/div[1]")).Click();//жмем на первого участника
				var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
				wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"narrow_column\"]/div[1]/aside/div/div[2]/div[2]/div[1]"))).Click();//раскрываем меню дополнительных функций у пользователя
				//Thread.Sleep(1000);
				wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"narrow_column\"]/div[1]/aside/div/div[2]/div[2]/div[2]/div[2]/a[4]"))).Click();//кликаем на пункт о блокировке пользователя
				Thread.Sleep(2000);
				wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id=\"narrow_column\"]/div[1]/aside/div/div[2]/div[2]/div[2]/div[2]/a[4]"))).Click();//кликаем на пункт о разблокировке пользователя

				Thread.Sleep(5000);
				driver.Close();
			}
		}

	}
}
