using StereoKit;
using System;
using System.Collections.Generic;
using CHIPSZClassLibrary;
using System.Threading;

namespace CHIPSZ
{
    internal class Program
    {
        private static Countdown countdown;
        private static ProjectileGenerator ballGenerator;
        private static TargetGenerator targetGenerator;
        private static Floor floor;
		private static StartingScreen screen;

        private static PauseMenu pauseMenu;
        private static bool paused;

        public static Vec3 GetVelocity(Vec3 currentPos, Vec3 prevPos)
        {
            Vec3 result = (currentPos - prevPos) / Time.Elapsedf; ;
            return result;
        }
        public static double Magnitude(Vec3 velocity)
        {
            return Math.Sqrt((velocity.x * velocity.x) + (velocity.y * velocity.y) + (velocity.z * velocity.z));
        }

        static void Main(string[] args)
        {
            AudioManager audioManager = new AudioManager();

            // Initialize StereoKit
            SKSettings settings = new SKSettings
            {
                appName = "StereoKitProject1",
                assetsFolder = "Assets",
            };
            if (!SK.Initialize(settings))
                Environment.Exit(1);

            countdown = new Countdown(90); // sets the game duration to 90 seconds
            countdown.SetRunning(false);
            floor = new Floor();
			screen = new StartingScreen();

            // Create pause menu:
            pauseMenu = new PauseMenu();
            paused = false;

            ballGenerator = new ProjectileGenerator();
            targetGenerator = new TargetGenerator();
            TargetGenerator demoTargets = new TargetGenerator();


            GameTimer spawnBallTimer = new GameTimer(0.5);           

            // Core application loop
            //while (countdown.IsRunning() && SK.Step(() => // when the time runs out the app closes
            //booleans to switch between game and demo states
            bool closeForGame = screen.GetIfStartGame();
            bool closeForDemo = screen.GetIfStartDemo();

            bool tempFlipWaterFireSpawn = false;

            Hand hand = Input.Hand(Handed.Right);
            Vec3 handPreviousFrame;
            Vec3 scoreTextPos = new Vec3(-1.0f, 0.9f, -2.0f);
            while (countdown.GetDuration() > 0.0 && SK.Step(() => // when the time runs out the app closes
            {
                // Draw pause menu, check for input
                pauseMenu.Draw();
                paused = pauseMenu.GetPaused();
                if (paused)
                    countdown.SetRunning(false);
                
                if (!paused)
                {
                    handPreviousFrame = hand.palm.position;
                    hand = Input.Hand(Handed.Right);
                    spawnBallTimer.Update();
                    screen.Draw();
                    closeForGame = screen.GetIfStartGame();
                    closeForDemo = screen.GetIfStartDemo();


                    //Pose solidCurrentPose;
                    //GAME STATE:
                    if (closeForGame == false)
                    {
                        ballGenerator.ResetPlayerScore();
                        countdown.SetRunning(true);

                        hand.Solid = false;
                        if (SK.System.displayType == Display.Opaque)
                            Default.MeshCube.Draw(floor.GetMaterial(), floor.GetTransform());

                        if (Input.Key(Key.MouseRight).IsJustActive() || hand.IsJustGripped)
                        {
                            if (spawnBallTimer.elasped)
                            {
                                ballGenerator.SpawnProjectile(hand, Element.EARTH);
                                audioManager.Play("spawnBall", hand.palm.position, 1f);
                                spawnBallTimer.Reset();
                            }
                        }

                        else if (Input.Key(Key.F).IsJustActive() || (GetVelocity(hand.palm.position, handPreviousFrame).z < -3f && hand.gripActivation == 0))
                        {

                            if (spawnBallTimer.elasped)
                            {
                                if (tempFlipWaterFireSpawn)
                                {
                                    ballGenerator.SpawnProjectile(hand, Element.FIRE);
                                    audioManager.Play("spawnBall", hand.palm.position, 1f);
                                    spawnBallTimer.Reset();
                                    tempFlipWaterFireSpawn = false;
                                }

                                else
                                {
                                    ballGenerator.SpawnProjectile(hand, Element.WATER);
                                    audioManager.Play("spawnBall", hand.palm.position, 1f);
                                    spawnBallTimer.Reset();
                                    tempFlipWaterFireSpawn = true;
                                }

                            }
                        }

                        //Text.Add("Score :" + targetGenerator.targetsHit, Matrix.TRS(scoreTextPos, Quat.FromAngles(0, 180.0f, 0), 10.0f));
                        ballGenerator.Update(hand);
                        ballGenerator.Draw(false);
                        targetGenerator.Draw();
                        targetGenerator.CheckHit(ballGenerator.GetAllProjectiles(), ballGenerator, hand);
                    }
                    //DEMO STATE:
                    else if (closeForDemo == false)
                    {
                        if (SK.System.displayType == Display.Opaque)
                            Default.MeshCube.Draw(floor.GetMaterial(), floor.GetTransform());

                        if (screen.PlayDemo1() == true)
                        {
                            if (screen.PlayDemo2() == true)
                            {
                                screen.PlayDemo3();
                                demoTargets.Draw();
                                demoTargets.CheckHit(ballGenerator.GetAllProjectiles(), ballGenerator, hand);
                            }
                        }

                        if (Input.Key(Key.MouseRight).IsJustActive() || hand.IsJustGripped)
                        {
                            if (spawnBallTimer.elasped)
                            {
                                ballGenerator.SpawnProjectile(hand, Element.EARTH);
                                audioManager.Play("spawnBall", hand.palm.position, 1f);
                                spawnBallTimer.Reset();
                            }
                        }
                        else if (Input.Key(Key.F).IsJustActive() || GetVelocity(hand.palm.position, handPreviousFrame).z < -3f && hand.gripActivation == 0)
                        {
                            if (spawnBallTimer.elasped)
                            {
                                if (tempFlipWaterFireSpawn)
                                {
                                    ballGenerator.SpawnProjectile(hand, Element.FIRE);
                                    audioManager.Play("spawnBall", hand.palm.position, 1f);
                                    spawnBallTimer.Reset();
                                    tempFlipWaterFireSpawn = false;
                                }

                                else
                                {
                                    ballGenerator.SpawnProjectile(hand, Element.WATER);
                                    audioManager.Play("spawnBall", hand.palm.position, 1f);
                                    spawnBallTimer.Reset();
                                    tempFlipWaterFireSpawn = true;
                                }
                            }
                        }

                        ballGenerator.Update(hand);
                        ballGenerator.Draw(true);

                        if (screen.GetIfEndDemo())
                        {
                            screen.SetIfStartDemo(true);
                            screen.SetIfStartGame(false);
                        }
                    }
                }
                countdown.Update();
            })) ;
            SK.Shutdown();
        }

    }

}
