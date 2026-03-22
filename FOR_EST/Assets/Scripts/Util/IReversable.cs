using System.Runtime.InteropServices;
using UnityEngine;

public interface IReversable
{
    // Position, Scale, Gravity 음수 전환
    public void Reverse();
}
