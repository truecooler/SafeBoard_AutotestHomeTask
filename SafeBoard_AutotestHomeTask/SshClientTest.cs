using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Newtonsoft.Json;



using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;

namespace SafeBoard_AutotestHomeTask
{
	//[Ignore]
	[TestClass]
	public class SshClientTest
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
		public static SshCredentials _SshCredentials;

		static int RandomNumber;
		[ClassInitialize]
		public static void Init(TestContext context)
		{
			string settingsFile = "./SshCredentials.json";
			if (!File.Exists(settingsFile))
			{
				File.WriteAllText(settingsFile, JsonConvert.SerializeObject(new SshCredentials()));
				throw new FileNotFoundException("Файл настроек не найден, однако был создан файл настроек по-умолчанию в папке с программой. Заполните его параметры.");
			}
			_SshCredentials = JsonConvert.DeserializeObject<SshCredentials>(File.ReadAllText(settingsFile));
			RandomNumber = new Random().Next(10000, 99999);
		}

		[DataTestMethod]
		[DataRow("SomeKasperskyFile.txt")]
		public void SshClient_CreateFile_FileShouldBeCreated(string filename)
		{
			using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SshClient.Connect();
				using (var cmd = SshClient.CreateCommand($"touch {filename} && ls -al && rm {filename}"))
				{
					cmd.Execute();
					string cmdOutput = cmd.Result;
					SshClient.Disconnect();
					Assert.IsTrue(cmdOutput.Contains(filename));
				}
			}

		}

		//no idea how to optimize this
		static public IEnumerable<object[]> GetFilenames()
		{
			yield return new object[] { "HiFromKaspersky.txt" };
			yield return new object[] { $"file_{RandomNumber}.txt" };
			yield return new object[] { $"file_ololo_{RandomNumber}.txt" };
			yield return new object[] { $"file_test_{RandomNumber}.txt" };
			yield return new object[] { $"file_test_{RandomNumber}_{RandomNumber}.txt" };
		}



		[DataTestMethod]
		[DynamicData(nameof(GetFilenames), DynamicDataSourceType.Method)]
		public void SshClient_CreateAndDeleteFile_FileShouldBeDeleted(string filename)
		{
			using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SshClient.Connect();
				using (var cmd = SshClient.CreateCommand($"touch {filename} && rm {filename} && ls"))
				{
					cmd.Execute();
					string cmdOutput = cmd.Result;
					SshClient.Disconnect();
					Assert.IsFalse(cmdOutput.Contains(filename));
				}
			}

		}


		[DataTestMethod]
		[DataRow("kaspersky_test.txt")]
		public void SshClient_WriteTextToFileAndReadIt_WritedTextShouldBeSameAsReadText(string filename)
		{
			using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SshClient.Connect();
				string textToWrite = $"KasperskyLab_LovesYou_Rand:{RandomNumber}";
				using (var cmd = SshClient.CreateCommand($"echo '{textToWrite}' > ./{filename} && cat ./{filename} && rm {filename}"))
				{
					cmd.Execute();
					string cmdOutput = cmd.Result;
					SshClient.Disconnect();
					StringAssert.Equals(cmdOutput, textToWrite + "\n");
				}
			}
		}


		[TestMethod]
		public void SshClient_MakePingToAliveHost_HostShouldSendResponseToUs()
		{
			using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SshClient.Connect();
				using (var cmd = SshClient.CreateCommand($"ping 1.1.1.1 -c 4"))
				{
					cmd.Execute();
					string cmdOutput = cmd.Result;
					SshClient.Disconnect();
					Assert.IsTrue(cmdOutput.Contains("0% packet loss"));
				}
			}
		}

		//[Ignore]
		[TestMethod]
		public void SshClient_MakePingToDeadHost_HostShouldNotSendResponseToUs()
		{
			using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SshClient.Connect();
				using (var cmd = SshClient.CreateCommand($"ping 2.0.1.8 -c 1"))
				{
					cmd.Execute();
					string cmdOutput = cmd.Result;
					SshClient.Disconnect();
					Assert.IsTrue(cmdOutput.Contains("100% packet loss"));
				}
			}
		}


		[TestMethod]
		public void SftpClient_SedReplaceWordVirusToKasperskyInFile_FileShouldContain3KasperskyWords()
		{
			using (var SftpClient = new SftpClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
				{
					int expectedKasperskyWordsCount = 3;
					SftpClient.Connect();
					using (Stream fileStream = File.OpenRead("../../../../TestDataSets/VirusToKaspersky.txt"))
					{
						SftpClient.UploadFile(fileStream, "./VirusToKaspersky.txt", canOverride: true);
					}

					SshClient.Connect();
					using (var cmd = SshClient.CreateCommand($"cat VirusToKaspersky.txt | sed 's/virus/kaspersky/g' | grep -c 'kaspersky'"))
					{
						cmd.Execute();
						string cmdOutput = cmd.Result;
						SftpClient.DeleteFile("./VirusToKaspersky.txt");
						SftpClient.Disconnect();
						SshClient.Disconnect();
						Assert.AreEqual(expectedKasperskyWordsCount, Convert.ToInt32(cmdOutput));
					}
				}
			}
		}


		[TestMethod]
		public void SftpClient_SedDeleteCommentsAndEmptyLinesInFile_FileShouldNotContainCommentsAndEmptyLines()
		{
			using (var SftpClient = new SftpClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
				{
					int expectedCommentsAndEmptryLinesCount = 0;
					SftpClient.Connect();
					using (Stream fileStream = File.OpenRead("../../../../TestDataSets/FileWithCode.txt"))
					{
						SftpClient.UploadFile(fileStream, "./FileWithCode.txt", canOverride: true);
					}

					SshClient.Connect();
					using (var cmd = SshClient.CreateCommand($"cat FileWithCode.txt | sed '/^#[^!].*/d; /^$/d' | grep -c -E '(^#[^!].*|^$)'"))
					{
						cmd.Execute();
						string cmdOutput = cmd.Result;
						SftpClient.DeleteFile("./FileWithCode.txt");
						SftpClient.Disconnect();
						SshClient.Disconnect();
						Assert.AreEqual(expectedCommentsAndEmptryLinesCount, Convert.ToInt32(cmdOutput));
					}
				}
			}
		}


		[TestMethod]
		public void SftpClient_AwkTableGetRowsCount_RowsCountShouldBe8()
		{
			using (var SftpClient = new SftpClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
				{
					SftpClient.Connect();
					using (Stream fileStream = File.OpenRead("../../../../TestDataSets/DataTable.txt"))
					{
						SftpClient.UploadFile(fileStream, "./DataTable.txt", canOverride: true);
					}

					SshClient.Connect();
					using (var cmd = SshClient.CreateCommand("cat DataTable.txt | awk '/a/{++cnt} END {print \"Count = \", cnt}'"))
					{
						cmd.Execute();
						string cmdOutput = cmd.Result;
						SftpClient.DeleteFile("./DataTable.txt");
						SftpClient.Disconnect();
						SshClient.Disconnect();
						StringAssert.Equals("Count = 8\n", cmdOutput);
					}
				}
			}
		}


		[TestMethod]
		public void SftpClient_AwkTableFilterManagers_ResultTableShouldContainAllManagers()
		{
			using (var SftpClient = new SftpClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				using (var SshClient = new SshClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
				{
					int expectedTotalManagersCount = 3;
					SftpClient.Connect();
					using (Stream fileStream = File.OpenRead("../../../../TestDataSets/DataTable.txt"))
					{
						SftpClient.UploadFile(fileStream, "./DataTable.txt", canOverride: true);
					}

					SshClient.Connect();
					using (var cmd = SshClient.CreateCommand("cat DataTable.txt | awk '/manager/ {print}' | grep -c 'manager'"))
					{
						cmd.Execute();
						string cmdOutput = cmd.Result;
						SftpClient.DeleteFile("./DataTable.txt");
						SftpClient.Disconnect();
						SshClient.Disconnect();
						Assert.AreEqual(expectedTotalManagersCount, Convert.ToInt32(cmdOutput));
					}
				}
			}
		}

		[TestMethod]
		public void SftpClient_SendFile_FileShouldAppearInRemoteFileSystem()
		{
			using (var SftpClient = new SftpClient(_SshCredentials.Host, _SshCredentials.Login, _SshCredentials.Password))
			{
				SftpClient.Connect();
				using (Stream fileStream = File.OpenRead("../../../../TestDataSets/sftp_test.txt"))
				{
					SftpClient.UploadFile(fileStream, "./sftp_test.txt", canOverride: true);
				}
				var files = SftpClient.ListDirectory(".");
				SftpClient.DeleteFile("./sftp_test.txt");
				SftpClient.Disconnect();
				Assert.IsTrue(files.Any(x => x.Name == "sftp_test.txt"));
			}
		}




		[ClassCleanup]
		public static void Cleanup()
		{
			//SshClient.Dispose();
			//SftpClient.Dispose();
		}
	}
}
