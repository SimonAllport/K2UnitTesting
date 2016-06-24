using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCode.Workflow.Client;

namespace SourceCode.Examples.UnitTests.Framework.Entities
{
	public class TestScenarioContext
	{
		public ProcessInstance ProcessInstance { get; set; }
		public Guid TestInstanceId { get; set; }

		public long ProcessId 
		{
			get
			{
				long id = -1;
				if (this.ProcessInstance != null)
				{
					id = this.ProcessInstance.ID;
				}
				return id;
			}
		}		
	}
}
