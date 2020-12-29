using System.Windows.Controls;
using HunterPie.Core.Definitions;
using HunterPie.Core;

namespace DebuggingTool.Controls
{
    public class CustomItem : TreeViewItem
    {
        public Monster MonsterData { get; set; }
        public Part PartData { get; set; }
        public Ailment AilmentData { get; set; }
    }
    public class MonsterFilteredData
    {
        public string Name { get; }
        public string Em { get; }
        public int Id { get; }
        public float Health { get; }
        public float MaxHealth { get; }
        public float Stamina { get; }
        public float MaxStamina { get; }
        public string Action { get; }
        public sMonsterModelData ModelData { get; }

        public MonsterFilteredData(Monster m)
        {
            Name = m.Name;
            Em = m.Id;
            Id = m.GameId;
            Health = m.Health;
            MaxHealth = m.MaxHealth;
            Stamina = m.Stamina;
            MaxStamina = m.MaxStamina;
            Action = m.ActionReferenceName;
            ModelData = m.ModelData;
        }
    }
}
