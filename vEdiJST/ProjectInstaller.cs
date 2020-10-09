using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace EM
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public const string SC_NAME = "JSTEDI";//"WebServiceHost";
        private const string SC_DESC = "JSTEDI";//"Host server WebService TKLeasing.";

        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
