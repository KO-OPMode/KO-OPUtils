using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace KOOP.Utils
{
    public static class PlasticUtils
    {
        public static bool TryGetCurrentChangesetNumber(out int changesetNumber)
        {
            changesetNumber = 0;

            string result = RunPlasticCommand("status --nochanges");
            if (result == null)
                return false;

            Match changesetMatch = Regex.Match(result, @"(?<=cs:)\d+");
            return changesetMatch.Success && int.TryParse(changesetMatch.Value, out changesetNumber);
        }

        public static string RunPlasticCommand(string args)
        {
            try
            {
                using Process p = new();
                p.StartInfo.UseShellExecute = false;
                
                // HACK: hi sorry idk what's up with my computer but i can't seem to find "cm" from within unity so here's how i solve that :)
                if (Environment.UserName == "Noe")
                    p.StartInfo.FileName = "/Applications/PlasticSCM.app/Contents/Applications/cm.app/Contents/MacOS/cm";
                else
                    p.StartInfo.FileName = "cm";
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.Start();

                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                return output;
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(RunPlasticCommand)} with args \'{args}\' threw an exception:\n{e.Message}");
                return null;
            }
        }
    }
}