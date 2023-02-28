using StereoKit;
using System;
using System.Collections;
using System.Timers;

namespace CHIPSZ
{
    internal class Program
    {
        private static Countdown countdown;
        private static BallGenerator ballGenerator;
        private static Floor floor;
		private static starting_screen screen;
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
            countdown.setRunning(false);
            ArrayList targets = new ArrayList();
            floor = new Floor();
			screen = new starting_screen();

            /*Widget widget = new Widget();
            widget.setSlider(0.5f);

            Widget highScores = new Widget();
            highScores.setWindowName("highScoreWindow");
            highScores.setPosition(new Pose(.4f, 0, .4f, Quat.LookDir(-1, 0, 1)));

            Widget welcome = new Widget();
            welcome.setWindowName("Welcome");
            welcome.setPosition(new Pose(-.4f, 0, .4f, Quat.LookDir(1, 0, 1)));
            welcome.addButton("Start Game");
            welcome.addButton("Start Demo");*/


            for (int i = 0; i < 10; i++) {
                targets.Add(new Target());
                Target target = (Target)targets[i];
                target.setDefaultShape();
                target.setRandomPose();
            }
            ballGenerator = new BallGenerator();


            // Core application loop
            //while (countdown.IsRunning() && SK.Step(() => // when the time runs out the app closes
            while (countdown.getDuration() > 0.0 && SK.Step(() => // when the time runs out the app closes
            {
                // Draw Basic Widget
                /*widget.draw();
                highScores.drawHighScores();
                welcome.draw();*/

                screen.Draw();
                //Pose solidCurrentPose;
                bool close = screen.getIfClose();
                if (close == false) {
                    countdown.setRunning(true);
                    Hand hand = Input.Hand(Handed.Right);
                    if (SK.System.displayType == Display.Opaque)
                        Default.MeshCube.Draw(floor.getMaterial(), floor.getTransform());

                    if (Input.Key(Key.MouseRight).IsJustActive() || hand.IsJustGripped)
                    {
                        ballGenerator.add(hand);
                        audioManager.Play("cymbalCrash2Second");
                    }
                    ballGenerator.draw(hand);
                    foreach (Target target in targets) {
                        target.draw();
                        target.checkHit(ballGenerator.getAllBalls());
                    };
                }
                countdown.Update();
            })) ;
            SK.Shutdown();
        }
        
        
    }

}
