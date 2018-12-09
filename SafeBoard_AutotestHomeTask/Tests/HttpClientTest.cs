using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Net;

using System.Collections.Generic;


namespace SafeBoard_AutotestHomeTask
{
	//[Ignore]
	[TestClass]
	public class HttpClientTest
	{
		public static HttpClient HttpClient;

		[ClassInitialize]
		public static void Init(TestContext context)
		{

			var handler = new HttpClientHandler()
			{
				AllowAutoRedirect = false
				
			};

			HttpClient = new HttpClient(handler);
		}



		[DataTestMethod]
		[DataRow("admin","admin",false)]
		[DataRow("korolev", "22m", true)]
		[DataRow("user", "pass", false)]
		[DataRow("111111111111", "222222222", false)]
		[DataRow("cplusplus", "compile", false)]
		[DataRow("software", "testing", false)]
		[DataRow("wannalogin", "plz", false)]
		[DataRow("this_is_valid_user", "somepass", true)]
		public void HttpPost_TryToAuthorizeAndCheckCredentials_LoginShouldBeSuccessfullDependsOfFlag(string login, string pass, bool isValid)
		{
			var values = new Dictionary<string, string>
			{
			   { "username", login },
			   { "password", pass }
			};

			var content = new FormUrlEncodedContent(values);

			var response = HttpClient.PostAsync("http://pk13.ru/lms/login/index.php", content).Result;

			/* HttpStatusCode.SeeOther if login successfull, HttpStatusCode.OK if not */

			HttpStatusCode expectedHttpStatusCode = isValid ? HttpStatusCode.SeeOther : HttpStatusCode.OK;

			Assert.AreEqual(expectedHttpStatusCode,response.StatusCode);
		}



		//Проверка соответствия установленного user-agent и возвращаемого user-agent из интернета
		[DataTestMethod]
		[DataRow("KasperskyLab_SuperUserAgent")]
		[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36")]
		[DataRow("MIIEzTCCA7WgAwIBAgIJAMhHag4WTuotMA0GCSqGSIb3DQEBCwUAMIGfMQswCQYD")]
		public void UserAgent_AssignAndSendRequest_ShouldBeSameAsAssigned(string useragentToTest)
		{
			HttpClient.DefaultRequestHeaders.UserAgent.Clear();
			HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(useragentToTest);
			var response = HttpClient.GetAsync("http://www.xhaus.com/headers").Result;
			var responseBody = response.Content.ReadAsStringAsync().Result;
			Assert.IsTrue(responseBody.Contains(useragentToTest));
		}


		//Проверка на фильтрацию некорректных user-agents хостингом
		[DataTestMethod]
		//[DataRow(user-agent,isCorrect)]
		[DataRow("KasperskyLab_SuperUserAgent", false)]
		[DataRow("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36", true)]
		[DataRow("Mozilla/5.0 (Linux; Android 7.0; SM-G892A Build/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/60.0.3112.107 Mobile Safari/537.36", true)]
		[DataRow("SuperBadUserAgent", false)]
		[DataRow("Mozilla/5.0 (Linux; Android 6.0; HTC One M9 Build/MRA58K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.98 Mobile Safari/537.3", true)]
		[DataRow("test", false)]
		[DataRow("Mozilla/5.0 (iPhone9,3; U; CPU iPhone OS 10_0_1 like Mac OS X) AppleWebKit/602.1.50 (KHTML, like Gecko) Version/10.0 Mobile/14A403 Safari/602.1", true)]
		[DataRow("FakeUserAgent", false)]
		[DataRow("11112222233333", false)]
		public void HostingUserAgentChecking_SendRequest_FakeShouldBeFiltered(string useragentToTest, bool isCorrect)
		{
			HttpClient.DefaultRequestHeaders.UserAgent.Clear();
			HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(useragentToTest);
			var response = HttpClient.GetAsync("http://thecooler.ru/some_resource.jpg").Result;
			Assert.IsTrue(response.IsSuccessStatusCode == isCorrect);
		}


		[ClassCleanup]
		public static void Cleanup()
		{
			HttpClient.Dispose();
		}
	}

	


	

}