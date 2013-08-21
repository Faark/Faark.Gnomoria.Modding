using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;


namespace Faark.Gnomoria.Mods.BugStuff
{
#if false

    
    /// <summary>
    /// Just a little mod that fixed me a save i did crash while modding....
    /// </summary>
    public class FixAnIssueHelper: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(GnomanEmpire).GetMethod("LoadGame", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<GnomanEmpire, string, bool>(GnomLoad)
                    );
            }
        }
        public static string toString(int[][] data)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == null)
                    continue;
                sb.Append(i).Append(";");
                for (var v = 0; v < data[i].Length; v++)
                {
                    sb.Append(data[i][v]).Append(";");
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        public static void GnomLoad(GnomanEmpire self, string file, bool fallen)
        {
            var toSkillLevel = 300;
            var trysPerSkillLevel = 10000000;
            var stats = new int[toSkillLevel][];

            for (var i = 1; i < toSkillLevel; i++)
            {
                stats[i] = new int[6];

                for (var v = 0; v < trysPerSkillLevel; v++)
                {
                    float num2 = 7.5f * (((float)i)/100.0f) / 3.0f;
                    double num3 = GnomanEmpire.MappedNormalDistribution(0f, 6f, num2, Math.Max(num2 - 0f, 6f - num2) * 0.25f);
                    int quality = (int)MathHelper.Clamp((float)num3, 0f, 5f);
                    stats[i][quality]++;
                }
            }

            return;
        }
    }
#endif
}
