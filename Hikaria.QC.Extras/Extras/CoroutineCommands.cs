#if !QC_DISABLED && !QC_DISABLE_BUILTIN_ALL && !QC_DISABLE_BUILTIN_EXTRA
using BepInEx.Unity.IL2CPP.Utils.Collections;
using System;
using System.Collections;
using UnityEngine;

namespace Hikaria.QC.Extras
{
    public class CoroutineCommands : MonoBehaviour
    {
        [Command("start-coroutine", "starts the supplied command as a coroutine", MonoTargetType.Singleton)]
        private void StartCoroutineCommand(string coroutineCommand)
        {
            object coroutineReturn = QuantumConsoleProcessor.InvokeCommand(coroutineCommand);
            if (coroutineReturn is IEnumerator)
            {
                StartCoroutine((coroutineReturn as IEnumerator).WrapToIl2Cpp());
            }
            else
            {
                throw new ArgumentException($"{coroutineCommand} is not a coroutine");
            }
        }
    }
}
#endif
