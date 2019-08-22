namespace QueryPortalSdkCore
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Xpertdoc.Portal.SdkCore;

    class Program
    {
        static async Task Main(string[] args)
        {
            // Creates a context using Windows Authentication
            var context = CreateWindowsContext("SERVERURL");

            // Or you can create a context using Forms Authentication if Xpertdoc Portal is configured this way.
            //var context = CreateFormsContext("SERVERURL", "user", "password");

            await ExecuteTemplate(context, "TemplateLibraryName", "TemplateGroupName", "TemplateName");

            var file = await FindFileInContentManager(context, "ContentLibraryName", "ParentFolderName", "FileName");
            await CheckOutFile(context, file);
            await CheckInFile(context, file, new byte[] { 0 });
        }

        /// <summary>
        /// Creates the windows context.
        /// </summary>
        /// <param name="portalUrl">The portal URL.</param>
        /// <returns>
        /// The context.
        /// </returns>
        private static PortalODataContext CreateWindowsContext(string portalUrl)
        {
            var context = new PortalODataContext(portalUrl);
            context.Credentials = CredentialCache.DefaultCredentials;

            return context;
        }

        /// <summary>
        /// Creates the forms context.
        /// </summary>
        /// <param name="portalUrl">The portal URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// The context.
        /// </returns>
        private static PortalODataContext CreateFormsContext(string portalUrl, string username, string password)
        {
            return new PortalODataContext(portalUrl, username, password);
        }

        /// <summary>
        /// Executes the template.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="templateLibraryName">Name of the template library.</param>
        /// <param name="templateGroupName">Name of the template group.</param>
        /// <param name="templateName">Name of the template.</param>
        private static async Task ExecuteTemplate(PortalODataContext context, string templateLibraryName, string templateGroupName, string templateName)
        {
            var templateLibrary = await context.TemplateLibraries.Where(tl => tl.Name == templateLibraryName).FirstAsync();
            var templateGroup = await context.TemplateGroups.Where(tg => tg.TemplateLibraryId == templateLibrary.TemplateLibraryId && tg.Name == templateGroupName).FirstAsync();
            var template = await context.Templates.Where(t => t.TemplateGroupId == templateGroup.TemplateGroupId && t.Name == templateName).FirstAsync();
            var templateExecutionInfo = await template.Execute("XML Payload or Any Execution Data", null, null).GetValueAsync();

            if (templateExecutionInfo.Success)
            {
                var templateExecutionResult = await context.TemplateExecutionResults.Where(ter => ter.TemplateExecutionId == templateExecutionInfo.TemplateExecutionId).FirstAsync();
                var documentContent = await templateExecutionResult.GetContent().GetValueAsync();

                // Save file to disk / Send to other services.
            }
            else
            {
                // Throw exception / Display error message.
            }
        }

        /// <summary>
        /// Finds a file in content manager.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="contentLibraryName">Name of the content library.</param>
        /// <param name="parentFolderName">Name of the parent folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        /// The content library file that was found.
        /// </returns>
        private static async Task<ContentLibraryFile> FindFileInContentManager(PortalODataContext context, string contentLibraryName, string parentFolderName, string fileName)
        {
            var contentLibrary = await context.ContentLibraries.Where(cl => cl.Name == contentLibraryName).FirstAsync();
            var folder = await context.ContentLibraryFolders.Where(clf => clf.ContentLibraryId == contentLibrary.ContentLibraryId && clf.Name == parentFolderName).FirstAsync();
            var file = await context.ContentLibraryFiles.Where(clf => clf.ContentLibraryId == contentLibrary.ContentLibraryId && clf.Name == fileName && clf.ParentContentLibraryFolderId == folder.ContentLibraryFolderId).FirstAsync();

            var fileContent = await file.GetContent().GetValueAsync();

            // Save file content to disk / Send to other services.

            return file;
        }

        /// <summary>
        /// Check out the the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="file">The file to check out.</param>
        private static async Task CheckOutFile(PortalODataContext context, ContentLibraryFile file)
        {
            await file.CheckOut().ExecuteAsync();
        }

        /// <summary>
        /// Check in the file.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="file">The file to check in.</param>
        /// <param name="updatedFileContent">The updated content of the file.</param>
        private static async Task CheckInFile(PortalODataContext context, ContentLibraryFile file, byte[] updatedFileContent)
        {
            await file.UpdateContent(updatedFileContent).ExecuteAsync();
            await file.CheckIn().ExecuteAsync();
        }
    }
}
