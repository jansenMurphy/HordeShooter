using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New StringIntInt Event", menuName = "Game Events/StringIntInt Event")]
public class StringIntIntGameEvent : BaseGameEvent<(string, int, int)> { }