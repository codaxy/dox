using System;
using Codaxy.Dextop;
using Codaxy.Dextop.Forms;

namespace Codaxy.Dox.Viewer.Windows
{
    public class SimpleWindow : DextopWindow
    {
        public override void InitRemotable(DextopRemote remote, DextopConfig config)
        {
            base.InitRemotable(remote, config);
            Remote.RemoteHostType = "Codaxy.Dox.SimpleWindow";
        }

    }
}