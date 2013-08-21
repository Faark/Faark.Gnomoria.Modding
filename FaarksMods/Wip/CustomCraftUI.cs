using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Faark.Gnomoria.Modding;
using Game;
using Game.GUI;
using Game.GUI.Controls;
using GameLibrary;
using Faark.Util;
using Microsoft.Xna.Framework;



namespace Faark.Gnomoria.Mods.CustomWorkshopUI
{
#if true

    public class SomewhatUsefulComponentMaterialSelector : Panel
    {
        public class MaterialSelectionChangedEventArgs : System.EventArgs
        {
            public int ComponentIndex { get; protected set; }
            public ItemComponent Component { get; protected set; }
            public Material OldMaterial { get; protected set; }
            public Material NewMaterial { get; protected set; }
            public MaterialSelectionChangedEventArgs(int index, ItemComponent comp, Material oldMat, Material newMat)
            {
                ComponentIndex = index;
                Component = comp;
                OldMaterial = oldMat;
                NewMaterial = newMat;
            }
        }
        protected List<Label> labels;
        protected List<ComboBox> comboBoxes;
        protected List<Dictionary<int, Material>> possibleMaterials;

        protected ItemComponent[] components;
        protected uint quantity;
        public SomewhatUsefulComponentMaterialSelector(Manager manager)
            : base(manager)
        {
            this.AutoScroll = true;
            this.Anchor = Anchors.Vertical;
            this.Color = Microsoft.Xna.Framework.Color.Transparent;
            this.labels = new List<Label>();
            this.comboBoxes = new List<ComboBox>();
        }
        public void CreateComponentSelectors(ItemComponent[] components, uint buildQuantity = 1u)
        {
            this.components = components;
            this.quantity = buildQuantity;
            #region clr
            foreach (Label current in this.labels)
            {
                current.Dispose();
                this.Remove(current);
            }
            foreach (ComboBox current2 in this.comboBoxes)
            {
                current2.Dispose();
                this.Remove(current2);
            }
            this.labels.Clear();
            this.comboBoxes.Clear();
            this.possibleMaterials = new List<Dictionary<int, Material>>();
            #endregion
            Manager manager = GnomanEmpire.Instance.GuiManager.Manager;
            int num = 0;
            GameEntityManager entityManager = GnomanEmpire.Instance.EntityManager;
            StockManager stockManager = GnomanEmpire.Instance.Fortress.StockManager;
            for (int i = 0; i < components.Length; i++)
            {
                ItemComponent itemComponent = components[i];
                ItemID iD = itemComponent.ID;
                ItemDef itemDef = entityManager.ItemDef(iD);
                uint num2 = buildQuantity * itemComponent.Quantity;
                #region lbl
                Label label = new Label(manager)
                {
                    Text = string.Format("{0}x", num2),
                    Margins = new Margins(0, 4, 0, 4),
                    Width = 40,
                    Alignment = Alignment.MiddleRight
                };
                num += label.Margins.Top;
                label.Top = num;
                label.Init();
                this.labels.Add(label);
                this.Add(label);
                #endregion
                ComboBox comboBox = new ComboBox(manager);
                comboBox.Init();
                comboBox.ReadOnly = true;
                comboBox.Margins = new Margins(8, 4, 8, 6);
                bool flag = itemComponent.AllowedMaterials.Count > 0;
                Dictionary<int, List<Item>> dictionary = stockManager.ItemsByItemID(iD);
                Dictionary<int, Material> dictionary2 = new Dictionary<int, Material>();
                if (flag)
                {
                    foreach (Material current3 in itemComponent.AllowedMaterials)
                    {
                        dictionary2.Add(comboBox.Items.Count, current3);
                        MaterialProperty arg_272_0 = GnomanEmpire.Instance.Map.TerrainProperties[(int)current3];
                        List<Item> list;
                        if (dictionary != null && dictionary.TryGetValue((int)current3, out list))
                        {
                            comboBox.Items.Add(string.Format("{0} ({1})", Item.Name(iD, (int)current3, null), list.Count));
                        }
                        else
                        {
                            comboBox.Items.Add(string.Format("{0} ({1})", Item.Name(iD, (int)current3, null), 0));
                        }
                    }
                    if (dictionary2.Count > 0)
                    {
                        this.possibleMaterials.Add(dictionary2);
                    }
                }
                else
                {
                    if (dictionary != null)
                    {
                        int num3 = 0;
                        foreach (KeyValuePair<int, List<Item>> current4 in dictionary)
                        {
                            num3 += current4.Value.Count;
                        }
                        dictionary2.Add(0, Material.Count);
                        comboBox.Items.Add(string.Format("any {0} ({1})", Item.GroupName(iD), num3));
                        foreach (KeyValuePair<int, List<Item>> current5 in dictionary)
                        {
                            MaterialProperty arg_3FA_0 = GnomanEmpire.Instance.Map.TerrainProperties[current5.Key];
                            dictionary2.Add(comboBox.Items.Count, (Material)current5.Key);
                            if (current5.Value.Count > 0)
                            {
                                comboBox.Items.Add(string.Format("{0} ({1})", current5.Value[0].Name(), current5.Value.Count));
                            }
                            else
                            {
                                comboBox.Items.Add(string.Format("{0} ({1})", Item.Name(iD, current5.Key, null), current5.Value.Count));
                            }
                        }
                        if (dictionary2.Count > 0)
                        {
                            this.possibleMaterials.Add(dictionary2);
                        }
                    }
                }
                if (dictionary2.Count == 0)
                {
                    dictionary2.Add(0, Material.Count);
                    comboBox.Items.Add(string.Format("any {0} ({1})", Item.GroupName(iD), 0));
                    this.possibleMaterials.Add(dictionary2);
                }
                var lastItemIndex = comboBox.ItemIndex = 0;
                var current_index = i;
                comboBox.ItemIndexChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
                {
                    var last = lastItemIndex;
                    lastItemIndex = comboBox.ItemIndex;
                    OnMaterialChanged(current_index, components[current_index], (Material)dictionary2[comboBox.ItemIndex], (Material)dictionary2[last]);
                });
                comboBox.Left = label.Left + label.Width + label.Margins.Right + comboBox.Margins.Left;
                comboBox.Top = num;
                comboBox.Width = this.ClientRect.Right - comboBox.Left - comboBox.Margins.Right - (10 + comboBox.Margins.Left);
                if (comboBox.Width < 300)
                {
                    comboBox.ListBox.Width = 300;
                }
                comboBox.Anchor = (Anchors.Left | Anchors.Top | Anchors.Right);
                this.comboBoxes.Add(comboBox);
                this.Add(comboBox);
                if (itemDef.ObtainDescription != null)
                {
                    Label label2 = new Label(manager);
                    label2.Init();
                    label2.Top = num;
                    label2.Left = comboBox.Left + comboBox.Width + comboBox.Margins.Left;
                    label2.Alignment = Alignment.MiddleLeft;
                    label2.Text = "?";
                    label2.ToolTip.Text = itemDef.ObtainDescription;
                    label2.Width = 10;
                    label2.Height = comboBox.Height;
                    label2.Anchor = (Anchors.Top | Anchors.Right);
                    this.labels.Add(label2);
                    this.Add(label2);
                }
                num += label.Height + label.Margins.Bottom;
            }
        }
        public bool HasAllIngredients()
        {
            for (int i = 0; i < this.comboBoxes.Count; i++)
            {
                if (!this.comboBoxes[i].Enabled || !this.possibleMaterials[i].ContainsKey(this.comboBoxes[i].ItemIndex))
                {
                    return false;
                }
            }
            return true;
        }

