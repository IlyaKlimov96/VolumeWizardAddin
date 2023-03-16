using Aveva.ApplicationFramework;
using Aveva.ApplicationFramework.Presentation;
using Aveva.Core.Database;
using System.Configuration;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace VolumeWizardAddin
{
    public class VolumeWizardAddinStart : IAddin
    {
        public string Name => "VolumeWizardAddin";

        public string Description => "Аддон для создания объемов в моделе для выпуска чертежей";

        public void Start(ServiceManager serviceManager)
        {
            ICommandManager commandManager = DependencyResolver.GetImplementationOf<ICommandManager>();
            commandManager.Commands.Add(new VolumeWizardAddinStartCommand());
        }

        public void Stop() { }

    }

    public class VolumeWizardAddinStartCommand : Command
    {
        MainWindow _wpfMainWindow;
        Control _wfControl;
        DockedWindow _dockedWindow;

        public VolumeWizardAddinStartCommand()
        {
            this.Key = "VolumeWizardAddinStartCommand";
            IWindowManager windowManager = DependencyResolver.GetImplementationOf<IWindowManager>();

            _wfControl = new Control() { Dock = DockStyle.Fill };
            _wpfMainWindow = new MainWindow();
            _wfControl.Controls.Add(new ElementHost() { Dock = DockStyle.Fill, Child = _wpfMainWindow});

            _dockedWindow = windowManager.CreateDockedWindow("VolumeWizardMainWindow", "VolumeWizard", _wfControl, DockedPosition.Floating);
            _dockedWindow.SaveLayout = true;     
        }

        public override void Execute()
        {
            _dockedWindow.Show();
        }
    }
}