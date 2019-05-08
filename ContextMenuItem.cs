using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.VersionControl;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace EzProjectLoad
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ContextMenuItem
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0432f43a-f023-4962-ab9a-56ceb2e44244");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItem"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ContextMenuItem(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            //HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\14.0_Config\ToolWindows\{e77209ba-064a-4625-b8ce-dfd1d7967cd1}
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ContextMenuItem Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ContextMenuItem's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ContextMenuItem(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;

            VersionControlExt versionControlExt =
                dte.GetObject("Microsoft.VisualStudio.TeamFoundation.VersionControl.VersionControlExt")
               as VersionControlExt;

            VersionControlExplorerExt versionControlExplorerExt = versionControlExt.Explorer;
            //writeObjectToFile(versionControlExplorerExt.SelectedItems);

            if (versionControlExplorerExt.CurrentFolderItem.LocalPath == null)
                //TODO: Implement get latest version when files are missing in your local version.
                return;//updateFolder(versionControlExplorerExt.Workspace.VersionControlServer, versionControlExplorerExt.CurrentFolderItem.SourceServerPath);

            VersionControlExplorerItem project = this.getProjectFileName(versionControlExplorerExt.SelectedItems);

            //return if csproj file is not in the selection.
            if (project == null)
                return;

            this.addProjectToSolution(dte.Solution, project.LocalPath);
        }

        /// <summary>
        /// Gets the filename including the path of the first C# project in the array of VersionControlExplorerItem.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>
        /// The filename including the path of the first C# project in the array of VersionControlExplorerItem
        /// or empty if there is no csproj file present in the array of VersionControlExplorerItem.
        /// </returns>
        private VersionControlExplorerItem getProjectFileName(VersionControlExplorerItem[] items)
        {
            for(int i = 0; i<items.Length; i++)
            {
                if (isFileExtensionOf(items[i].SourceServerPath, "csproj"))
                    return items[i];
            }

            return null;
        }

        private void addProjectToSolution(Solution sln, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            sln.AddFromFile(fileName);
        }

        /// <summary>
        ///  Updates all the items of the folder.
        /// </summary>
        /// <param name="sourceControl"></param>
        /// <param name="serverPath"></param>
        //private void updateFolder(VersionControlServer sourceControl, string serverPath)
        //{

        //    var items = sourceControl.GetItems(serverPath);

        //    foreach(Item item in items.Items) {
        //        item.DownloadFile();
        //    }
        //}

        /// <summary>
        /// Returns the parent forder of a given item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //private string getFolderServerPath(VersionControlExplorerItem item)
        //{
        //    return item.SourceServerPath;
        //}

        //private void getFiles(VersionControlExplorerItem item)
        //{

            
        //}

        #region Misc methods
        private string getExtensionName(string fileNameFullPath)
        {
            //Split the path, last element will contain the file name.
            string[] splittedPath = fileNameFullPath.Split('/');

            //Split file name, last element will contain the file extension.
            string[] splittedFilename = splittedPath[splittedPath.Length - 1].Split('.');

            return splittedFilename[splittedFilename.Length - 1];
        }

        private bool isFileExtensionOf(string fileName, string extension)
        {
            return getExtensionName(fileName) == extension;
        }

        //private void writeObjectToFile(object obj)
        //{
        //    string text = JsonConvert.SerializeObject(obj, Formatting.Indented);
        //    System.IO.File.WriteAllText($@"C:\Users\Jose\Desktop\{nameof(obj)}.txt", text);
        //}
        #endregion
    }
}