        public List<Material> ComponentMaterials()
        {
            if (!this.HasAllIngredients())
            {
                return null;
            }
            var list = new List<Material>();
            for (int i = 0; i < this.comboBoxes.Count; i++)
            {
                list.Add((Material)this.possibleMaterials[i][this.comboBoxes[i].ItemIndex]);
            }
            return list;
        }

        public event EventHandler<MaterialSelectionChangedEventArgs> MaterialChanged;
        protected virtual void OnMaterialChanged(int index, ItemComponent comp, Material newMaterial, Material oldMaterial)
        {
            MaterialChanged.TryRaise(this, new MaterialSelectionChangedEventArgs(index, comp, oldMaterial, newMaterial));
        }

        public void TrySetMaterials(IEnumerable<Material> mats)
        {
            var index = 0;
            foreach (var mat in mats)
            {
                if (index >= components.Length)
                {
                    break;
                }

                var got = false;
                var indexToMat = possibleMaterials[index].FirstOrDefault(kvp => (got = got || (kvp.Value == mat)));
                if (got)
                {
                    comboBoxes[index].ItemIndex = indexToMat.Key;
                }
                index++;
            }
        }
    }
    public class CustomListbox<T> : LoweredPanel where T : Control
    {
        protected List<T> controls = new List<T>();
        public IEnumerable<T> Elements { get { return controls; } }
        public void AddElements(IEnumerable<T> ctrls)
        {
            foreach (var ctrl in ctrls)
            {
                AddElement(ctrl);
            }
        }
        public void AddElement(T ctrl)
        {
            controls.Add(ctrl);
            ctrl.Anchor = Anchors.Top | Anchors.Horizontal;
            ctrl.Width = this.ClientRect.Width;
            Add(ctrl);
            RefreshPositions();
            Invalidate();
        }
        public void RemoveElements(IEnumerable<T> ctrls)
        {
            ctrls = ctrls.ToList();
            foreach (var ctrl in ctrls)
            {
                RemoveElement(ctrl);
            }
        }
        public void RemoveElements(Func<T, bool> predicate)
        {
            RemoveElements(ClientArea.Controls.Select(ctrl => ctrl as T).Where(el => el != null).Where(predicate));
        }
        public void RemoveElement(T ctrl)
        {
            Remove(ctrl);
            controls.Remove(ctrl);
            RefreshPositions();
            Invalidate();
        }
        public void ReplaceElement(T old_ctrl, T new_ctrl)
        {
            controls.Insert(controls.IndexOf(old_ctrl), new_ctrl);
            controls.Remove(old_ctrl);
            Remove(old_ctrl);
            Add(new_ctrl);
            new_ctrl.Anchor = Anchors.Top | Anchors.Horizontal;
            new_ctrl.Width = this.ClientRect.Width;
            RefreshPositions();
            Invalidate();
            //RuntimeModController.WriteLogO(controls);
        }
        public void InsertElement(int index, T ctrl)
        {
            controls.Insert(index, ctrl);
            ctrl.Anchor = Anchors.Top | Anchors.Horizontal;
            ctrl.Width = this.ClientRect.Width;
            Add(ctrl);
            RefreshPositions();
            Invalidate();
        }
        public void MoveElement(T el, T insert_after)
        {
            var from = controls.IndexOf(el);
            var to = controls.IndexOf(insert_after) + 1;
            if (from == -1)
            {
                throw new ArgumentException("Element to move was not found");
            }
            MoveElement(from, to > from ? to - 1 : to);
        }
        public void MoveElement(int from_index, int to_index)
        {
            if (from_index == to_index)
            {
                return;
            }
            var el = controls[from_index];
            controls.RemoveAt(from_index);
            controls.Insert(to_index, el);
            RefreshPositions();
            Invalidate();
        }
        public void RefreshPositions()
        {
            // TODO: make some kind of invalidate, here?
            var h = 0;
            foreach (var ctrl in controls)
            {
                ctrl.Top = h;
                h += ctrl.Height + 2;
            }
        }
        public CustomListbox(Manager mgr)
            : base(mgr)
        {
            AutoScroll = true;
        }
    }

    public enum QuantityMode
    {
        Once,
        CraftTo,
        Repeat
    }
    public class QuantityModeSelectedEventArgs : System.EventArgs
    {
        public QuantityMode OldMode { get; private set; }
        public QuantityMode NewMode { get; private set; }
        public QuantityModeSelectedEventArgs(QuantityMode oldMode, QuantityMode newMode)
        {
            OldMode = oldMode;
            NewMode = newMode;
        }
    }
    public class QuantitySelect : Panel
    {
        private QuantityMode sMode;
        public event EventHandler<QuantityModeSelectedEventArgs> QuantityChanged;
        public QuantityMode SelectedMode
        {
            get
            {
                return sMode;
            }
            set
            {
                if (value == sMode)
                    return;
                var oldMode = sMode;
                sMode = value;

                if (cbOnce.Checked != (sMode == QuantityMode.Once))
                {
                    cbOnce.Checked = (sMode == QuantityMode.Once);
                }
                if (cbRepeat.Checked != (sMode == QuantityMode.Repeat))
                {
                    cbRepeat.Checked = (sMode == QuantityMode.Repeat);
                }
                if (cbCraftTo.Checked != (sMode == QuantityMode.CraftTo))
                {
                    cbCraftTo.Checked = (sMode == QuantityMode.CraftTo);
                }

                QuantityChanged.TryRaise(this, new QuantityModeSelectedEventArgs(oldMode, sMode));
            }
        }

        public WorkshopJob Job { get; private set; }

        private CheckBox cbOnce;
        private CheckBox cbCraftTo;
        private CheckBox cbRepeat;
        private TextBox tbxCraftTo;
        public uint CraftToValue { get; private set; }
        public QuantitySelect(WorkshopJob job, Manager mgr)
            : base(mgr)
        {
            Job = job;
            CraftToValue = job.CraftTo > 0 ? job.CraftTo : 0u;
            sMode = job.Repeat ? QuantityMode.Repeat : (job.CraftTo > 0 ? QuantityMode.CraftTo : QuantityMode.Once);
            Setup();
        }
        private void unsetMode(QuantityMode quantityMode)
        {
            if (SelectedMode == quantityMode)
            {
                SelectedMode = QuantityMode.Once;
                if (!cbOnce.Checked)
                {
                    cbOnce.Checked = true;
                }
            }
        }
        private void Setup()
        {
            base.Init();
            var label1 = new Label(this.Manager);
            label1.Init();
            label1.Text = "Quantity:";
            label1.Anchor = Anchors.Top | Anchors.Left;
            label1.Width = 90;
            label1.Top = 0;
            label1.Left = 0;
            Add(label1);
            var cbLeft = label1.Left + label1.Width + 2;
            cbOnce = new CheckBox(Manager);
            cbOnce.Init();
            cbOnce.Text = "Craft just once";
            cbOnce.Anchor = Anchors.Top | Anchors.Left;
            cbOnce.Width = 160;
            cbOnce.Top = 4;
            cbOnce.Left = cbLeft;
            cbOnce.Checked = sMode == QuantityMode.Once;
            cbOnce.CheckedChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                if (cbOnce.Checked)
                {
                    SelectedMode = QuantityMode.Once;
                }
                else
                {
                    unsetMode(QuantityMode.Once);
                }
            });
            Add(cbOnce);
            cbCraftTo = new CheckBox(Manager);
            cbCraftTo.Init();
            cbCraftTo.Text = "Craft to ";
            cbCraftTo.Anchor = Anchors.Top | Anchors.Left;
            cbCraftTo.Width = 100;
            cbCraftTo.Top = cbOnce.Top + cbOnce.Height + 4;
            cbCraftTo.Left = cbLeft;
            cbCraftTo.Checked = sMode == QuantityMode.CraftTo;
            cbCraftTo.CheckedChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                if (cbCraftTo.Checked)
                {
                    SelectedMode = QuantityMode.CraftTo;
                }
                else
                {
                    unsetMode(QuantityMode.CraftTo);
                }
            });
            Add(cbCraftTo);
            tbxCraftTo = new TextBox(Manager);
            tbxCraftTo.Init();
            tbxCraftTo.Anchor = Anchors.Top | Anchors.Left;
            tbxCraftTo.Top = cbOnce.Top + cbOnce.Height;
            tbxCraftTo.Left = cbCraftTo.Left + cbCraftTo.Width;
            tbxCraftTo.Text = CraftToValue.ToString();
            tbxCraftTo.Enabled = true;
            tbxCraftTo.TextChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                uint value;
                if (uint.TryParse(tbxCraftTo.Text, out value))
                {
                    if (value != CraftToValue)
                    {
                        CraftToValue = value;
                        if (SelectedMode != QuantityMode.CraftTo)
                        {
                            SelectedMode = QuantityMode.CraftTo;
                        }
                        else
                        {
                            QuantityChanged.TryRaise(this, new QuantityModeSelectedEventArgs(QuantityMode.CraftTo, QuantityMode.CraftTo));
                        }
                    }
                }
            });
            Add(tbxCraftTo);
            cbRepeat = new CheckBox(Manager);
            cbRepeat.Init();
            cbRepeat.Text = "Repeat";
            cbRepeat.Anchor = Anchors.Left | Anchors.Top;
            cbRepeat.Width = 80;
            cbRepeat.Top = cbCraftTo.Top + cbCraftTo.Height + 4;
            cbRepeat.Left = cbLeft;
            cbRepeat.Checked = sMode == QuantityMode.Repeat;
            cbRepeat.CheckedChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
            {
                if (cbRepeat.Checked)
                {
                    SelectedMode = QuantityMode.Repeat;
                }
                else
                {
                    unsetMode(QuantityMode.Repeat);
                }
            });
            Add(cbRepeat);

            Height = cbRepeat.Top + cbRepeat.Height + 4;
            BackColor = Microsoft.Xna.Framework.Color.Transparent;
            AutoScroll = true;
        }

    }

    class CustomGuiInstance
    {
        protected static String CraftableItemText(CraftableItem craftableItem, Material customMaterial = Material.Count)
        {
            var itemDef = GnomanEmpire.Instance.EntityManager.ItemDef(craftableItem.ItemID);
            var sb = new StringBuilder();
            if (craftableItem.Quantity > 1)
            {
                sb.Append(craftableItem.Quantity).Append("x ");
            }
            if (itemDef.Name != null)
            {
                if (craftableItem.ConversionMaterial == GameLibrary.Material.Count)
                {
                    if (customMaterial == Material.Count)
                    {
                        sb.Append(itemDef.Name);
                    }
                    else
                    {
                        sb.Append(Item.Name(craftableItem.ItemID, (int)customMaterial, null));
                    }
                }
                else
                {
                    sb.Append(Item.Name(craftableItem.ItemID, (int)craftableItem.ConversionMaterial, null));
                }
            }
            else
            {
                return itemDef.GroupName;
            }
            return sb.ToString();
        }
        private class DetailedJob_AutocraftPanel : Panel
        {
            protected WorkshopJob job;
            protected CustomGuiInstance guiInstance;

            protected CheckBox toggleOnOffCheckbox;
            protected Label isAutocraftJob_CraftForLabel;
            protected Button isAutocraftJob_CraftForButton;
            protected Label childJob_Label;
            protected Button childJob_Button;
            private int measureTextAndSetWidth(Control ctrl, int offset)
            {
                return ctrl.Width = (int)ctrl.Skin.Layers[0].Text.Font.Resource.MeasureString(ctrl.Text).X + offset;
            }
            private void mayClear<T>(ref T ctrl) where T : Control
            {
                if (ctrl != null)
                {
                    Remove(ctrl);
                    ctrl = null;
                }
            }
            public void Update()
            {
                var left = 0;

                mayClear(ref toggleOnOffCheckbox);
                mayClear(ref isAutocraftJob_CraftForLabel);
                mayClear(ref isAutocraftJob_CraftForButton);
                mayClear(ref childJob_Label);
                mayClear(ref childJob_Button);

                if (!job.HasParent)
                {
                    toggleOnOffCheckbox = new CheckBox(this.Manager);
                    toggleOnOffCheckbox.Init();
                    toggleOnOffCheckbox.Text = "Craft Ingredients";
                    toggleOnOffCheckbox.Left = 0;
                    toggleOnOffCheckbox.Top = 0;
                    toggleOnOffCheckbox.Width = left = 175;
                    toggleOnOffCheckbox.Anchor = Anchors.Left | Anchors.Top;
                    toggleOnOffCheckbox.ToolTip.Text = "Auto craft missing ingredients";
                    toggleOnOffCheckbox.Checked = job.AutoQueue;
                    toggleOnOffCheckbox.CheckedChanged += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        job.AutoQueue = toggleOnOffCheckbox.Checked;
                        guiInstance.SyncJobGuiWithJobs();
                    });
                    Add(toggleOnOffCheckbox);
                }
                else
                {
                    isAutocraftJob_CraftForLabel = new Label(this.Manager);
                    isAutocraftJob_CraftForLabel.Init();
                    isAutocraftJob_CraftForLabel.Text = "For " + job.ParentName() + " at ";
                    isAutocraftJob_CraftForLabel.Left = left;
                    isAutocraftJob_CraftForLabel.Top = 0;
                    left += measureTextAndSetWidth(isAutocraftJob_CraftForLabel, 2);
                    isAutocraftJob_CraftForLabel.Anchor = Anchors.Left | Anchors.Top;
                    Add(isAutocraftJob_CraftForLabel);

                    var parentWorkshop = GnomanEmpire.Instance.EntityManager.Entity(job.ParentWorkshop) as Workshop;

                    isAutocraftJob_CraftForButton = new Button(this.Manager);
                    isAutocraftJob_CraftForButton.Init();
                    isAutocraftJob_CraftForButton.Text = (parentWorkshop == null) ? job.ParentJob.JobName() : parentWorkshop.Name();
                    isAutocraftJob_CraftForButton.Left = left;
                    isAutocraftJob_CraftForButton.Top = 0;
                    left += measureTextAndSetWidth(isAutocraftJob_CraftForButton, 15);
                    isAutocraftJob_CraftForButton.Anchor = Anchors.Left | Anchors.Top;
                    isAutocraftJob_CraftForButton.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        if (parentWorkshop != null)
                        {
                            if (parentWorkshop.ID == job.Workshop)
                            {
                                guiInstance.ToggleJobControl(guiInstance.jobPanel.Elements.Single(el => el.job.ChildJob == job));
                            }
                            else
                            {
                                parentWorkshop.ShowDetailsUI();
                            }
                        }
                        else
                        {
                            job.ParentJob.ShowUI();
                        }
                    });
                    Add(isAutocraftJob_CraftForButton);
                }

                if (job.ChildJob != null)
                {
                    childJob_Label = new Label(this.Manager);
                    childJob_Label.Init();
                    childJob_Label.Text = "; Needs " + job.ChildName() + " at ";
                    childJob_Label.Left = left;
                    childJob_Label.Top = 0;
                    left += measureTextAndSetWidth(childJob_Label, 2);
                    childJob_Label.Anchor = Anchors.Left | Anchors.Top;
                    Add(childJob_Label);

                    var childWorkshop = GnomanEmpire.Instance.EntityManager.Entity(job.ChildJob.Workshop) as Workshop;
                    childJob_Button = new Button(this.Manager);
                    childJob_Button.Init();
                    childJob_Button.Text = childWorkshop.Name();
                    childJob_Button.Left = left;
                    childJob_Button.Top = 0;
                    left += measureTextAndSetWidth(childJob_Button, 15);
                    childJob_Button.Anchor = Anchors.Left | Anchors.Top;
                    childJob_Button.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        if (childWorkshop != null)
                        {
                            if (childWorkshop == guiInstance.Workshop)
                            {
                                guiInstance.ToggleJobControl(guiInstance.jobPanel.Elements.Single(el => el.job == job.ChildJob));
                            }
                            else
                            {
                                childWorkshop.ShowDetailsUI();
                            }
                        }
                        else
                        {
                            job.ParentJob.ShowUI();
                        }
                    });
                    Add(childJob_Button);
                }
            }
            public DetailedJob_AutocraftPanel(CustomGuiInstance guiInstance, WorkshopJob job, Manager mgr)
                : base(mgr)
            {
                this.job = job;
                this.guiInstance = guiInstance;
                Update();
                var last = ((Control)childJob_Button ?? (Control)isAutocraftJob_CraftForButton ?? (Control)toggleOnOffCheckbox);
                Width = last.Left + last.Width;
                Height = last.Height;
            }
        }


        JobDetailed lastSelectedJob = null;
        abstract class JobGuiElement : Panel
        {
            public WorkshopJob job { get; private set; }
            public JobGuiElement(WorkshopJob job, Manager mgr)
                : base(mgr)
            {
                this.job = job;
            }
            public string DebugText { get { return CraftableItemText(job.CraftableItem, (Material)job.Materials[0]); } }
        }
        class JobDetailed : JobGuiElement
        {
            public SomewhatUsefulComponentMaterialSelector ComponentSelector { get; private set; }
            public QuantitySelect QuantitySelector { get; private set; }
            public JobSummary Summary { get; private set; }
            public DetailedJob_AutocraftPanel AutocraftPanel { get; private set; }
            public JobDetailed(CustomGuiInstance guiInstance, WorkshopJob job, Manager mgr)
                : base(job, mgr)
            {
                Summary = new JobSummary(guiInstance, job, mgr);
                Summary.Width = this.Width;
                Summary.Top = 0;
                Summary.Anchor = Anchors.Top | Anchors.Horizontal;
                Add(Summary);
                /*
                var ingridPanel = new CraftItemPanel(mgr);
                */
                ComponentSelector = new SomewhatUsefulComponentMaterialSelector(mgr);
                ComponentSelector.Anchor = Anchors.Horizontal | Anchors.Top;
                ComponentSelector.Top = Summary.Height;
                ComponentSelector.Width = this.Width;
                ComponentSelector.CreateComponentSelectors(job.CraftableItem.Components, 1);
                ComponentSelector.TrySetMaterials(job.Materials.Select(matInt => (Material)matInt));
                var h = ComponentSelector.ClientArea.Controls.Max(el => el.ControlRect.Bottom) + 2;
                ComponentSelector.Height = h;
                ComponentSelector.MaterialChanged += new EventHandler<SomewhatUsefulComponentMaterialSelector.MaterialSelectionChangedEventArgs>((sender, args) =>
                {
                    guiInstance.UpdateJobComponents(job, ComponentSelector.ComponentMaterials());
                    Summary.Update();
                });
                //ingridPanel.UpdateComponent(job.CraftableItem);
                Add(ComponentSelector);
                QuantitySelector = new QuantitySelect(job, mgr);
                QuantitySelector.Init();
                QuantitySelector.Anchor = Anchors.Top | Anchors.Horizontal;
                QuantitySelector.Top = Summary.Height + ComponentSelector.Height;
                QuantitySelector.Left = 0;
                QuantitySelector.Width = this.Width;
                QuantitySelector.QuantityChanged += new EventHandler<QuantityModeSelectedEventArgs>((sender, args) =>
                {
                    job.Repeat = args.NewMode == QuantityMode.Repeat;
                    job.CraftTo = args.NewMode == QuantityMode.CraftTo ? QuantitySelector.CraftToValue /*TODO: insert number here*/: 0u;
                    Summary.Update();
                });
                Add(QuantitySelector);
                AutocraftPanel = new DetailedJob_AutocraftPanel(guiInstance, job, mgr);
                AutocraftPanel.Init();
                AutocraftPanel.Anchor = Anchors.Top | Anchors.Horizontal;
                AutocraftPanel.Top = Summary.Height + ComponentSelector.Height + QuantitySelector.Height;
                AutocraftPanel.Left = 0;
                AutocraftPanel.Width = this.Width;
                Add(AutocraftPanel);
                //BackColor = Microsoft.Xna.Framework.Color.Red;
                Height = Summary.Height + ComponentSelector.Height + QuantitySelector.Height + AutocraftPanel.Height + 5;


            }
        }
        class JobSummary : JobGuiElement
        {
            public bool mouseOver = false;
            public bool selected = false;
            public bool shortMode = false;
            protected CustomGuiInstance guiInstance;
            protected Label label1;
            protected Label label2;
            protected Button removeButton;
            public JobSummary(CustomGuiInstance guiInstance, WorkshopJob job, Manager mgr)
                : base(job, mgr)
            {
                this.guiInstance = guiInstance;
                label1 = new Label(mgr);
                label1.Init();
                label1.Anchor = Anchors.Top | Anchors.Left;
                label1.Width = 150;
                label1.Top = 2;
                this.Add(label1);

                label2 = new Label(mgr);
                label2.Init();
                label2.Anchor = Anchors.Top | Anchors.Horizontal;
                label2.Left = 150;
                label2.Top = 2;
                label2.Width = 500;
                this.Add(label2);


                if (!job.HasParent)
                {
                    removeButton = new Button(mgr);
                    removeButton.Init();
                    removeButton.Width = 22;
                    removeButton.Height = 22;
                    removeButton.Text = "x";
                    removeButton.Anchor = Anchors.Top | Anchors.Right;
                    removeButton.Left = this.ClientWidth - removeButton.ControlRect.Width - 2;
                    removeButton.Top = 1;
                    removeButton.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                    {
                        guiInstance.Workshop.CancelJob(job);
                        guiInstance.SyncJobGuiWithJobs();
                    });
                    this.Add(removeButton);
                }

                //BackColor = Control.UndefinedColor;
                Anchor = Anchors.Horizontal;
                Height = label1.Height;
                Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                {
                    var selfElement = guiInstance.jobPanel.Elements.Single(el => el.job == job);
                    guiInstance.ToggleJobControl(selfElement);
                });
                Update();
            }
            public void Update()
            {
                label1.Text = CraftableItemText(job.CraftableItem, (Material)job.Materials[0]);
                if (shortMode)
                {
                    label2.Text = "";
                }
                else
                {
                    var parts = new List<String>();
                    if (job.Repeat)
                    {
                        parts.Add("repeat");
                    }
                    else if (job.CraftTo > 0)
                    {
                        parts.Add("craft to " + job.CraftTo);

                        var material = job.CraftableItem.ConversionMaterial == Material.Count ? job.Materials[0] : (int)job.CraftableItem.ConversionMaterial;
                        var cnt = GnomanEmpire.Instance.Fortress.StockManager.QuantityInStock(job.CraftableItem.ItemID, material);
                        if (cnt > 0)
                        {
                            parts.Add("got " + cnt);
                        }
                    }
                    if (job.HasParent)
                    {
                        parts.Add("for " + job.ParentName());
                    }
                    if (job.HasChildJob)
                    {
                        parts.Add("needs " + job.ChildName());
                    }
                    else
                    {
                        var stockManager = GnomanEmpire.Instance.Fortress.StockManager;
                        for (var i = 0; i < job.CraftableItem.Components.Length; i++)
                        {
                            var comp = job.CraftableItem.Components[i];
                            if (!stockManager.AreItemsAvailable(guiInstance.Workshop.CraftPos(), comp.ID, comp.Quantity, job.Materials[i]))
                            {
                                parts.Add("missed " + Item.GroupName(comp.ID));
                                break;
                            }
                        }
                    }
                    label2.Text = parts.Count > 0 ? ("(" + parts.Join("; ") + ")") : "";
                }
            }
            protected override void OnMouseOver(MouseEventArgs e)
            {
                if (!mouseOver)
                {
                    mouseOver = true;
                    Invalidate();
                }
            }
            protected override void OnMouseOut(MouseEventArgs e)
            {
                if (mouseOver)
                {
                    mouseOver = false;
                    Invalidate();
                }
            }
            private bool canBeDrag = false;
            private bool isDrag = false;
            protected override void OnMouseDown(MouseEventArgs e)
            {
                canBeDrag = true;
                isDrag = false;
                base.OnMouseDown(e);
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                canBeDrag = false;
                if (!isDrag)
                {
                    base.OnMouseUp(e);
                }
                else
                {
                    //var prevEl = guiInstance.jobPanel.Elements.ElementBeforeOrDefault(el => el.job == job);
                    //guiInstance.MoveJobToPos(job, prevEl == null ? null : prevEl.job);
                    e.Handled = true;
                }
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (canBeDrag)
                {
                    var currentPrev = guiInstance.jobPanel.Elements.ElementBeforeOrDefault(el => el.job == this.job);
                    JobGuiElement self = guiInstance.jobPanel.Elements.Single(el => el.job == this.job);
                    JobGuiElement prev = null;
                    JobGuiElement curr = null;
                    JobGuiElement next = null;
                    var mouseY = e.Position.Y + self.Top;
                    //label1.Text = mouseY.ToString();
                    var aboveMe = e.Position.Y < 0;
                    /*
                     * curr (mosueover), prev & next
                     */
                    foreach (var ctrl in guiInstance.jobPanel.Elements)
                    {
                        if (curr != null)
                        {
                            next = ctrl;
                            break;
                        }
                        else
                        {
                            if ((ctrl.Top + ctrl.Height) > mouseY)
                            {
                                curr = ctrl;
                            }
                            else
                            {
                                prev = ctrl;
                            }
                        }
                    }
                    if (curr != self)
                    {
                        isDrag = true;
                        if (aboveMe)
                        {
                            guiInstance.jobPanel.MoveElement(self, prev);
                        }
                        else
                        {
                            guiInstance.jobPanel.MoveElement(self, prev = curr ?? prev);
                        }
                        if (prev != currentPrev)
                        {
                            guiInstance.MoveJobToPos(job, prev != null ? prev.job : null);
                        }
                    }
                }
            }
            protected override void DrawControl(Renderer renderer, Microsoft.Xna.Framework.Rectangle rect, Microsoft.Xna.Framework.GameTime gameTime)
            {
                //poor mans mouse over ftw :D
                if (mouseOver)
                {
                    base.DrawControl(renderer, rect, gameTime);
                }
            }
        }


        public WorkshopCraftUI WorkshopCraftUI { get; private set; }
        //CustomListbox<Button> craftable_items_selection;
        public Workshop Workshop { get; private set; }
        CustomListbox<JobGuiElement> jobPanel;

        private void MoveJobToPos(WorkshopJob movedJob, WorkshopJob insertAfter)
        {
            var toMoveIndex = Workshop.JobQueue.IndexOf(movedJob);
            if (toMoveIndex == -1)
            {
                throw new ArgumentException("Moved job is not in currents workshop queue!");
            }
            var trgIndex = Workshop.JobQueue.IndexOf(insertAfter) + 1;
            if (trgIndex > toMoveIndex)
                trgIndex--;
            //workshop.CancelJob(toMoveIndex); don't cancel it, or we may end up with lots other jobs removed as well.
            Workshop.JobQueue.RemoveAt(toMoveIndex);
            Workshop.JobQueue.Insert(trgIndex, movedJob);
            SyncJobGuiWithJobs();//sync can never hurt, only eats time :)
        }
        private void UpdateJobComponents(WorkshopJob jobToUpdate, List<Material> materialsToUse)
        {
            var index = Workshop.JobQueue.IndexOf(jobToUpdate);
            if (index >= 0)
            {
                //hope this works
                Workshop.CancelJob(index);
                jobToUpdate.Materials = materialsToUse.Select(mat => (int)mat).ToList();
                Workshop.JobQueue.Insert(index, jobToUpdate);
            }
            SyncJobGuiWithJobs();
        }
        public CustomGuiInstance(WorkshopCraftUI self, Workshop ws)
        {
            WorkshopCraftUI = self;
            Workshop = ws;
        }
        private JobDetailed ToggleJobControl(JobGuiElement el)
        {
            if (lastSelectedJob != null)
            {
                jobPanel.ReplaceElement(lastSelectedJob, new JobSummary(this, lastSelectedJob.job, WorkshopCraftUI.Manager));
            }
            if (lastSelectedJob == el)
            {
                lastSelectedJob = null;
                return null;
            }
            else
            {
                lastSelectedJob = new JobDetailed(this, el.job, WorkshopCraftUI.Manager);
                jobPanel.ReplaceElement(el, lastSelectedJob);
                return lastSelectedJob;
            }
        }
        private void CreateNewJob(CraftableItem ci)
        {
            WorkshopJob newJob = new WorkshopJob(Workshop, ci, ci.Components.Select(comp => (int)Material.Count).ToList(), ItemQuality.Legendary, true);
            Workshop.JobQueue.Add(newJob);
            var mats = lastSelectedJob == null ? null : lastSelectedJob.ComponentSelector.ComponentMaterials();
            var sumEl = new JobSummary(this, newJob, WorkshopCraftUI.Manager);
            jobPanel.AddElement(sumEl);
            var detEl = ToggleJobControl(sumEl);
            if (mats != null)
            {
                detEl.ComponentSelector.TrySetMaterials(mats);
            }
            SyncJobGuiWithJobs();
        }
        private void SyncJobGuiWithJobs()
        {
            var jobEls = jobPanel.Elements.ToDictionary(el=>el.job);
            var trgJobs = Workshop.JobQueue;
            for (var i = 0; i < trgJobs.Count; i++)
            {
                var trgJob = trgJobs[i];
                var current_index = jobPanel.Elements.IndexOf(el => el.job == trgJob);

                if (current_index >= 0)
                {
                    if (current_index != i)
                    {
                        jobPanel.MoveElement(current_index, i);
                    }
                    jobEls.Remove(trgJob);
                }
                else
                {
                    jobPanel.InsertElement(i, new JobSummary(this, trgJob, WorkshopCraftUI.Manager));
                }
            }
            foreach (var el in jobEls)
            {
                if (el.Value == lastSelectedJob)
                {
                    lastSelectedJob = null;
                }
                jobPanel.RemoveElement(el.Value);
            }
        }
        public void SetupWorkshopInterface()
        {
            /*
             * Collect GUI content
             */
            var blueprintManager = GnomanEmpire.Instance.Fortress.BlueprintManager;
            var entityManager = GnomanEmpire.Instance.EntityManager;
            var workshopDef = entityManager.WorkshopDef(Workshop.WorkshopID);
            var unlockedCraftableItems = workshopDef.CraftableItems.Where(ci => blueprintManager.UnlockedBlueprint(ci.BlueprintID)).ToArray();

            /*
             * Set up GUI elements
             */
            var craftable_label = new Label(WorkshopCraftUI.Manager);
            craftable_label.Text = "Select item to craft:";
            craftable_label.Anchor = Anchors.Top | Anchors.Left;
            craftable_label.Left = craftable_label.Margins.Left;
            craftable_label.Top = craftable_label.Margins.Top;
            craftable_label.Width = 180;
            WorkshopCraftUI.Add(craftable_label);


            var craftable_items_selection = new CustomListbox<Button>(WorkshopCraftUI.Manager);
            craftable_items_selection.Init();
            craftable_items_selection.Anchor = Anchors.Left | Anchors.Vertical;
            craftable_items_selection.Width = craftable_label.Width;
            craftable_items_selection.Height = WorkshopCraftUI.ClientHeight - craftable_label.ClientRect.Bottom - 2;
            craftable_items_selection.Top = craftable_label.ClientRect.Bottom + 2;


            craftable_items_selection.AddElements(
                workshopDef
                    .CraftableItems
                    .Where(ci => blueprintManager.UnlockedBlueprint(ci.BlueprintID))
                    .Select(ci =>
                    {

                        var btn = new Button(WorkshopCraftUI.Manager);
                        btn.Init();
                        btn.Text = CraftableItemText(ci);
                        btn.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                        {
                            CreateNewJob(ci);
                        });
                        var sb = new StringBuilder();
                        if ((ci.Components != null) && (ci.Components.Length > 0))
                        {
                            sb.AppendLine("Materials:");
                            foreach (var comp in ci.Components)
                            {
                                var thisItems = GnomanEmpire.Instance.Region.Fortress.StockManager.ItemsByItemID(comp.ID);
                                sb
                                    .Append("  ")
                                    .Append(comp.Quantity)
                                    .Append("x ")
                                    .Append(Item.GroupName(comp.ID))
                                    .AppendLine(
                                        "  ("
                                        + (
                                            ((thisItems != null) && (thisItems.Count > 0))
                                            ? thisItems
                                                .Where(kvp => kvp.Value != null)
                                                .Select(kvp => kvp.Value.Count)
                                                .Sum()
                                            : 0)
                                        + ")");
                            }
                        }
                        if (ci.RequiredSkillLevel > 0.0) // 1.0 ?
                        {
                            if (sb.Length > 0)
                            {
                                sb.AppendLine().AppendLine();
                            }
                            sb.Append("Required ").Append(entityManager.SkillDef(ci.SkillUsed).Name).Append(": ").Append(ci.RequiredSkillLevel);
                        }
                        if (sb.Length > 0)
                        {
                            btn.ToolTip = new ToolTip(btn.Manager)
                            {
                                Text = sb.ToString()
                            };
                        }

                        return btn;
                    })
                );
            WorkshopCraftUI.Add(craftable_items_selection);



            jobPanel = new CustomListbox<JobGuiElement>(WorkshopCraftUI.Manager);
            jobPanel.Left = craftable_items_selection.ControlRect.Right + 10;
            jobPanel.Width = WorkshopCraftUI.ClientWidth - jobPanel.Left - 5;
            jobPanel.Height = WorkshopCraftUI.ClientHeight;
            jobPanel.Anchor = Anchors.All;
            //jobPanel.BackColor = Microsoft.Xna.Framework.Color.Red;

            SyncJobGuiWithJobs();
            WorkshopCraftUI.Add(jobPanel);


        }
        public void Update(GameTime diff)
        {
            SyncJobGuiWithJobs();
        }
    }
