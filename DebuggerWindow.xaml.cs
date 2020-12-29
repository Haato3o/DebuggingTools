using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;
using DebuggingTool.Controls;
using Newtonsoft.Json;
using HunterPie.GUI;

namespace DebuggingTool
{
    /// <summary>
    /// Interaction logic for DebuggerWindow.xaml
    /// </summary>
    public partial class DebuggerWindow : Widget
    {

        public string DataText
        {
            get { return (string)GetValue(DataTextProperty); }
            set { SetValue(DataTextProperty, value); }
        }
        public static readonly DependencyProperty DataTextProperty =
            DependencyProperty.Register("DataText", typeof(string), typeof(DebuggerWindow));

        Game game;

        public DebuggerWindow()
        {
            InitializeComponent();
        }

        private void OnRender(object sender, EventArgs e)
        {
            if (dataTreeView.SelectedItem is CustomItem)
            {
                CustomItem selected = dataTreeView.SelectedItem as CustomItem;
                
                if (selected.AilmentData != null)
                {
                    DataText = JsonConvert.SerializeObject(selected.AilmentData.cMonsterAilment, Formatting.Indented);
                } else if (selected.PartData != null)
                {
                    DataText = JsonConvert.SerializeObject(selected.PartData.cMonsterPartData, Formatting.Indented);
                } else if (selected.MonsterData != null)
                {
                    DataText = JsonConvert.SerializeObject(new MonsterFilteredData(selected.MonsterData), Formatting.Indented);
                }
                
            }
        }

        internal void SetGameContext(Game ctx)
        {
            game = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            foreach (Monster m in game.Monsters)
            {
                m.OnMonsterSpawn += OnMonsterSpawn;
                m.OnMonsterAilmentsCreate += OnAilmentsCreate;
                m.OnMonsterDespawn += OnMonsterDespawn;
            }
        }

        internal void UnhookEvents()
        {
            foreach (Monster m in game.Monsters)
            {
                m.OnMonsterSpawn -= OnMonsterSpawn;
                m.OnMonsterAilmentsCreate -= OnAilmentsCreate;
            }
        }

        private void OnAilmentsCreate(object source, EventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                TreeViewItem parentItem = null;
                for (int i = 0; i < dataTreeView.Items.Count; i++)
                {
                    if ((dataTreeView.Items[i] as TreeViewItem).Header.ToString().StartsWith(m.MonsterNumber.ToString()))
                        parentItem = dataTreeView.Items[i] as TreeViewItem;
                }

                if (parentItem is null)
                {
                    return;
                }

                TreeViewItem ailmParent = parentItem.Items.Cast<TreeViewItem>().Where(p => p.Header.ToString() == "Ailments").First();

                foreach (Ailment ailment in m.Ailments)
                {
                    CustomItem item = new CustomItem
                    {
                        Header = $"{ailment.Id} [{ailment.Name}]",
                        AilmentData = ailment,
                        FontWeight = FontWeights.Normal
                    };
                    parentItem.Items.Add(item);
                }
            });
            
        }

        private void OnMonsterSpawn(object source, MonsterSpawnEventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                CustomItem parentItem = new CustomItem
                {
                    Header = $"{m.MonsterNumber} [{m.Name}]",
                    MonsterData = m,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                TreeViewItem partParent = new TreeViewItem
                {
                    Header = "Parts",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                TreeViewItem ailmParent = new TreeViewItem
                {
                    Header = "Ailments",
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.WhiteSmoke
                };

                foreach (Part part in m.Parts)
                {
                    CustomItem partItem = new CustomItem
                    {
                        Header = part.Name,
                        PartData = part,
                        FontWeight = FontWeights.Normal,
                        Foreground = Brushes.WhiteSmoke
                    };
                    partParent.Items.Add(partItem);
                }

                parentItem.Items.Add(partParent);
                parentItem.Items.Add(ailmParent);

                if (m.Ailments.Count > 0)
                {
                    foreach (Ailment ailment in m.Ailments)
                    {
                        CustomItem ailmItem = new CustomItem
                        {
                            Header = ailment.Name,
                            AilmentData = ailment,
                            FontWeight = FontWeights.Normal,
                            Foreground = Brushes.WhiteSmoke
                        };
                        ailmParent.Items.Add(ailmItem);
                    }
                }

                dataTreeView.Items.Add(parentItem);
            });
        }

        private void OnMonsterDespawn(object source, EventArgs args)
        {
            Monster m = (Monster)source;

            Dispatch(() =>
            {
                TreeViewItem parentItem = null;
                for (int i = 0; i < dataTreeView.Items.Count; i++)
                {
                    if ((dataTreeView.Items[i] as TreeViewItem).Header.ToString().StartsWith(m.MonsterNumber.ToString()))
                        parentItem = dataTreeView.Items[i] as TreeViewItem;
                }

                if (parentItem is null)
                {
                    return;
                }

                parentItem.Items.Clear();
                dataTreeView.Items.Remove(parentItem);
            });
            
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            CompositionTarget.Rendering += OnRender;
        }

        private void Dispatch(Action f) =>
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, f);
    }
}
