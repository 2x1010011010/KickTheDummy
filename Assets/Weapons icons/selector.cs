using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace selector
{


public class selector : MonoBehaviour
{

	public Button leftButton, rightButton;
	[SerializeField]
	Assault_Rifles[] assault_rifles;
	[SerializeField]
	Explosives[] explosives;
	[SerializeField]
	Handguns[] handguns;
	[SerializeField]
	Launchers_Specials[] launchers_specials;
	[SerializeField]
	Machine_Guns[] machine_guns;
	[SerializeField]
	Sniper_Rifles[] sniper_rifle;
	[SerializeField]
	Shotguns[] shotguns;
	[SerializeField]
	Submachine_Guns[] submachine_guns;

	private void Start()
	{
		foreach (Assault_Rifles a in assault_rifles)
		{
			weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));

		}
		size_multiply = 5;
		ChangeIcon(0);
		ChangeDescription(weapons[0].text);
		leftButton.interactable = false;
	}


	[Serializable]
	public class Weapons
	{
		public Sprite image;
		public string text;
		public Weapons(Sprite image, string st)
		{
			this.image = image; text = st;
		}
	}
	[SerializeField]
	public List<Weapons> weapons = new List<Weapons>();
	public RawImage icon;
	public int icon_number;
	float size_multiply;
	public UnityEngine.UI.Text icon_description;
	public void SetIcon(Dropdown w_name)
	{
	weapons.Clear();
	weapons = new List<Weapons>();
	Debug.Log(  (w_name.options[w_name.value]).text );
		icon_number = 0;
		switch ( (w_name.options[w_name.value]).text)
		{
			case "Assault_Rifles":
				foreach(Assault_Rifles a in assault_rifles)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 5;
				}
				break;
			case "Explosives":
				foreach(Explosives a in explosives)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 3;
				}
				break;
			case "Handguns":
				foreach(Handguns a in handguns)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 5;
				}
				break;
			case "Launchers_Specials":
				foreach(Launchers_Specials a in launchers_specials)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 3f;
				}
				break;
			case "Machine_Guns":
				foreach(Machine_Guns a in machine_guns)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 2;
				}
				break;
			case "Sniper_Rifles":
				foreach(Sniper_Rifles a in sniper_rifle)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 5;
				}
				break;
			case "Shotguns":
				foreach(Shotguns a in shotguns)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 5;
				}
				break;
			case "Submachine_Guns":
				foreach(Submachine_Guns a in submachine_guns)
				{
					weapons.Add(new Weapons(a.image, a.image.name.Remove(a.image.name.Length-8)));
					size_multiply = 3.5f;
				}
				break;
		}
		ChangeIcon(0);
		ChangeDescription(weapons[0].text);
		rightButton.interactable = true;
		leftButton.interactable=false;
	}

	void ChangeIcon(int number)
	{
		icon.texture = weapons[number].image.texture;
		icon.rectTransform.sizeDelta = new Vector2(weapons[number].image.bounds.size.x, weapons[number].image.bounds.size.y)*size_multiply;
	}

	void ChangeDescription(string description)
	{
		icon_description.text = description;
	}

	public void SelectLeft()
	{
		if (icon_number>0)
		{

			leftButton.interactable = true;
			rightButton.interactable = true;
			ChangeIcon(icon_number-1);
			icon_number--;
			if(icon_number<=0)
				leftButton.interactable = false;
			ChangeDescription(weapons[icon_number].text);
		}
		
	}

	public void SelectRight()
	{
		if (icon_number < weapons.Count-1)
		{

			rightButton.interactable = true;
			leftButton.interactable = true;
			ChangeIcon(icon_number+1);
			icon_number++;
			if(icon_number >=weapons.Count-1)
				rightButton.interactable = false;
			ChangeDescription(weapons[icon_number].text);
		}
	}

	[Serializable]
	public class Assault_Rifles
	{
		public Sprite image;
	}
	[Serializable]
	public class Explosives
	{
		public Sprite image;
	}
	[Serializable]
	public class Handguns
	{
		public Sprite image;
	}
	[Serializable]
	public class Launchers_Specials
	{
		public Sprite image;
	}
	[Serializable]
	public class Machine_Guns
	{
		public Sprite image;
	}
	[Serializable]
	public class Shotguns
	{
		public Sprite image;
	}
	[Serializable]
	public class Sniper_Rifles
	{
		public Sprite image;
	}
	[Serializable]
	public class Submachine_Guns
	{
		public Sprite image;
	}
	

}


}