using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCode.Examples.UnitTests.Framework.Interfaces;
using SourceCode.Workflow.Client;
using System.Configuration;
using SourceCode.Data.SmartObjectsClient;
using System.Data;

namespace SourceCode.Examples.UnitTests.Framework
{
	public static class ProcessHelper
	{
		public static string ConnectionString
		{
			get { return ConfigurationManager.AppSettings["K2WorkflowServerConnectionString"]; }
		}

		public static string ConnectionStringManagementServer
		{
			get { return ConfigurationManager.AppSettings["K2ManagementServerConnectionString"]; }
		}

		public static string ConnectionStringImpersonation
		{
			get { return ConfigurationManager.AppSettings["K2WorkflowServerConnectionStringImpersonate"]; }
		}

		public static void StartProcess(IProcessTest testClass)
		{
			StartProcess(testClass, null, true);
		}

		public static string SOConnectionString
		{
			get { return ConfigurationManager.AppSettings["K2SOConnectionString"]; }
		}

		public static string WorkflowServer
		{
			get { return ConfigurationManager.AppSettings["K2WorkflowServer"]; }
		}

		public static int WorkflowServerPort
		{
			get
			{
				int port = 0;
				bool parsed = Int32.TryParse(ConfigurationManager.AppSettings["K2WorkflowServerPort"], out port);
				if (!parsed)
				{
					port = 5555;
				}
				return port;
			}
		}

		public static void StartProcess(IProcessTest testClass, Dictionary<string, string> dataFields)
		{
			StartProcess(testClass, dataFields, true);
		}

		public static void StartProcess(IProcessTest testClass, Dictionary<string, string> dataFields, bool sleep)
		{
			ConnectionSetup connSetup = new ConnectionSetup();
			connSetup.ConnectionString = ConnectionString;
			using (Connection conn = new Connection())
			{
				conn.Open(connSetup);
				ProcessInstance instanceToStart = conn.CreateProcessInstance(testClass.ProcessName);

				if (dataFields != null)
				{
					foreach (KeyValuePair<string, string> field in dataFields)
					{
						instanceToStart.DataFields[field.Key].Value = field.Value;
					}
				}

				instanceToStart.Folio = GetFolio(testClass);

				conn.StartProcessInstance(instanceToStart, true);

				testClass.TestScenarioContext.ProcessInstance = instanceToStart;
			}			
			
			if (sleep)
			{
				System.Threading.Thread.Sleep(2000);
			}
		}

		public static void StopProcess(IProcessTest testClass)
		{
			try
			{
				SourceCode.Workflow.Management.WorkflowManagementServer server = new SourceCode.Workflow.Management.WorkflowManagementServer();
				server.Open(ConnectionStringManagementServer);
				server.StopProcessInstances(Convert.ToInt32(testClass.TestScenarioContext.ProcessId));
			}
			catch { } // ignore, don't want to fail the test if the process can't be stopped
		}

		/// <summary>
		/// Gets worklist items for the current running user
		/// </summary>
		/// <param name="testClass"></param>
		/// <returns></returns>
		public static Worklist GetWorklist(IProcessTest testClass)
		{
			return GetWorklist(testClass, null);
		}

		/// <summary>
		/// Gets worklist items for the username that is passed in. Pass in the username as domain\user
		/// </summary>
		/// <param name="testClass"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public static Worklist GetWorklist(IProcessTest testClass, string user)
		{
			ConnectionSetup connSetup = new ConnectionSetup();

			if (String.IsNullOrEmpty(user))
			{
				connSetup.ConnectionString = ConnectionString;
			}
			else
			{
				connSetup.ConnectionString = ConnectionStringImpersonation;
			}

			using (Connection conn = new Connection())
			{
				conn.Open(connSetup);
				if (!String.IsNullOrEmpty(user))
				{
					conn.ImpersonateUser(user);					
				}

				WorklistCriteria criteria = new WorklistCriteria();
				criteria.AddFilterField(WCField.ProcessFullName, WCCompare.Equal, testClass.ProcessName);

				// filter by the folio as well so we only get worklist items back for this process instance
				criteria.AddFilterField(WCField.ProcessFolio, WCCompare.Equal, GetFolio(testClass));

				Worklist list = conn.OpenWorklist(criteria);
				return list;
			}
		}

		public static void ActionWorklistItem(IProcessTest testClass, WorklistItem worklistItem, string action)
		{
			ConnectionSetup connSetup = new ConnectionSetup();
			connSetup.ConnectionString = ConnectionStringImpersonation;

			using (Connection conn = new Connection())
			{
				conn.Open(connSetup);

				// need to impersonate the allocated user in order to action the item
				conn.ImpersonateUser(worklistItem.AllocatedUser);

				// need to open the item so there is a current socket open
				WorklistItem openItem = conn.OpenWorklistItem(worklistItem.SerialNumber);

				if (openItem != null)
				{
					foreach (SourceCode.Workflow.Client.Action itemAction in openItem.Actions)
					{
						if (itemAction.Name == action)
						{
							itemAction.Execute(true);
							break;
						}
					}
				}
			}

			// sleep the thread to give time for the logs to be written
			System.Threading.Thread.Sleep(2000);
		}

		public static bool EnsureActivityExecuted(IProcessTest testClass, string activityName)
		{
			bool executed = false;
			string query = "select * from Activity_Instance.List where ProcessInstanceID = " + testClass.TestScenarioContext.ProcessId.ToString();
			using (SOConnection soConn = new SOConnection(SOConnectionString))
			using (SOCommand command = new SOCommand(query, soConn))
			using (SODataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
			{
				soConn.Open();
				while (reader.Read())
				{
					string executedActivityName = reader["ActivityName"].ToString();					
					if (executedActivityName.Equals(activityName))
					{
						executed = true;
						break;
					}
				}
			}

			return executed;
		}

		public static string GetFolio(IProcessTest testClass)
		{
			string folio = "Unit Test - " + testClass.TestContext.TestName + " - Test Instance ID: " + testClass.TestScenarioContext.TestInstanceId.ToString();
			return folio;
		}
	}
}
