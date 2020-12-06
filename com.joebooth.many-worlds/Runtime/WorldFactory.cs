using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManyWorlds
{
    public class WorldFactory : MonoBehaviour
    {
        public Factory Factory;
        public bool DebugSkipPopUp;
        bool showPopUp = false;
        static int envIdIdex = -1;
        string[] envIds;
        int heightRequirments;
        int fontSize;

        void Awake()
        {
            envIds = Factory.spawnableEnvDefinitions.Select(x => x.envId).ToArray();
            fontSize = 26;
            if (Screen.height < 720)
                fontSize /= 2;
            heightRequirments = (fontSize + 4) * (envIds.Length + 1);
            if (envIdIdex == -1)
            {
                var envId = GetEnvId();
                var envDef = Factory.spawnableEnvDefinitions.FirstOrDefault(x => x.envId == envId);
                envIdIdex = Factory.spawnableEnvDefinitions.IndexOf(envDef);
            }

            showPopUp = true;
        }

        /// <summary>
        /// Return the envId to spawn.
        /// </summary>
        string GetEnvId()
        {
            // try get from command line
            List<string> commandLineArgs = new List<string>(System.Environment.GetCommandLineArgs());
            var entry = commandLineArgs.FirstOrDefault(x => x.ToLowerInvariant().StartsWith("--spawn-env"));
            if (entry != null)
            {
                string value = string.Empty;
                if (entry.Contains("="))
                    value = entry.Split('=')[1];
                else
                    value = commandLineArgs[commandLineArgs.IndexOf(entry) + 1];
                print("-----------------");
                print($"--spawn-env:{value}");
                return value;
            }
            return Factory.envIdDefault;
        }
        /// <summary>
        /// Return the number of environments to spawn.
        /// </summary>
        public int GetNumEnvironments()
        {
            // try get from command line
            List<string> commandLineArgs = new List<string>(System.Environment.GetCommandLineArgs());
            var entry = commandLineArgs.FirstOrDefault(x => x.ToLowerInvariant().StartsWith("--num-spawn-envs"));
            if (entry != null)
            {
                string value = string.Empty;
                if (entry.Contains("="))
                    value = entry.Split('=')[1];
                else
                    value = commandLineArgs[commandLineArgs.IndexOf(entry) + 1];
                int numEnvs;
                if (int.TryParse(value, out numEnvs))
                {
                    print("-----------------");
                    print($"--num-spawn-envs:{numEnvs}");
                    return numEnvs;
                }
            }
            return !Academy.Instance.IsCommunicatorOn ? Factory.inferenceNumEnvsDefault : Factory.trainingNumEnvsDefault;
        }        
        bool ShouldInitalizeOnAwake()
        {
            if (DebugSkipPopUp)
                return true;
            if (IsTrainingMode())
                return true;
            if (GetComponent<WorldFactory>() == null)
                return true;
            return false;
        }
                /// <summary>
        /// Return if training mode.
        /// </summary>
        bool IsTrainingMode()
        {
            return Academy.Instance.IsCommunicatorOn;
        }

        // Update is called once per frame
        void Update()
        {
            if (showPopUp)
            {
                var oldEnvIdIdex = envIdIdex;
        		if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                        envIdIdex--;
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                        envIdIdex++;
                    envIdIdex = Mathf.Clamp(envIdIdex, 0, Factory.spawnableEnvDefinitions.Count - 1);
                    if (Input.GetKeyDown(KeyCode.Return))
                        Go();
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("`") || Input.GetKeyDown(KeyCode.Delete))
            {
                StartCoroutine(ReloadScene());
            }
        }
        IEnumerator ReloadScene()
        {
            var activeScene = SceneManager.GetActiveScene().buildIndex;
            Academy.Instance.Dispose();
            yield return SceneManager.LoadSceneAsync(activeScene, LoadSceneMode.Single);
        }

        void OnGUI()
        {
            if (showPopUp)
            {
                int height = heightRequirments + 38;
                int yPos = (Screen.height / 2) - (height / 2);
                GUI.skin.window.fontSize = fontSize;
                // GUI.skin.window.margin.top = fontSize;
                GUI.Window(0, new Rect((Screen.width / 2) - 200, yPos
                    , 400, height), ShowGUI, "Select Environment");
            }
        }

        void ShowGUI(int windowID)
        {
            GUI.skin.toggle.fontSize = fontSize;
            Rect rect = new Rect(5, fontSize + 4, 400, fontSize + 4);
            for (int i = 0; i < envIds.Length; i++)
            {
                bool active = envIdIdex == i;
                GUI.SetNextControlName(envIds[i]);
                if (GUI.Toggle(rect, active, envIds[i]))
                    envIdIdex = i;
                rect.y += rect.height;
            }

            rect.x = 400 - 90;
            rect.width = 75;
            rect.height = 30;
            GUI.SetNextControlName("GO");
            if (GUI.Button(new Rect(rect), "GO"))
                Go();
            GUI.FocusControl(Factory.spawnableEnvDefinitions[envIdIdex].envId);
            if (ShouldInitalizeOnAwake())
                Go();            
        }
        void Go()
        {
            showPopUp = false;
            Factory.envIdDefault = envIds[envIdIdex];
            var spawnPrefab = Factory.GetPrefabFor(GetEnvId());
            Factory.SpawnSpawnableEnv(this.gameObject, GetNumEnvironments() ,spawnPrefab);
        }
    }
}
