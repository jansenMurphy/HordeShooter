using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Float PlayerManager Event", menuName = "Game Events/Float PlayerManager Event")]
public class FloatPlayerManagerGameEvent : BaseGameEvent<(float,PlayerManager)> { }