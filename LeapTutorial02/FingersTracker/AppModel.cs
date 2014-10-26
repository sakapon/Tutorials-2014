﻿using KLibrary.ComponentModel;
using Leap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace FingersTracker
{
    public class AppModel : NotifyBase
    {
        const int ScreenWidth = 1920;
        const int ScreenHeight = 1080;
        const int MappingScale = 3;

        Controller controller;
        FrameListener listener;

        public Point3D[] Positions
        {
            get { return GetValue<Point3D[]>(); }
            private set { SetValue(value); }
        }

        public AppModel()
        {
            controller = new Controller();
            listener = new FrameListener();
            controller.AddListener(listener);

            listener.FrameArrived += listener_FrameArrived;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            controller.RemoveListener(listener);
            listener.Dispose();
            controller.Dispose();
        }

        void listener_FrameArrived(Leap.Frame frame)
        {
            Positions = frame.Pointables
                .Where(p => p.IsValid)
                .Where(p => p.StabilizedTipPosition.IsValid())
                .Select(p => ToStylusPoint(p.StabilizedTipPosition))
                .ToArray();
        }

        static Point3D ToStylusPoint(Leap.Vector v)
        {
            return new Point3D(ScreenWidth / 2 + MappingScale * v.x, ScreenHeight - MappingScale * v.y, MappingScale * v.z);
        }
    }
}