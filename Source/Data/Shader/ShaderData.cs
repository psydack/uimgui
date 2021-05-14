using System;

namespace UImGui
{
	[Serializable]
	internal class ShaderData
	{
		public ShaderData Mesh;
		public ShaderData Procedural;

		public ShaderData Clone()
		{
			return (ShaderData)MemberwiseClone();
		}
	}
}
