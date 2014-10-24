using Leap;
using System;

namespace AirCanvas
{
    class FrameListener : Listener
    {
        public event Action<Frame> FrameArrived;

        public override void OnFrame(Controller controller)
        {
            var h = FrameArrived;
            if (h != null)
            {
                using (var frame = controller.Frame())
                {
                    h(frame);
                }
            }
        }
    }
}
