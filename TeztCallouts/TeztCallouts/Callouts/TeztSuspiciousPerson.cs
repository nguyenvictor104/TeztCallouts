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
    [CalloutInfo("TeztSuspiciousPerson", CalloutProbability.High)]


    internal class TeztSuspiciousPerson : Callout
    {

        string pluginName = "TeztCallouts";

        private bool debug = true;
        private int scenario = 0;
        private Ped Suspect;
        private Blip SuspectBlip;
        private Vector3 Spawnpoint;
        private LHandle Pursuit;
        private bool PursuitCreated;
    
        private string[] weaponList = { "WEAPON_PISTOL", "WEAPON_MINISMG", "WEAPON_KNIFE", "WEAPON_KNUCKLE", "WEAPON_BAT", "WEAPON_HAMMER", "WEAPON_BOTTLE" };
        int weaponSelect = 0;

        public override bool OnBeforeCalloutDisplayed()
        {
            
            Spawnpoint = World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(500f));
            ShowCalloutAreaBlipBeforeAccepting(Spawnpoint, 30f);
            AddMinimumDistanceCheck(30f, Spawnpoint);
            CalloutMessage = "Suspicious Person";
            CalloutPosition = Spawnpoint;
            Functions.PlayScannerAudioUsingPosition("INTRO_01 CITIZENS_REPORT_01 A_01 CRIME_DISTURBING_THE_PEACE_02 UNITS_RESPOND_CODE_03_01 OUTRO_01", Spawnpoint);

            
            weaponSelect = new Random().Next(0, 6);
            scenario = new Random().Next(0, 2);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {

            Suspect = new Ped(Spawnpoint);
            Suspect.IsPersistent = true;
            Suspect.BlockPermanentEvents = true;
            Suspect.Tasks.Wander();

            SuspectBlip = Suspect.AttachBlip();
            SuspectBlip.Color = System.Drawing.Color.Red;
            SuspectBlip.IsRouteEnabled = true;
            
            return base.OnCalloutAccepted();
        }

        public override void OnCalloutNotAccepted()
        {
            if (SuspectBlip) SuspectBlip.Delete();
            if (Suspect) Suspect.Delete();

            base.OnCalloutNotAccepted();
        }

        public override void Process()
        {
            

            if (Game.LocalPlayer.Character.DistanceTo(Suspect) <= 30f)
            {
                if (SuspectBlip.Exists())
                {
                    SuspectBlip.IsRouteEnabled = false;
                }

                switch (scenario)
                {
                    
                    case 1:
                        
                        Suspect.Inventory.GiveNewWeapon(weaponList[weaponSelect], 30, true);
                        Suspect.Tasks.FightAgainst(Game.LocalPlayer.Character);
                        if (debug)
                        {
                            Game.DisplayNotification("Suspect.FightAgainst");
                        }
                        break;
                    
                    case 2:
                        Suspect.Tasks.ReactAndFlee(Game.LocalPlayer.Character);

                        Pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(Pursuit, Suspect);
                        Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                        PursuitCreated = true;

                        if (debug)
                        {
                            Game.DisplayNotification("Suspect.ReactAndFlee");
                        }
                        break;

                    default:
                        Suspect.Tasks.Cower(-1);
                        Game.DisplaySubtitle("Use STP to investigate the suspect.");
                        if (debug)
                        {
                            Game.DisplayNotification("Suspect.Default");
                        }
                        break;
                }

            }

            if (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit))
            {
                End();
            }

            if (Suspect.IsDead || Suspect.IsCuffed || Game.LocalPlayer.Character.IsDead || !Suspect.Exists())
            {
                End();
            }

            base.Process();
        }

        public override void End()
        {
            

            if (Suspect.Exists())
            {
                Suspect.Tasks.Clear();
                Suspect.Dismiss();
            }

            if (SuspectBlip.Exists())
            {
                SuspectBlip.Delete();
            }

            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.LogTrivial("Suspicious Person cleaned up");
            Game.LogTrivial(string.Format("=============================================== {0} ===============================================", pluginName));
            Game.DisplayNotification("Proceed back to patrol.");
            Functions.PlayScannerAudioUsingPosition("INTRO_01 OUTRO_01", Spawnpoint);

            base.End();

        }
    }
}
