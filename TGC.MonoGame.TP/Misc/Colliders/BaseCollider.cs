using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.Misc.Colliders
{
	public abstract class BaseCollider
	{
		public Vector3 Center;

		public Matrix Orientation = Matrix.Identity;

		public Vector3 Extents;

		public BaseCollider() { }

		public abstract bool Intersects(BaseCollider collider);
	}
}
