using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCode.Examples.UnitTests.Framework.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SourceCode.Examples.UnitTests.Framework.Interfaces
{
	public interface IProcessTest
	{
		TestScenarioContext TestScenarioContext { get; set; }
		TestContext TestContext { get; set; }
		string ProcessName { get; }
	}
}
