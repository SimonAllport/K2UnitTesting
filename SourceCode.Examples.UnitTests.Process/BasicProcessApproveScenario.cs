using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceCode.Examples.UnitTests.Framework.Entities;
using SourceCode.Examples.UnitTests.Framework.Interfaces;
using SourceCode.Examples.UnitTests.Framework;
using SourceCode.Workflow.Client;

namespace SourceCode.Examples.UnitTests.Process
{
	[TestClass]
	public class BasicProcessApproveScenario : IProcessTest
	{
		private const string PROCESS_NAME = @"UnitTestingProcesses\BasicProcess";

		private TestScenarioContext _testScenarioContext;
		private TestContext _testContext;

		public TestContext TestContext
		{
			get { return _testContext; }
			set { _testContext = value; }
		}

		public TestScenarioContext TestScenarioContext
		{
			get { return _testScenarioContext; }
			set { _testScenarioContext = value; }
		}

		public string ProcessName
		{
			get { return PROCESS_NAME; }
		}

		[TestInitialize]
		public void InitializeTest()
		{
			_testScenarioContext = new TestScenarioContext();
			_testScenarioContext.TestInstanceId = Guid.NewGuid();
		}

		[TestCleanup]
		public void CleanupTest()
		{			
			_testScenarioContext = null;
		}

		[TestMethod]
		public void TestApproveScenario()
		{
			ProcessHelper.StartProcess(this);
			Assert.IsNotNull(this.TestScenarioContext.ProcessInstance, "Unable to start process.");

			Worklist activeWorkListItems = ProcessHelper.GetWorklist(this, @"denallix\chad");

			// in this process there should only be one active worklist item
			Assert.AreEqual<int>(1, activeWorkListItems.Count);

			// the user for this task should be chad
			Assert.AreEqual<string>(@"k2:denallix\chad", activeWorkListItems[0].AllocatedUser.ToLower());

			ProcessHelper.ActionWorklistItem(this, activeWorkListItems[0], "Approve");

			bool approveBranchExecuted = ProcessHelper.EnsureActivityExecuted(this, "Approve Branch");
			Assert.IsTrue(approveBranchExecuted, "Approve Branch did not execute");

			bool denyBranchExecuted = ProcessHelper.EnsureActivityExecuted(this, "Deny Branch");
			Assert.IsFalse(denyBranchExecuted, "Deny Branch executed when it should not have");
		}
	}
}
