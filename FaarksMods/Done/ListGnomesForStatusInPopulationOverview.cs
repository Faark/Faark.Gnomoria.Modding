using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;

namespace Faark.Gnomoria.Mods
{
#if true
    /// <summary>
    /// Displays the number of gnomes with missing limbs and adds Mouse Popups that lists the gnomes for the following labels on the Population Overview: 
    ///    Idle, Injured, Deceased, Missing Limbs
    /// </summary>
    public class ListGnomesForStatusInPopulationOverview: Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(PopulationOverviewUI).GetMethod("SetupPanel", BindingFlags.Public | BindingFlags.Instance),
                    Method.Of<PopulationOverviewUI>(On_PopulationOverviewUI_SetupPanel)
                    );
            }
        }
        public override string Author
        {
            get
            {
                return "Faark";
            }
        }
        public override string Description
        {
            get
            {
                return "Displays the number of gnomes with missing limbs and adds Mouse Popups that lists the gnomes for the following labels on the Population Overview: Idle, Injured, Deceased, Missing Limbs";
            }
        }

        private static void MayAddTooltipToLabel<T>(Label targetLabel, IEnumerable<T> elements, Func<T, String> toString)
        {
            if (elements.Any())
            {
                var text = String.Join(Environment.NewLine, elements.Take(16).Select(el => toString(el)));
                targetLabel.ToolTip = new ToolTip(targetLabel.Manager)
                {
                    Text = (elements.Count() <= 16) ? text : (text + Environment.NewLine + "...")
                };
            }
        }
        private static void MayAddTooltipToLabelFromDict<TK, TV>(Label trgLabel, IEnumerable<KeyValuePair<TK, TV>> elements, Func<TV, String> toString)
        {
            MayAddTooltipToLabel(trgLabel, elements.Select(el => el.Value), toString);
        }
        public static void On_PopulationOverviewUI_SetupPanel(PopulationOverviewUI self)
        {
            var labels = self.ClientArea.Controls.Where(ctrl => ctrl is Label).Cast<Label>();
            var leftPos = labels.Min(lbl => lbl.Left);
            var possibleLabels = labels.Where(lbl => lbl.Left == leftPos).OrderBy(lbl => lbl.Top).ToArray();

            if (possibleLabels.Length < 5)
            {
                throw new Exception("Could not find all labels to modify.");
            }
            var playerFaction = GnomanEmpire.Instance.World.AIDirector.PlayerFaction;
            MayAddTooltipToLabelFromDict(possibleLabels[1], playerFaction.DeceasedMembers, gnome => gnome.Name());
            MayAddTooltipToLabelFromDict(possibleLabels[2], playerFaction.Members.Where(kvp => kvp.Value.Job == null && !kvp.Value.Body.IsSleeping), gnome => gnome.NameAndTitle());
            MayAddTooltipToLabelFromDict(possibleLabels[3], playerFaction.Members.Where(kvp => !kvp.Value.IsHealthy()), gnome => gnome.NameAndTitle());

            var limbless = playerFaction.Members.Where(kvp => kvp.Value.Body.BodySections.Any(section=>section.Status.HasFlag(BodySectionStatus.Missing)));
            var limblessCnt = limbless.Count();
            var dist = possibleLabels[1].Top - possibleLabels[0].Top;
            var limblessLabel = new Label(self.Manager);
            limblessLabel.Init();
            limblessLabel.Anchor = possibleLabels[3].Anchor;
            limblessLabel.Top = possibleLabels[3].Top + dist;
            limblessLabel.Height = possibleLabels[3].Height;
            limblessLabel.Width = possibleLabels[3].Width;
            limblessLabel.Left = possibleLabels[3].Left;
            limblessLabel.Text = "Misses Limbs: " + limblessCnt;
            if (limblessCnt > 0)
            {
                MayAddTooltipToLabelFromDict(limblessLabel, limbless, gnome =>
                {
                    return gnome.NameAndTitle() + ": " + String.Join(
                        ", ",
                        gnome.Body.BodySections.Where(
                            section => section.Status.HasFlag(BodySectionStatus.Missing)
                        ).Select(
                            section => section.Name
                        ));
                });
            }
            self.Add(limblessLabel);

            foreach (var el in self.ClientArea.Controls.Where(ctrl => (ctrl.Left == leftPos) && (ctrl.Top > limblessLabel.Top)))
            {
                el.Top += dist;
            }
        }
    }
#endif
}
