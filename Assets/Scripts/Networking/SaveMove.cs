using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveMove : MonoBehaviour
{
    public PlayerManager pm;

    Vector2 xz;
    bool jump, fire;

    public List<Move> moves = new List<Move>();

    private void Start()
    {
        pm.moveDelegate += (cx) =>
        {
            xz = cx.ReadValue<Vector2>();
        };
        pm.jumpDelegate += (cx) =>
        {
            jump = cx.ReadValueAsButton();
        };
    }

    private void LateUpdate()
    {
        moves.Add(new Move() { xz = xz, jump=jump,time = Time.timeSinceLevelLoad});
    }

    public struct Move
    {
        public Vector2 xz;
        public bool jump;
        public float time;
    }

}
