using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GOOColourManager : MonoBehaviour
{
	public struct HSVColour
	{
		public Vector3 value;
	}

	public enum eGOOColours
	{
		Bounce,
		Speed,
		Sticky,
		Electric,
		Count,
		None
	}

	[SerializeField] private List<Color> _gooColours = new List<Color>();

	[SerializeField] private float _colourSensitivity = 0.1f;

	private List<HSVColour> _hsvGooColour = new List<HSVColour>();

	public static GOOColourManager _instance = null;

	void Awake()
	{
		_instance = this;

		// foreach (Color goo in _gooColours)
		// {
		// 	HSVColour colour = new HSVColour();
		// 	Color.RGBToHSV(goo, out colour.value.x, out colour.value.y, out colour.value.z);
		// 	_hsv
		// }
	}

	public bool CheckColour(Color colour, out eGOOColours gooType)
	{
		HSVColour checkingColour = new HSVColour();
		Color.RGBToHSV(colour, out checkingColour.value.x, out checkingColour.value.y, out checkingColour.value.z);

		// foreach(Color goo in _gooColours)
		// {
		// 	HSVColour storedGOO = new HSVColour();
		// 	Color.RGBToHSV(goo, out storedGOO.value.x, out storedGOO.value.y, out storedGOO.value.z);

		// 	if (Mathf.Abs(storedGOO.value.x - checkingColour.value.x) < _colourSensitivity)
		// 	{
		// 		//Debug.Log("GOO check value was " + Mathf.Abs(storedGOO.value.x - checkingColour.value.x));
		// 		Debug.Log("Standing on a GOO!");
		// 		gooType = eGOOColours.Bounce;
		// 		return true;
		// 	}
		// }
		for (int i = 0; i < _gooColours.Count; ++i)
		{
			HSVColour storedGOO = new HSVColour();
			Color.RGBToHSV(_gooColours[i], out storedGOO.value.x, out storedGOO.value.y, out storedGOO.value.z);

			if (Mathf.Abs(storedGOO.value.x - checkingColour.value.x) < _colourSensitivity)
			{
				gooType = (eGOOColours)i;
				return true;
			}
		}
		Debug.Log("Not standing on a GOO!");
		gooType = eGOOColours.None;
		return false;
	}
}
