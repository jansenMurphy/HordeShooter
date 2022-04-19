using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Float Game Object Event", menuName = "Game Events/Float Game Object Event")]
public class FloatGameObjectGameEvent : BaseGameEvent<(float,GameObject)> { }