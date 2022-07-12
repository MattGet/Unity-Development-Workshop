using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomTubes;

namespace CustomTubes
{
    public class ShellTubeMatchData : MonoBehaviour
    {
        public ShellData shellData;

        public struct ShellData
        {
            public int ShellMatchNumber { get; set; }
            public GameObject Shell;
        }
    }



}

