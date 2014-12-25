using KLibrary.ComponentModel;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace BodyTracker
{
    public class AppModel : NotifyBase
    {
        const int ScreenWidth = 1920;
        const int ScreenHeight = 1080;
        const int MappingScale = 1000;

        KinectSensor sensor;
        Skeleton[] skeletons;

        public Point3D[] Positions
        {
            get { return GetValue<Point3D[]>(); }
            private set { SetValue(value); }
        }

        public AppModel()
        {
            if (KinectSensor.KinectSensors.Count == 0) return;

            sensor = KinectSensor.KinectSensors[0];
            sensor.SkeletonFrameReady += sensor_SkeletonFrameReady;
            sensor.SkeletonStream.Enable();
            // Smoothing
            //sensor.SkeletonStream.Enable(new TransformSmoothParameters
            //{
            //    Correction = 0.5f,
            //    JitterRadius = 0.05f,
            //    MaxDeviationRadius = 0.04f,
            //    Prediction = 0.0f,
            //    Smoothing = 0.5f,
            //});

            skeletons = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            sensor.Start();
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            sensor.Stop();
        }

        void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var frame = e.OpenSkeletonFrame())
            {
                if (frame == null)
                {
                    Positions = null;
                    return;
                }

                frame.CopySkeletonDataTo(skeletons);

                var skeleton = skeletons
                    .Where(s => s.TrackingState == SkeletonTrackingState.Tracked)
                    .OrderBy(s => s.Position.Z)
                    .FirstOrDefault();
                if (skeleton == null)
                {
                    Positions = null;
                    return;
                }

                Positions = skeleton.Joints
                    .Where(j => j.TrackingState == JointTrackingState.Tracked)
                    .Select(j => ToScreenPoint(j.Position))
                    .ToArray();
            }
        }

        static Point3D ToScreenPoint(SkeletonPoint p)
        {
            return new Point3D(ScreenWidth / 2 + MappingScale * p.X, ScreenHeight / 2 - MappingScale * p.Y, MappingScale * p.Z);
        }
    }
}
