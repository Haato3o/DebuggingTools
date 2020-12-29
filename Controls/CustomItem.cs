using System.Windows.Controls;
using HunterPie.Core.Definitions;
using HunterPie.Core;
using HunterPie.Core.Enums;

namespace DebuggingTool.Controls
{
    public class CustomItem : TreeViewItem
    {
        public object Data { get; set; }
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
    public class PlayerFilteredData
    {
        public string Name { get; }
        public long SteamId { get; }
        public string Zone { get; }
        public float Health { get; }
        public float MaxHealth { get; }
        public float Stamina { get; }
        public float MaxStamina { get; }
        public Classes Weapon { get; }
        public Vector3 Position { get; }
        public string Action { get; }

        public PlayerFilteredData(Player p)
        {
            Name = p.Name;
            SteamId = p.SteamID;
            Zone = p.ZoneName;
            Health = p.Health.Health;
            MaxHealth = p.Health.MaxHealth;
            Stamina = p.Stamina.Stamina;
            MaxStamina = p.Stamina.MaxStamina;
            Weapon = (Classes)p.WeaponID;
            Position = p.Position;
            Action = p.PlayerActionRef;
        }
    }
}
