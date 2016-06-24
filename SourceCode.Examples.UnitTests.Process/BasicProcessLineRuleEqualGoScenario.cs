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
	public class BasicProcessLineRuleEqualGoScenario : IProcessTest
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
		public void TestLineRuleBranchEqualsGoScenario()
		{
			Dictionary<string, string> dataFields = new Dictionary<string, string>(1);
			dataFields.Add("lineRuleField", "Go");
			
			ProcessHelper.StartProcess(this, dataFields);
			Assert.IsNotNull(this.TestScenarioContext.ProcessInstance, "Unable to start process.");

			bool lineRuleActivityExecuted = ProcessHelper.EnsureActivityExecuted(this, "Line Rule Activity");
			Assert.IsTrue(lineRuleActivityExecuted, "Line Rule Activity did not execute");

			ProcessHelper.StopProcess(this);
		}
	}
}
