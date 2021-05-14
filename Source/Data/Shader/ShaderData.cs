using System;
using UnityEngine;

namespace UImGui
{
	[Serializable]
	internal class ShaderData
	{
		public Shader Mesh;
		public Shader Procedural;

		public ShaderData Clone()
		{
			return (ShaderData)MemberwiseClone();
		}
	}
}
