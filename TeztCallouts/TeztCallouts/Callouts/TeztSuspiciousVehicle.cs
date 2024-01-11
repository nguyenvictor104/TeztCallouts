using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;

namespace TeztCallouts.Callouts
{
    [CalloutInfo("TeztSuspiciousVehicle", CalloutProbability.High)]


    internal class TeztSuspiciousVehicle : Callout
    {

        string pluginName = "TeztCallouts";

        private Ped Suspect;
        private Vehicle SuspectVehicle;
        private Blip SuspectBlip;
        private LHandle Pursuit;
        private Vector3 Spawnpoint;
        private bool PursuitCreated;

        public override bool OnBeforeCalloutDisplayed()
        {
            Spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(750f));
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "Suspicious Vehicle";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("INTRO_01 CITIZENS_REPORT_01 A_01 CRIME_DISTURBING_THE_PEACE_02 UNITS_RESPONSE_CODE_02_01 OUTRO_01", Spawnpoint);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            SuspectVehicle = new Vehicle("BALLER", Spawnpoint);
            SuspectVehicle.IsPersistent = true;

            Suspect = new Ped(SuspectVehicle.GetOffsetPositionFront(5f));
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.WarpIntoVehicle(SuspectVehicle, -1);

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.Color = System.Drawing.Color.Red;
            SuspectBlip.IsRouteEnabled = true;

            PursuitCreated = false;

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();

            if (!PursuitCreated && Game.LocalPlayer.Character.DistanceTo(SuspectVehicle) <= 30f)
            {
                if (SuspectBlip.Exists())
                {
                    SuspectBlip.Delete();
                }

                Pursuit = Functions.CreatePursuit();
                Functions.AddPedToPursuit(Pursuit, Suspect);
                Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                PursuitCreated = true;
            }




            if (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit))
            {
                End();
            }

            if (Suspect.IsDead)
            {
                End();
            }
        }

        public override void End()
        {
            base.End();

            if (Suspect.Exists())
            {
                Suspect.Dismiss();
            }

            if (SuspectBlip.Exists())
            {
                SuspectBlip.Delete();
            }

            if (SuspectVehicle.Exists())
            {
                SuspectVehicle.Dismiss();
            }

            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.LogTrivial("Suspicious Vehicle cleaned up");
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.DisplayNotification("Proceed back to patrol.");
        }
    }
}