#if SMELTERASWELL
    class CustomSmelterGuiInstance : CustomGuiInstance
    {
        public Smelter Smelter { get; private set; }
        public CustomSmelterGuiInstance(SmelterCraftUI self, Smelter ws): base(self, ws)
        {
            Smelter = ws;
        }
        public void SetupSmelterInterface()
        {
            
            var blueprintManager = GnomanEmpire.Instance.Fortress.BlueprintManager;
            var entityManager = GnomanEmpire.Instance.EntityManager;
            var workshopDef = entityManager.WorkshopDef(Workshop.WorkshopID);
            var unlockedCraftableItems = workshopDef.CraftableItems.Where(ci => blueprintManager.UnlockedBlueprint(ci.BlueprintID)).ToArray();
            /*
             * Set up GUI elements
             */
            var craftable_label = new Label(WorkshopCraftUI.Manager);
            craftable_label.Text = "Select item to smelt:";
            craftable_label.Anchor = Anchors.Top | Anchors.Left;
            craftable_label.Left = craftable_label.Margins.Left;
            craftable_label.Top = craftable_label.Margins.Top;
            craftable_label.Width = 180;
            WorkshopCraftUI.Add(craftable_label);


            var craftable_items_selection = new CustomListbox<Button>(WorkshopCraftUI.Manager);
            craftable_items_selection.Init();
            craftable_items_selection.Anchor = Anchors.Left | Anchors.Vertical;
            craftable_items_selection.Width = craftable_label.Width;
            craftable_items_selection.Height = WorkshopCraftUI.ClientHeight - craftable_label.ClientRect.Bottom - 2;
            craftable_items_selection.Top = craftable_label.ClientRect.Bottom + 2;
            /*
            craftable_items_selection.AddElements(
                workshopDef
                    .CraftableItems
                    .Where(ci => blueprintManager.UnlockedBlueprint(ci.BlueprintID))
                    .Select(ci =>
                    {

                        var btn = new Button(WorkshopCraftUI.Manager);
                        btn.Init();
                        btn.Text = CraftableItemText(ci);
                        btn.Click += new Game.GUI.Controls.EventHandler((sender, args) =>
                        {
                            CreateNewJob(ci);
                        });
                        var sb = new StringBuilder();
                        if ((ci.Components != null) && (ci.Components.Length > 0))
                        {
                            sb.AppendLine("Available to smelt:");
                            foreach (var comp in ci.Components)
                            {
                                sb.Append("  ").Append(comp.Quantity).Append("x ").AppendLine(Item.GroupName(comp.ID));
                            }
                        }
                        //if (ci.RequiredSkillLevel > 1.0)
                        //{
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                            sb.Append("Require ").Append(entityManager.SkillDef(ci.SkillUsed).Name).Append(": ").Append(ci.RequiredSkillLevel);
                        }
                        //}
                        if (sb.Length > 0)
                        {
                            btn.ToolTip = new ToolTip(WorkshopCraftUI.Manager)
                            {
                                Text = sb.ToString()
                            };
                        }
                        return btn;
                    })
                );*/
            WorkshopCraftUI.Add(craftable_items_selection);

        }
    }
