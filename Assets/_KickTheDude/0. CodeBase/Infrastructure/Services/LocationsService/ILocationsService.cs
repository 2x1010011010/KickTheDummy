using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocationsService
{
    void LoadLocationByID(LocationID locationID);
    void ReloadLocation();
}
