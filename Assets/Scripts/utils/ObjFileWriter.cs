using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kayac
{
	public class ObjFileWriter
	{
		public static string ToText(Mesh mesh, int subMeshIndex)
		{
			return ToText(mesh.vertices, mesh.uv, mesh.normals, mesh.GetIndices(subMeshIndex));
		}

		public static string ToText(
			IList<Vector3> positions,
			IList<Vector2> uvs,
			IList<Vector3> normals,
			IList<int> indices)
		{
			var sb = new System.Text.StringBuilder();
			Debug.Assert(positions != null);
			sb.AppendFormat("Generated by Kayac.ObjFileWriter. {0} vertices, {1} faces.\n", positions.Count, indices.Count / 3);
			sb.AppendLine("# positions");
			foreach (var item in positions)
			{
				sb.AppendFormat("v {0} {1} {2}\n",
					item.x.ToString("F8"), //精度指定しないとfloat精度の全体を吐かないので劣化してしまう。10進8桁必要
					item.y.ToString("F8"),
					item.z.ToString("F8"));
			}

			bool hasUv = (uvs != null) && (uvs.Count > 0);
			if (hasUv)
			{
				Debug.Assert(uvs.Count == positions.Count);
				sb.AppendLine("\n# texcoords");
				foreach (var item in uvs)
				{
					sb.AppendFormat("vt {0} {1}\n",
						item.x.ToString("F8"),
						item.y.ToString("F8"));
				}
			}

			Debug.Assert(normals != null);
			sb.AppendLine("\n# normals");
			foreach (var item in normals)
			{
				sb.AppendFormat("vn {0} {1} {2}\n",
					item.x.ToString("F8"),
					item.y.ToString("F8"),
					item.z.ToString("F8"));
			}

			Debug.Assert(indices != null);
			Debug.Assert((indices.Count % 3) == 0);
			sb.AppendLine("\n# triangle faces");
			for (var i = 0; i < indices.Count; i += 3)
			{
				var i0 = indices[i + 0] + 1; // 1 based index.
				var i1 = indices[i + 1] + 1;
				var i2 = indices[i + 2] + 1;
				if (hasUv)
				{
					sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
						i0,
						i1,
						i2);
				}
				else
				{
					sb.AppendFormat("f {0}//{0} {1}//{1} {2}//{2}\n",
						i0,
						i1,
						i2);
				}
			}
			return sb.ToString();
		}

#if UNITY_EDITOR
		[MenuItem("Assets/Save .obj")]
		public static void Save()
		{
			var selected = Selection.activeObject;
			var mesh = selected as Mesh;
			if (mesh == null)
			{
				Debug.LogError("selected object is not mesh. type=" + selected.GetType().Name);
				return;
			}
			var originalPath = AssetDatabase.GetAssetPath(mesh);
			var dir = System.IO.Path.GetDirectoryName(originalPath);
			Write(dir, mesh, importImmediately: true);
		}

		[MenuItem("Assets/Save .obj", true)]
		private static bool ValidateSave()
		{
			// Meshでだけ有効
			return Selection.activeObject.GetType() == typeof(Mesh);
		}

		[MenuItem("CONTEXT/MeshFilter/Save .obj")]
		public static void SaveObjFromInspector(MenuCommand menuCommand)
		{
			var meshFilter = menuCommand.context as MeshFilter;
			if (meshFilter != null)
			{
				var mesh = meshFilter.sharedMesh;
				if (mesh != null)
				{
					Write("Assets", mesh, importImmediately: true);
				}
			}
		}

		public static bool Write(
			string directory,
			Mesh mesh,
			bool importImmediately = false)
		{
			Debug.Assert(mesh != null);
			bool ret = true;
			string name = mesh.name;
			if (string.IsNullOrEmpty(name))
			{
				name = "noname";
			}
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				string filename;
				if (mesh.subMeshCount == 1) // 1個なら番号ついても邪魔だろう
				{
					filename = string.Format("{0}.obj", name);
				}
				else
				{
					filename = string.Format("{0}_{1}.obj", name, i);
				}
				var path = System.IO.Path.Combine(directory, filename);
				if (!Write(path, mesh, i, importImmediately))
				{
					ret = false;
				}
			}
			return ret;
		}

		public static bool Write(
			string path,
			Mesh mesh,
			int subMeshIndex,
			bool importImmediately = false)
		{
			var text = ToText(mesh, subMeshIndex);
			return Write(path, text, importImmediately);
		}

		public static bool Write(
			string path,
			IList<Vector3> positions,
			IList<Vector2> uvs,
			IList<Vector3> normals,
			IList<int> indices,
			bool importImmediately = false)
		{
			var text = ToText(positions, uvs, normals, indices);
			return Write(path, text, importImmediately);
		}

		// おまけ。 TODO: Objと何の関係もないので、別ファイルが望ましい。
		[MenuItem("CONTEXT/MeshFilter/Save .asset")]
		public static void SaveAssetFromInspector(MenuCommand menuCommand)
		{
			var meshFilter = menuCommand.context as MeshFilter;
			if (meshFilter != null)
			{
				var mesh = meshFilter.sharedMesh;
				if (mesh != null)
				{
					string name = mesh.name;
					if (string.IsNullOrEmpty(name))
					{
						name = "noname";
					}
					var path = string.Format("Assets/{0}.asset", name);
					AssetDatabase.CreateAsset(mesh, path);
					AssetDatabase.SaveAssets(); // これがないと中身が空になる仕様らしい
				}
			}
		}

		// non-public ------------------
		static bool Write(string path, string objFileText, bool importImmediately)
		{
			bool ret = false;
			try
			{
				System.IO.File.WriteAllText(path, objFileText);
				if (importImmediately)
				{
					UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.Default);
				}
				ret = true;
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
			return ret;
		}
#endif
	}
}