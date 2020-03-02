using System.Collections.Generic;
using JetbrainsToolboxShortcutsGenerator;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class ProgramTests
	{
		[Test]
		[TestCase("C:\\Programming\\Jetbrains\\apps\\WebStorm\\ch-0\\193.6494.34", ExpectedResult = "193.6494.34")]
		public string GetVersionFromPath_ShouldBe_CorrectVersion(string path)
		{
			string version = Program.GetVersionFromPath(path);
			return version;
		}

		[Test]
		public void GetHighestVersionResult_TwoResults()
		{
			List<BinarySearchResult> data = new List<BinarySearchResult>
			{
					new BinarySearchResult()
					{
							Version = "193.6494.30"
					},
					new BinarySearchResult()
					{
							Version = "193.6494.48"
					}
			};

			var result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "193.6494.48");
			
			data.Add(new BinarySearchResult()
			{
					Version = "193.6495.1"
			});
			
			result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "193.6495.1");
			
			data.Add(new BinarySearchResult()
			{
					Version = "211.1.101"
			});
			result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "211.1.101");
		}
		
		[Test]
		public void GetHighestVersionResult_ThreeResults()
		{
			List<BinarySearchResult> data = new List<BinarySearchResult>
			{
					new BinarySearchResult()
					{
							Version = "193.6494.30"
					},
					new BinarySearchResult()
					{
							Version = "193.6495.1"
					},
					new BinarySearchResult()
					{
							Version = "193.6494.48"
					}
			};

			var result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "193.6495.1");
		}
		
		[Test]
		public void GetHighestVersionResult_FourResults()
		{
			List<BinarySearchResult> data = new List<BinarySearchResult>
			{
					new BinarySearchResult()
					{
							Version = "211.1.101"
					},
					new BinarySearchResult()
					{
							Version = "193.6494.30"
					},
					new BinarySearchResult()
					{
							Version = "193.6495.1"
					},
					new BinarySearchResult()
					{
							Version = "193.6494.48"
					}
			};

			var result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "211.1.101");
		}
		
		[Test]
		public void GetHighestVersionResult_EmptyVersion()
		{
			List<BinarySearchResult> data = new List<BinarySearchResult>
			{
					new BinarySearchResult()
					{
							Version = "211.1.101"
					},
					new BinarySearchResult()
					{
							Version = "193.6494.30"
					},
					new BinarySearchResult()
					{
							Version = ""
					},
					new BinarySearchResult()
					{
							Version = "193.6494.48"
					}
			};

			var result = Program.GetHighestVersionResult(data);
			Assert.IsTrue(result.Version == "211.1.101");
		}
	}
}