#endif

    /// <summary>
    /// Replaces the default interface of Workshops with one that allows your to more easily modify existing jobs.
    /// 
    /// *THIS MOD IS WORK IN PROGRESS*
    /// 
    /// Basic concept: http://forums.gnomoria.com/index.php?topic=3623.0
    /// 
    /// Check those lots of "TODO"s in this file....
    /// </summary>
    public class CustomWorkshopUI : Mod
    {
        public override IEnumerable<IModification> Modifications
        {
            get
            {
                yield return new MethodHook(
                    typeof(WorkshopCraftUI).GetConstructor(new Type[]{ typeof(Manager), typeof(Workshop) }),
                    Method.Of < WorkshopCraftUI, Manager, Workshop>(OnCreate_WorkshopCraftUI)
                    );
                yield return new MethodHook(
                    typeof(WorkshopCraftUI).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic),
                    Method.Of<WorkshopCraftUI, GameTime>(On_WorkshopCraftUI_Update),
                    MethodHookType.RunBefore
                    //does not need skipping, since CraftUI has some kind of "isSetup" check anyway and we need its base
                    );
                yield return new MethodHook(
                    Method.Of(Method.CreateDummy<WorkshopCraftUI>().SetupPanel),
                    Method.Of<WorkshopCraftUI, bool>(On_WorkshopCraftUI_SetupPanel),
                    MethodHookType.RunBefore,
                    MethodHookFlags.CanSkipOriginal
                    );
#if SMELTERASWELL
                yield return new MethodHook(
                    typeof(SmelterCraftUI).GetMethod("SetupPanel", BindingFlags.Instance | BindingFlags.Public),
                    Method.Of<SmelterCraftUI, bool>(On_SmelterCraftUI_SetupPanel),
                    MethodHookType.RunBefore,
                    MethodHookFlags.CanSkipOriginal
                    );
#endif
            }
        }
        public override string Name
        {
            get
            {
                return "Custom Workshop Interface";
            }
        }
        public override string Description
        {
            get
            {
                return "Replaces the default interface of Workshops with one that allows your to more easily modify existing jobs.";
            }
        }
        public override string Author
        {
            get
            {
                // Made mentionable changes? Add yourself!
                return "Faark";
            }
        }

        private static Workshop currentWorkshop;
        private static WorkshopCraftUI currentCraftUI;

        private static List<WeakReference<CustomGuiInstance>> instances = new List<WeakReference<CustomGuiInstance>>();
        private static CustomGuiInstance GetInstance(WorkshopCraftUI ui)
        {
            var list = instances;
            for (var i = 0; i < list.Count;)
            {
                var el = list[i];
                if( !el.IsAlive )
                {
                    list.RemoveAt(i);
                }
                else if (el.Target.WorkshopCraftUI == ui)
                {
                    return el.Target;
                }
                else
                {
                    i++;
                }
            }
            return null;
        }

        public static void On_WorkshopCraftUI_Update(WorkshopCraftUI self, GameTime diff)
        {
            var t = self.GetType();
            if ((t != typeof(WorkshopCraftUI)))// && (t != typeof(SmelterCraftUI)))
            {
                return;
            }
            else
            {
                var inst = GetInstance(self);
                if (inst == null)
                {
                    throw new Exception("This should not be possible.... pls contact a CustomCraftUI-Dev!");
                }
                inst.Update(diff);
            }
        }
        public static void OnCreate_WorkshopCraftUI(WorkshopCraftUI self, Manager mgr, Workshop workshop)
        {
            currentCraftUI = self;
            currentWorkshop = workshop;
        }
        public static bool On_WorkshopCraftUI_SetupPanel(WorkshopCraftUI self)
        {
            if (self.GetType() == typeof(WorkshopCraftUI))
            {
                if (self != currentCraftUI)
                {
                    throw new Exception("This should not be possible.... pls contact a CustomCraftUI-Dev!");
                }
                var instance = new CustomGuiInstance(self, currentWorkshop);
                instance.SetupWorkshopInterface();
                instances.Add(instance);
                return true;
            }
            return false;
        }
#if SMELTERASWELL
        public static bool On_SmelterCraftUI_SetupPanel(SmelterCraftUI self)
        {
            if (self != currentCraftUI)
            {
                throw new Exception("This should not be possible.... pls contact a CustomCraftUI-Dev!");
            }
            var instance = new CustomSmelterGuiInstance(self as SmelterCraftUI, currentWorkshop as Smelter);
            instance.SetupSmelterInterface();
            instances.Add(instance);
            return true;
        }
#endif
    }
#endif
}
