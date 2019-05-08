using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var gamelanguage = "de,en,es,fr,it,pl,ru,cs";
			var languages = gamelanguage.Split(',').Select(iso =>
			{
				CultureInfo ci = new CultureInfo(iso.ToUpperInvariant(), false);
				return ci;
			}).OrderBy(cb => cb.DisplayName).ToArray();


		}
	}
}
