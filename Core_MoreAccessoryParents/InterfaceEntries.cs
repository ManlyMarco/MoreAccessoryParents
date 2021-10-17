using System.Collections.Generic;
using System.Linq;

namespace KK_MoreAccessoryParents
{
	internal static class InterfaceEntries
	{
		internal static readonly BoneDropdown[] BoneList =
		{
				new BoneDropdown(
					 "Fingers", new[]
					 {
						  new BoneEntry("cf_j_little01_L", "Little F. Base Left"  , "cf_j_little01_R", "Little F. Base Right"  ),
						  new BoneEntry("cf_j_little02_L", "Little F. Center Left", "cf_j_little02_R", "Little F. Center Right"),
						  new BoneEntry("cf_j_little03_L", "Little F. Tip Left"   , "cf_j_little03_R", "Little F. Tip Right"   ),
						  new BoneEntry("cf_j_ring01_L"  , "Ring F. Base Left"    , "cf_j_ring01_R"  , "Ring F. Base Right"    ),
						  new BoneEntry("cf_j_ring02_L"  , "Ring F. Center Left"  , "cf_j_ring02_R"  , "Ring F. Center Right"  ),
						  new BoneEntry("cf_j_ring03_L"  , "Ring F. Tip Left"     , "cf_j_ring03_R"  , "Ring F. Tip Right"     ),
						  new BoneEntry("cf_j_middle01_L", "Middle F. Base Left"  , "cf_j_middle01_R", "Middle F. Base Right"  ),
						  new BoneEntry("cf_j_middle02_L", "Middle F. Center Left", "cf_j_middle02_R", "Middle F. Center Right"),
						  new BoneEntry("cf_j_middle03_L", "Middle F. Tip Left"   , "cf_j_middle03_R", "Middle F. Tip Right"   ),
						  new BoneEntry("cf_j_index01_L" , "Index F. Base Left"   , "cf_j_index01_R" , "Index F. Base Right"   ),
						  new BoneEntry("cf_j_index02_L" , "Index F. Center Left" , "cf_j_index02_R" , "Index F. Center Right" ),
						  new BoneEntry("cf_j_index03_L" , "Index F. Tip Left"    , "cf_j_index03_R" , "Index F. Tip Right"    ),
						  new BoneEntry("cf_j_thumb01_L" , "Thumb Base Left"      , "cf_j_thumb01_R" , "Thumb Base Right"      ),
						  new BoneEntry("cf_j_thumb02_L" , "Thumb Center Left"    , "cf_j_thumb02_R" , "Thumb Center Right"    ),
						  new BoneEntry("cf_j_thumb03_L" , "Thumb Tip Left"       , "cf_j_thumb03_R" , "Thumb Tip Right"       )
					 }),
				new BoneDropdown(
					 "Other", new[]
					 {
						  new BoneEntry("cf_s_siri_L", "Left Butt Cheek", "cf_s_siri_R", "Right Butt Cheek"),
						  new BoneEntry("cf_s_spine01", "Upper Waist"),
						  new BoneEntry("cf_s_spine02", "Lower Torso"),
						  new BoneEntry("cf_j_toes_L", "Left Toes", "cf_j_toes_R", "Right Toes"),
					 }),
		  };

		public static readonly string[] AllBones = BoneList.SelectMany(x => x.GetBoneNames(false)).ToArray();

		public static readonly string[] FancyBoneNames = BoneList.SelectMany(x => x.GetBoneNames(true)).ToArray();

		public static string FindReverseBone(string boneName)
		{
			foreach (var entry in BoneList.SelectMany(dd => dd.Bones))
			{
				if (entry.Left == boneName)
					return entry.HasSides ? entry.Right : string.Empty;
			}
			return string.Empty;
		}

		internal sealed class BoneDropdown
		{
			public BoneDropdown(string name, BoneEntry[] bones)
			{
				Name = name;
				Bones = bones;
			}

			public IEnumerable<string> GetBoneNames(bool fancy)
			{
				if (fancy)
					return Bones.SelectMany(
					y => y.HasSides ? new[] { y.LeftFancy, y.RightFancy } : new[] { y.LeftFancy });

				return Bones.SelectMany(
					 y => y.HasSides ? new[] { y.Left, y.Right } : new[] { y.Left });
			}

			internal BoneEntry[] Bones { get; }
			public string Name { get; }
		}

		internal sealed class BoneEntry
		{
			public BoneEntry(string left, string leftFancy) : this(left, leftFancy, null, null) { }

			public BoneEntry(string left, string leftFancy, string right, string rightFancy)
			{
				Left = left;
				Right = right;
				LeftFancy = leftFancy;
				RightFancy = rightFancy;
			}

			public bool HasSides => Right != null;

			public string Left { get; }
			public string LeftFancy { get; }
			public string Right { get; }
			public string RightFancy { get; }
		}
	}
}
