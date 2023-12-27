using EDHM_UI_mk2.Clases;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace EDHM_UI_mk2
{
	[Serializable]
	public class ship
	{
		public ship() { }

		public string ship_full_name { get; set; }
		public int ship_id { get; set; }
		public string ed_short { get; set; }
		public string size { get; set; }
		public string role { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public byte[] image { get; set; }

		/// <summary>'true' si la nave es diferente a la actual</summary>
		/// <param name="ShipShortName"></param>
		public bool NaveCambiada(string ShipShortName)
		{
			bool _ret = false;
			if (ShipShortName != null && ShipShortName != string.Empty)
			{
				_ret = (ShipShortName.ToLower() != ed_short.ToLower()) ? true : false;
			}
			return _ret;
		}

		public override string ToString()
		{
			return string.Format("{0} ({1})", ship_id, ship_full_name);
		}
	}


	[Serializable]
	public class theme_details
	{
		public theme_details() { }

		public string theme { get; set; }
		public string author { get; set; }
		public string description { get; set; }
		public string preview { get; set; }

		public List<string> color { get; set; }
	}

	[Serializable]
	public class ui_translation
	{
		public ui_translation() { }

		public string id { get; set; }
		public string group { get; set; }

		public List<language> lang { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", id, group);
		}
	}
	[Serializable]
	public class language
	{
		public language() { }
		public language(string _Key, string _Value)
		{
			key = _Key;
			value = _Value;
		}

		public string key { get; set; }
		public string value { get; set; }
		public string category { get; set; }
		public string description { get; set; }


		public object Clone()
		{
			return (key_value_ex)MemberwiseClone();
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", key, value);
		}
	}

	#region Old Shipyard [DELETE]

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

	#endregion

	#region New Shipyard

	[Serializable]
	public class ShipyardEx
	{
		public ShipyardEx() { }

		public bool enabled { get; set; }
		public string player_name { get; set; }
		public string active_instance { get; set; }

		public List<ship_loadout_ex> ships { get; set; }
	}

	[Serializable]
	public class ship_loadout_ex
	{
		public ship_loadout_ex() { }
		public ship_loadout_ex(List<ship> ShipList, string ed_short_id)
		{
			if (ShipList != null && ShipList.Count > 0)
			{
				this.Ship = ShipList.Find(x => x.ed_short == ed_short_id.ToLower());
				this.theme = string.Empty;
				if (Ship != null && Ship.image != null)
				{
					this.Preview = (Bitmap)Util.byteArrayToImage(Ship.image);
				}				
			}
		}

		public ship Ship { get; set; }

		public string ship_name { get; set; }
		public string ship_plate { get; set; }
		public string theme { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public Bitmap Preview { get; set; }

		/// <summary>'true' si la nave es diferente a la actual</summary>
		/// <param name="ShipShortName"></param>
		public bool IsDifferentShip(string ShipShortName, string ShipIdent)
		{
			bool _ret = false;
			if (!string.IsNullOrEmpty(ShipShortName) && Ship != null)
			{
				if (ShipShortName.ToLower() != Ship.ed_short.ToLower()
				 || ShipIdent.ToLower() != this.ship_plate.ToLower())
				{
					//_ret = (ShipShortName.ToLower() != Ship.ed_short.ToLower()) ? true : false;
					_ret = true;
				}				
			}
			return _ret;
		}
	}

	#endregion

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
			name = Name;
			folder = Folder;
			author = Author;
		}

		public string name { get; set; }
		public string folder { get; set; }
		public string author { get; set; }
		public string description { get; set; }

		/// <summary>This is a Link to the Big Preview Image</summary>
		public string BigPreview { get; set; }

		public Image Preview { get; set; }
		public bool HasPreview { get; set; }
		public bool IsFavorite { get; set; }

		public object Clone()
		{
			return (ui_preset_new)MemberwiseClone();
		}

		public override string ToString()
		{
			return name;
		}
	}

	[Serializable]
	public class combo_item : ICloneable
	{
		public combo_item() { }
		public combo_item(string _Type, string _Name, decimal _Index)
		{
			Type = _Type;
			Name = _Name;
			Index = _Index;
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
			return string.Format("[{0}]: '{1}'", Type, Name);
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
		public List<key_value_ex> menus { get; set; }

		public object Clone()
		{
			return (ui_setting)MemberwiseClone();
		}
	}


	[Serializable]
	public class ui_group : ICloneable
	{
		public ui_group() { }
		public ui_group(string pName, string pTitle)
		{
			Name = pName;
			Title = pTitle;
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
			return Title;
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
			return string.Format("{0}: {1}", Title, Value);
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
			return string.Format("Name: {0}, Value: {1}", Name, Value);
		}
	}


	[Serializable]
	public class TPMod_Config
	{
		public TPMod_Config() { }

		public string mod_name { get; set; }
		public string theme_name { get; set; }
		public string author { get; set; }
		public string description { get; set; }

		/// <summary>Version of the 3PMod</summary>
		public string version { get; set; } = "1.0.0.0";

		/// <summary>URL of a JSON file having the latest version info.</summary>
		public string update_url { get; set; } = string.Empty;

		/// <summary>Configuration File (Ini or XML) that this Mod edits.</summary>
		public string file { get; set; }

		/// <summary>Complete Path (with file and Extension) to this JSON file.</summary>
		public string file_full { get; set; }

		/// <summary></summary>
		public string read_me { get; set; }

		/// <summary>Managed by the UI (has a JSON) or not.</summary>
		public bool managed { get; set; }

		/// <summary>INIConfig, XMLConfig</summary>
		public string mod_type { get; set; } = "INIConfig";

		[Newtonsoft.Json.JsonIgnore] public Image Thumbnail { get; set; }

		/// <summary>Folder path where this mod lives.</summary>
		[Newtonsoft.Json.JsonIgnore] public string root_folder { get; set; }

		/// <summary>'true' if this mod has 'child' mods.</summary>
		[Newtonsoft.Json.JsonIgnore] public bool IsRootMod { get; set; } = false;

		public List<TPMod_Section> sections { get; set; }
		public List<TPMod_Type> types { get; set; }
		public List<TPMod_Type> custom_types { get; set; }

		/// <summary>Information about Updates.</summary>
		[Newtonsoft.Json.JsonIgnore] public TPMod_UpdateInfo update_info { get; set; }

		/// <summary>List of files, besides the basic ones, dependant of this mod. ejem: ShaderFixes. 
		/// <para>Files on this list will be removed when uninstall.</para></summary>
		public List<string> dependencies { get; set; }

		public override string ToString()
		{
			return string.Format("{0} (v{1})", mod_name, version);
		}
	}
	[Serializable]
	public class TPMod_Section : ICloneable
	{
		public TPMod_Section() { }
		public TPMod_Section(string _Name = "", string _Title = "")
		{
			name = _Name;
			title = _Title;
		}

		public string name { get; set; }
		public string title { get; set; }

		/// <summary>Secion name in the Config File (ini or xml)</summary>
		public string ini_section { get; set; }

		/// <summary>Overrides the Mod File (ini or xml), only for this section.</summary>
		public string file_override { get; set; }

		public List<TPMod_Key> keys { get; set; }

		/// <summary>Permite Copiar por Valor el Objeto con todas sus propiedades y atributos.</summary>
		public object Clone()
		{
			return (TPMod_Section)MemberwiseClone();
		}
	}
	[Serializable]
	public class TPMod_Key : ICloneable
	{
		public TPMod_Key() { }

		public string name { get; set; }
		public string type { get; set; }
		public string key { get; set; }
		public string value { get; set; }
		public bool visible { get; set; } = true;
		public string description { get; set; }

		/// <summary>Secion name in the Config File (ini or xml)</summary>
		[Newtonsoft.Json.JsonIgnore] public string section_name { get; set; }

		/// <summary>Name of the section who owns this key</summary>
		[Newtonsoft.Json.JsonIgnore] public TPMod_Section root_section { get; set; }

		/// <summary>Permite Copiar por Valor el Objeto con todas sus propiedades y atributos.</summary>
		public object Clone()
		{
			return (TPMod_Key)MemberwiseClone();
		}
	}
	[Serializable]
	public class TPMod_Type
	{
		public TPMod_Type() { }
		public TPMod_Type(string _Type = "", string _Name = "", string _Value = "")
		{
			type = _Type;
			name = _Name;
			value = _Value;
		}

		public string type { get; set; }
		public string name { get; set; }
		public string value { get; set; }
	}

	[Serializable]
	public class TPMod_UpdateInfo
	{
		public TPMod_UpdateInfo() { }
		public TPMod_UpdateInfo(TPMVersionInfo pInfo)
		{
			info = pInfo;
		}

		public TPMVersionInfo info { get; set; }
		public bool is_update { get; set; } = false;
		public bool not_installed { get; set; } = false;
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

	[Flags]
	public enum StatusFlags
	{
		Docked_Landing_pad = 1,
		Landed_Planet_surface = 2,
		Landing_Gear_Down = 4,
		Shields_Up = 8,
		Supercruise = 16,
		FlightAssist_Off = 32,
		Hardpoints_Deployed = 64,
		In_Wing = 128,
		LightsOn = 256,
		Cargo_Scoop_Deployed = 512,
		Silent_Running = 1024,
		Scooping_Fuel = 2048,
		Srv_Handbrake = 4096,
		Srv_Turret_View = 8192,
		Srv_Turret_Retracted = 16384,
		Srv_DriveAssist = 32768,
		Fsd_MassLocked = 65536,
		Fsd_Charging = 131072,
		Fsd_Cooldown = 262144,
		Low_Fuel = 524288,
		Over_Heating = 1048576,
		Has_Lat_Long = 2097152,
		IsInDanger = 4194304,
		Being_Interdicted = 8388608,
		In_MainShip = 16777216,
		In_Fighter = 33554432,
		In_SRV = 67108864,
		Hud_Analysis_Mode = 134217728,
		Night_Vision_ON = 268435456,
		Altitude_Average_Radius = 536870912
		//fsd_jump = 1073741824??
	};
}
