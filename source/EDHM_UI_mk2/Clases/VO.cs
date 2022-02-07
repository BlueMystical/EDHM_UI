using System;
using System.Collections.Generic;
using System.Drawing;

namespace EDHM_UI_mk2
{
	[Serializable]
	public class player_loadout
	{
		public player_loadout() { }

		public bool theme_swaping { get; set; }
		public string player_name { get; set; }
		public string active_instance { get; set; }		
		public List<ship_loadout> ships { get; set; }
	}
	[Serializable]
	public class ship_loadout
	{
		public ship_loadout() { }

		public string ship_short_type { get; set; }
		public string ship_full_tye { get; set; }
		public string ship_name { get; set; }
		public string ship_id { get; set; }
		public string theme { get; set; }

		public override string ToString()
		{
			return String.Format("{0} ({1} {2})", ship_short_type.ToUpper(), ship_name, ship_id);
		}
	}


	[Serializable]
	public class game_instance
	{
		public game_instance() { }

		public string instance { get; set; } = "Default";
		public string game_id { get; set; }

		public string key { get; set; }
		public string name { get; set; }
		public string path { get; set; }
		
		public string themes_folder { get; set; }
		public bool is_active { get; set; } = false;

		public override string ToString()
		{
			return String.Format("{0} ({1})", name, instance);
		}
	}

	[Serializable]
	public class GameInstance
	{
		public GameInstance() { }

		public string instance { get; set; }
		public List<game_instance> games { get; set; }
	}

	[Serializable]
	public class ui_preset_new : ICloneable
	{
		public ui_preset_new() { }
		public ui_preset_new(string Name, string Folder, string Author = "")
		{
			this.name = Name;
			this.folder = Folder;
			this.author = Author;
		}

		public string name { get; set; }
		public string folder { get; set; }
		public string author { get; set; }

		public Image Preview { get; set; }
		public bool HasPreview { get; set; }
		public bool IsFavorite { get; set; }

		public object Clone()
		{
			return (ui_preset_new)this.MemberwiseClone();
		}

		public override string ToString()
		{
			return this.name;
		}
	}

	[Serializable]
	public class combo_item : ICloneable
	{
		public combo_item() { }
		public combo_item(string _Type, string _Name, decimal _Index)
		{
			this.Type = _Type;
			this.Name = _Name;
			this.Index = _Index;
		}

		public string Type { get; set; }
		public string Name { get; set; }
		public decimal Index { get; set; }

		public object Clone()
		{
			return (combo_item)MemberwiseClone();
		}
		public override string ToString()
		{
			return string.Format("[{0}]: '{1}'", this.Type, this.Name);
		}
	}

	[Serializable]
	public class ui_setting : ICloneable
	{
		public ui_setting() { }

		public string language { get; set; }
		public string game { get; set; }
		public string version { get; set; }

        public string name { get; set; }
        public string author { get; set; }

		public List<ui_group> ui_groups { get; set; }
		public List<combo_item> Presets { get; set; }
		public List<value_key> xml_profile { get; set; }

		public object Clone()
		{
			return (ui_setting)MemberwiseClone();
		}
	}

	[Serializable]
	public class value_key : ICloneable
	{
		public value_key() { }
		public value_key(string _Key, decimal _Value)
		{
			this.key = _Key;
			this.value = _Value;
		}

		public string key { get; set; }
		public decimal value { get; set; }

		public object Clone()
		{
			return (value_key)MemberwiseClone();
		}
	}

	[Serializable]
	public class ui_group : ICloneable
	{
		public ui_group() { }
		public ui_group(string pName, string pTitle)
		{
			this.Name = pName;
			this.Title = pTitle;
		}

		public string Name { get; set; }
		public string Title { get; set; }
		public List<element> Elements { get; set; }

		public object Clone()
		{
			return (ui_group)MemberwiseClone();
		}
		public override string ToString()
		{
			return this.Title;
		}
	}	

	[Serializable]
	public class element : ICloneable
	{
		public element() { }

		
		public string Category { get; set; }
		public string Title { get; set; }
		public string File { get; set; }

		public string Section { get; set; } = "Constants";
		public string Key { get; set; }		
		public decimal Value { get; set; }

		public string ValueType { get; set; }
		public string Type { get; set; }

		public string Description { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public string Parent { get; set; }

		public object Clone()
		{
			return (element)MemberwiseClone();
		}
		public override string ToString()
		{
			return string.Format("{0}: {1}", this.Title, this.Value);
		}
	}

	[Serializable]
	public class key
	{
		public key() { }

		public string Category { get; set; }
		public string Name { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return string.Format("Name: {0}, Value: {1}", this.Name, this.Value);
		}
	}


	[Serializable]
	public class TPMod_Config
	{
		public TPMod_Config() { }

		public string mod_name { get; set; }
		public string author { get; set; }
		public string description { get; set; }

		public string file { get; set; }
		public string file_full { get; set; }

		public bool managed { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public Image Thumbnail { get; set; }

		public List<TPMod_Section> sections { get; set; }
		public List<TPMod_Type> types { get; set; }
		public List<TPMod_Type> custom_types { get; set; }
	}
	[Serializable]
	public class TPMod_Section
	{
		public TPMod_Section() { }
		public TPMod_Section(string _Name = "", string _Title = "")
		{
			this.name = _Name;
			this.title = _Title;
		}

		public string name { get; set; }
		public string title { get; set; }
		public string ini_section { get; set; }

		public List<TPMod_Key> keys { get; set; }		
	}
	[Serializable]
	public class TPMod_Key
	{
		public TPMod_Key() { }

		public string name { get; set; }
		public string type { get; set; }
		public string key { get; set; }		
		public string value { get; set; }
		public string description { get; set; }
	}
	[Serializable]
	public class TPMod_Type
	{
		public TPMod_Type() { }
		public TPMod_Type(string _Type = "", string _Name = "", string _Value = "")
		{
			this.type = _Type;
			this.name = _Name;
			this.value = _Value;
		}
				
		public string type { get; set; }
		public string name { get; set; }
		public string value { get; set; }
	}


	[Serializable]
	public class UI_Tips
	{
		public UI_Tips() { }

		public string Name { get; set; }
		public string Title { get; set; }
		public string Language { get; set; }
		public List<string> Elements { get; set; }
	}

	[Serializable]
	public class file_job
	{
		public file_job() { }

		public string game { get; set; }
		public string file_path { get; set; }
		public string action { get; set; }

		public string destination { get; set; }
	}
}
