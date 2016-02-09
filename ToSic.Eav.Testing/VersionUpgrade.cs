using NUnit.Framework;

namespace ToSic.Eav.Testing
{
	public class VersionUpgrade
	{
		[Test]
		public void EnsurePipelineDesignerAttributeSets()
		{
			var upgrade = new Eav.VersionUpgrade("Unit Test");
			upgrade.EnsurePipelineDesignerAttributeSets();
		}
	}
}
