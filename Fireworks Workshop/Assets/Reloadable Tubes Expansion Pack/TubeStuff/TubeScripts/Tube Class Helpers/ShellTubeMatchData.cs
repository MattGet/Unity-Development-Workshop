using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomTubes;

namespace CustomTubes
{
    /// <summary>
    /// Contains the shell data used to match a shell in the proper tube on blueprint load.
    /// </summary>
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

